export declare interface WebMediatorRequest { 
    type: string,
    data?: any,
}

export declare interface WebMediatorResponse extends WebMediatorRequest { }

export declare class WebMediatorClient {
    constructor(endpointUrl: string, requestInit?: RequestInit | undefined);

    private readonly _endpointUrl;
    private readonly _requestInit;

    public getUrl(type: string) : string;
    public getUrl(type: string, data?: any) : string;
    public getUrl(request: WebMediatorRequest) : string;

    public send(type: string): Promise<WebMediatorResponse>;
    public send(type: string, data?: any): Promise<WebMediatorResponse>;
    public send(request: WebMediatorRequest): Promise<WebMediatorResponse>;

    public eventStream(type: string): AsyncGenerator<MessageEvent, void, unknown>;
    public eventStream(type: string, data?: any, reconnectionDelay?: number, reconnectionRetriesLimit?: number): AsyncGenerator<MessageEvent, void, unknown>;
    public eventStream(request: WebMediatorRequest): AsyncGenerator<MessageEvent, void, unknown>;
}