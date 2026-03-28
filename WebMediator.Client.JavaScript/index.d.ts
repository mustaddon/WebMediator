
export declare type WebMediatorResponse = {
    type?: string,
    data?: any,
}

export declare class WebMediatorClient {
    constructor(endpointUrl: string, requestInit?: RequestInit | undefined);

    private readonly _endpointUrl;
    private readonly _requestInit;

    public send(type: string): Promise<WebMediatorResponse>;
    public send(type: string, data?: any): Promise<WebMediatorResponse>;

    public getLink(type: string) : string;
    public getLink(type: string, data?: any) : string;
}