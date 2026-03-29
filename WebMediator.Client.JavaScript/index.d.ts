export declare interface WebMediatorRequest { 
    type: string,
    data?: any,
}

export declare interface WebMediatorResponse extends WebMediatorRequest { }

export declare class WebMediatorClient {
    constructor(endpointUrl: string, requestInit?: RequestInit | undefined);

    private readonly _endpointUrl;
    private readonly _requestInit;

    public send(type: string): Promise<WebMediatorResponse>;
    public send(type: string, data?: any): Promise<WebMediatorResponse>;
    public send(request: WebMediatorRequest): Promise<WebMediatorResponse>;

    public getLink(type: string) : string;
    public getLink(type: string, data?: any) : string;
    public getLink(request: WebMediatorRequest) : string;
}