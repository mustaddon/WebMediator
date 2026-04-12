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
            const sse = { event: 'message', data: null, lastEventId };

            for (let x of chunk.split('\n'))
                if(x.startsWith('event: '))
                    sse.event = x.substring(7);
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

async function safe(promise) {
    try{
        return { value: await promise, error: null };
    }
    catch(e) {
        return { value: null, error: e };
    }
}

/**
 * @param {function(): Promise<AsyncGenerator<MessageEvent>>} sseFactory
 * @returns {AsyncGenerator<MessageEvent>}
 */
async function* eventStream(sseFactory){
    while(true){
        const generator = await safe(sseFactory());

        if(!generator.error)
            while (true) {
                const item = await safe(generator.value.next());
                if (item.error || item.value.done) break;
                yield item.value.value;
            }

        await new Promise(resolve => setTimeout(resolve, 3000));
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

export class WebMediatorClient
{
    constructor(endpointUrl, requestInit) {
        this._endpointUrl = endpointUrl;
        this._requestInit = requestInit ?? {};

        if(endpointUrl[endpointUrl.length-1] != '/')
            this._endpointUrl=this._endpointUrl+'/';
    }

    getUrl(type, data)
    {
        if(!type)
            throw new Error('The type must not be empty.');

        if(data === undefined && typeof(type) != typeof(''))
            return this.getUrl(type.type, type.data);

        if(data === undefined)
            return `${this._endpointUrl}${type}`;

        if(data instanceof Blob || getBlobProperty(data))
            throw new Error('The data contains Blob property and cannot be presented as a link.');

        return `${this._endpointUrl}${type}?data=${encodeURIComponent(JSON.stringify(data))}`;
    }

    eventStream(type, data)
    {
        return eventStream(() => this.__send(type, data, (res, controller) => {
            if(res.headers.get('content-type') != 'text/event-stream')
                throw new Error('The response does not contain "text/event-stream".');

            return eventStreamBase(res, controller);
        }));
    }

    send(type, data) {
        return this.__send(type, data, (r,c) => getResult(r, c));
    }

    __send(type, data, then) 
    {
        if(!type)
            throw new Error('The type must not be empty.');

        if(data === undefined && typeof(type) != typeof(''))
            return this.__send(type.type, type.data);

        const controller = new AbortController();
        const requestInit = {
            ... this._requestInit, 
            signal: controller.signal, 
            method: 'POST' 
        };

        if(data === undefined)
            return fetch(this._endpointUrl+type, requestInit).then(r => then(r, controller));

        if(data instanceof Blob)
            return fetch(this._endpointUrl+type, { ... requestInit, body: data }).then(r => then(r, controller));

        const blobProp = getBlobProperty(data);

        if(blobProp)
        {
            let copy = Object.assign({}, data);
            delete copy[blobProp];

            return fetch(`${this._endpointUrl}${type}?data=${encodeURIComponent(JSON.stringify(copy))}`, {
                ... requestInit,
                body: data[blobProp]
            }).then(r => then(r, controller));
        }

        return fetch(this._endpointUrl+type, {
            ... requestInit,
            body: JSON.stringify(data)
        }).then(r => then(r, controller));
    }
}

export default { WebMediatorClient };