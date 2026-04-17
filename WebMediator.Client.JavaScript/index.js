export class WebMediatorClient
{
    /**
     * @param {string} endpointUrl
     * @param {RequestInit} [requestInit]
     */
    constructor(endpointUrl, requestInit) {
        this._endpointUrl = endpointUrl;
        this._requestInit = requestInit ?? {};

        if(endpointUrl[endpointUrl.length-1] != '/')
            this._endpointUrl=this._endpointUrl+'/';
    }

    /**
     * @param {string} type
     * @param {any} [data]
     * @param {SendOptions} [options]
     * @returns {string}
     */
    getUrl(type, data)
    {
        if(this.__hasRequestArgument(...arguments))
            return this.getUrl(arguments[0].type, arguments[0].data);

        this.__checkArguments(...arguments);

        if(data === undefined)
            return `${this._endpointUrl}${type}`;

        if(data instanceof Blob || getBlobProperty(data))
            throw new WebMediatorError('The data contains Blob property and cannot be presented as a link.');

        return `${this._endpointUrl}${type}?data=${encodeURIComponent(JSON.stringify(data))}`;
    }
    
    /**
     * @typedef {Object} EventStreamOptions
     * @property {number} [reconnectionDelay]
     * @property {number} [reconnectionRetriesLimit]
     */
    /**
     * @param {string} type
     * @param {any} [data]
     * @param { SendOptions & EventStreamOptions } [options]
     * @returns {AsyncGenerator<MessageEvent, void, unknown>}
     */
    eventStream(type, data, options)
    {
        if(this.__hasRequestArgument(...arguments))
            return this.eventStream(arguments[0].type, arguments[0].data, arguments[1], arguments[2]);

        this.__checkArguments(...arguments);
        
        const controller = this.__createAbortController(options);

        return eventStream(() => this.__send(type, data, controller.signal)
            .then(res => {
                if(res.headers.get('content-type') != 'text/event-stream')
                {
                    controller.abort();
                    throw new SseError(`The response does not contain 'text/event-stream'. (RequestURL: ${res.url})`);
                }

                return eventStreamBase(res, this.__createAbortController(options));
            }), {
                reconnectionDelay: 3000,
                ... options, 
                signal: controller.signal
            });
    }


    /**
     * @typedef {Object} SendOptions
     * @property {AbortSignal} [signal]
     */
    /**
     * @param {string} type
     * @param {any} [data]
     * @param {SendOptions} [options]
     * @returns {Promise}
     */
    send(type, data, options) {
        if(this.__hasRequestArgument(...arguments))
            return this.send(arguments[0].type, arguments[0].data, arguments[1], arguments[2]);

        this.__checkArguments(...arguments);

        const controller = this.__createAbortController(options);

        return this.__send(type, data, controller.signal).then(r => getResult(r, controller));
    }

    __createAbortController(options){
        const controller = new AbortController();
        if(options?.signal)
            options.signal.addEventListener('abort', e => controller.abort(new AbortError(e.target.reason)));
        return controller;
    }

    __checkArguments(type) {
        if(!type) throw new WebMediatorError('The type must not be empty.');
        return true;
    }

    __hasRequestArgument(type) {
        return type && typeof(type) != typeof('') && typeof(type.type) == typeof('');
    }

    __send(type, data, signal) 
    {
        const requestInit = {
            ... this._requestInit, 
            signal: signal, 
            method: 'POST' 
        };

        const checkStatus = r => {
            if(!r.ok) throw new WebMediatorError(`Response status is ${r.status}. (RequestUrl: ${r.url})`);
            return r;
        };

        if(data === undefined)
            return fetch(this._endpointUrl+type, requestInit).then(checkStatus);

        if(data instanceof Blob)
            return fetch(this._endpointUrl+type, { ... requestInit, body: data }).then(checkStatus);

        const blobProp = getBlobProperty(data);

        if(blobProp)
        {
            let copy = Object.assign({}, data);
            delete copy[blobProp];

            return fetch(`${this._endpointUrl}${type}?data=${encodeURIComponent(JSON.stringify(copy))}`, {
                ... requestInit,
                body: data[blobProp]
            }).then(checkStatus);
        }

        return fetch(this._endpointUrl+type, {
            ... requestInit,
            body: JSON.stringify(data)
        }).then(checkStatus);
    }
}

export class WebMediatorError extends Error { }

export class SseError extends WebMediatorError { }

export class AbortError extends WebMediatorError { 
    constructor(reason, options) {
        super(reason?.message || reason || 'aborted without reason', options)
    }
}

export default { WebMediatorClient };




function getBlobProperty(data)
{
    if(data && !Array.isArray(data))
    {
        for(let p in data) 
            if(data[p] instanceof Blob)
                return p;
    }

    return null;
}


/**
 * @param {AsyncGenerator<MessageEvent>} sseGenerator
 * @returns {AsyncGenerator<any>}
 */
async function* asyncStream(sseGenerator)
{
    for await (const sse of sseGenerator) 
        yield sse.data;
}


/**
 * @param {Response} response
 * @param {AbortController} controller
 * @returns {AsyncGenerator<MessageEvent>}
 */
async function* eventStreamBase(response, controller) {
    try {
        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let lastEventId = null;
        let sse = { type: 'message', data: undefined, lastEventId };
        
        while (true) {
            const { value, done } = await reader.read();
            if (done) break;
            
            const chunk = decoder.decode(value, { stream: true });

            for (let x of chunk.split('\n'))
                if(x.startsWith('event: '))
                    sse.type = x.substring(7);
                else if(x.startsWith('data: '))
                    sse.data = JSON.parse(x.substring(6));
                else if(x.startsWith('id: '))
                    sse.lastEventId = lastEventId = x.substring(4);
                else if(!x && sse.data !== undefined)
                {
                    yield sse;
                    sse = { type: 'message', data: undefined, lastEventId };
                }
        }
    } finally {
        controller.abort();
    }
}

/**
 * @param {function(): Promise} promiseFac
 * @returns {Promise<{value, error: Error | null}>}
 */
async function safe(promiseFac) {
    try{
        return { value: await promiseFac(), error: null };
    }
    catch(e) {
        return { value: null, error: e };
    }
}


/**
 * @param {function(): Promise<AsyncGenerator<MessageEvent>>} sseFactory
 * @param { SendOptions & EventStreamOptions } options
 * @returns {AsyncGenerator<MessageEvent>}
 */
async function* eventStream(sseFactory, options){
    let reconnections = 0;

    while(true){
        const generator = await safe(sseFactory);
        
        if(!generator.error)
        {
            while (true) {
                const item = await safe(() => generator.value.next());
                if (item.error || item.value.done) break;
                yield item.value.value;
            }
        }
        else if(generator.error instanceof SseError || generator.error instanceof AbortError)
        {
            throw generator.error;
        }
        else if(generator.error instanceof DOMException && ['AbortError', 'TimeoutError'].indexOf(generator.error.name) >= 0)
        {
            throw new AbortError(generator.error);
        }

        if(options.signal?.aborted)
            throw new AbortError(options.signal.reason);

        if(options.reconnectionRetriesLimit != null && reconnections >= options.reconnectionRetriesLimit)
            throw new SseError(`The limit on the number of reconnection attempts has been reached.`);
        
        if(options.reconnectionDelay > 0)
            await new Promise(resolve => setTimeout(resolve, options.reconnectionDelay));

        reconnections++;
    }
}



/**
 * @param {Response} res
 * @param {AbortController} controller
 */
async function getResult(res, controller)
{
    let result = {
        type: res.headers.get('data-type') || undefined,
    };

    if(res.status == 204)
        return result;
    
    if(!res.ok)
        throw new WebMediatorError(`Response status is ${res.status} (RequestUrl: ${res.url})`);

    let contentType = res.headers.get('content-type') ?? '';

    if(contentType == 'text/event-stream')
    {
        result.data = asyncStream(eventStreamBase(res, controller));
    }
    else if(contentType.indexOf('application/json') < 0)
    {
        let blobProp = res.headers.get('data-stream-property');

        if(blobProp)
        {
            result.data = JSON.parse(atob(res.headers.get('data')));
            result.data[blobProp] = await res.blob();
        }
        else
        {
            result.data = await res.blob();
        }
    }
    else
    {
        result.data = await res.json();
    }
    
    return result;
}