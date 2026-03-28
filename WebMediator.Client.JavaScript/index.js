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

async function getResult(res)
{
    let result = {
        type: res.headers.get('data-type') || undefined,
    };

    if(res.status == 204)
        return result;

    let contentType = res.headers.get('content-type') ?? '';

    if(contentType.indexOf('application/json') < 0)
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

    getLink(type, data)
    {
        if(data === undefined)
            return `${this._endpointUrl}${type}`;

        if(data instanceof Blob || getBlobProperty(data))
            throw new Error('The data contains Blob property and cannot be presented as a link.');

        return `${this._endpointUrl}${type}?data=${encodeURIComponent(JSON.stringify(data))}`;
    }

    send(type, data) 
    {
        if(data === undefined)
            return fetch(this._endpointUrl+type, {... this._requestInit, method: 'POST' }).then(getResult);

        if(data instanceof Blob)
            return fetch(this._endpointUrl+type, {... this._requestInit, method: 'POST', body: data }).then(getResult);

        const blobProp = getBlobProperty(data);

        if(blobProp)
        {
            let copy = Object.assign({}, data);
            delete copy[blobProp];

            return fetch(`${this._endpointUrl}${type}?data=${encodeURIComponent(JSON.stringify(copy))}`, {
                ... this._requestInit,
                method: 'POST',
                body: data[blobProp]
            }).then(getResult);
        }

        return fetch(this._endpointUrl+type, {
            ... this._requestInit,
            method: 'POST',
            body: JSON.stringify(data)
        }).then(getResult);
    }
}

export default { WebMediatorClient };