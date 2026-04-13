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
     * @returns {string}
     */
    getUrl(type, data)
    {
        if(!type)
            throw new WebMediatorError('The type must not be empty.');

        if(data === undefined && typeof(type) != typeof(''))
            return this.getUrl(type.type, type.data);

        if(data === undefined)
            return `${this._endpointUrl}${type}`;

        if(data instanceof Blob || getBlobProperty(data))
            throw new WebMediatorError('The data contains Blob property and cannot be presented as a link.');

        return `${this._endpointUrl}${type}?data=${encodeURIComponent(JSON.stringify(data))}`;
    }
    
    /**
     * @param {string} type
     * @param {any} [data]
     * @param {number} [reconnectionDelay]
     * @param {number} [reconnectionRetriesLimit]
     * @returns {AsyncGenerator<MessageEvent, void, unknown>}
     */
    eventStream(type, data, reconnectionDelay, reconnectionRetriesLimit)
    {
        return eventStream(() => this.__send(type, data, 
            (res, controller) => {
                if(res.headers.get('content-type') != 'text/event-stream')
                {
                    controller.abort();
                    throw new SseError(`The response does not contain 'text/event-stream'. (RequestURL: ${res.url})`);
                }

                return eventStreamBase(res, controller);
            }), { 
                reconDelay: reconnectionDelay ?? 3000, 
                reconRetries: reconnectionRetriesLimit
            });
    }

    /**
     * @param {string} type
     * @param {any} [data]
     * @returns {Promise}
     */
    send(type, data) {
        return this.__send(type, data, getResult);
    }

    __send(type, data, next) 
    {
        if(!type)
            throw new WebMediatorError('The type must not be empty.');

        if(data === undefined && typeof(type) != typeof(''))
            return this.__send(type.type, type.data);

        const controller = new AbortController();
        const requestInit = {
            ... this._requestInit, 
            signal: controller.signal, 
            method: 'POST' 
        };

        const goNext = r => {
            if(!r.ok) throw new WebMediatorError(`Response status is ${r.status}. (RequestUrl: ${r.url})`);
            return next(r, controller)
        };

        if(data === undefined)
            return fetch(this._endpointUrl+type, requestInit).then(goNext);

        if(data instanceof Blob)
            return fetch(this._endpointUrl+type, { ... requestInit, body: data }).then(goNext);

        const blobProp = getBlobProperty(data);

        if(blobProp)
        {
            let copy = Object.assign({}, data);
            delete copy[blobProp];

            return fetch(`${this._endpointUrl}${type}?data=${encodeURIComponent(JSON.stringify(copy))}`, {
                ... requestInit,
                body: data[blobProp]
            }).then(goNext);
        }

        return fetch(this._endpointUrl+type, {
            ... requestInit,
            body: JSON.stringify(data)
        }).then(goNext);
    }
}

export class WebMediatorError extends Error { }

export class SseError extends WebMediatorError { }

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
        
        while (true) {
            const { value, done } = await reader.read();
            if (done) break;
            
            const chunk = decoder.decode(value, { stream: true });
            const sse = { type: 'message', data: null, lastEventId };

            for (let x of chunk.split('\n'))
                if(x.startsWith('event: '))
                    sse.type = x.substring(7);
                else if(x.startsWith('data: '))
                    sse.data = JSON.parse(x.substring(6));
                else if(x.startsWith('id: '))
                    sse.lastEventId = lastEventId = x.substring(4);
                
            yield sse;
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
 * @param { ({reconDelay?: number, reconRetries?: number}) } options
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
        else if(generator.error instanceof SseError)
        {
            throw generator.error;
        }

        if(options?.reconRetries != null && reconnections >= options.reconRetries)
            throw new SseError(`The limit on the number of reconnection attempts has been reached.`);
        
        if(options?.reconDelay > 0)
            await new Promise(resolve => setTimeout(resolve, options.reconDelay));

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