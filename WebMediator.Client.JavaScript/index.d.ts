export declare class WebMediatorClient {
    constructor(endpointUrl: string, requestInit?: RequestInit | undefined);

    private readonly _endpointUrl;
    private readonly _requestInit;

    public getUrl(type: string) : string;
    public getUrl(type: string, data?: any) : string;
    public getUrl(request: WebMediatorRequest) : string;

    public send(type: string): Promise<WebMediatorResponse>;
    public send(type: string, data?: any, options?: SendOptions): Promise<WebMediatorResponse>;
    public send(request: WebMediatorRequest, options?: SendOptions): Promise<WebMediatorResponse>;

    public eventStream(type: string): AsyncGenerator<ServerSentEvent, void, unknown>;
    public eventStream(type: string, data?: any, options?: EventStreamOptions): AsyncGenerator<ServerSentEvent, void, unknown>;
    public eventStream(request: WebMediatorRequest, options?: EventStreamOptions): AsyncGenerator<ServerSentEvent, void, unknown>;
}

export interface WebMediatorRequest { 
    type: string;
    data?: any;
}

export interface WebMediatorResponse extends WebMediatorRequest { 

}

export interface SendOptions { 
    signal?: AbortSignal;
}

export interface EventStreamOptions extends SendOptions { 
    reconnectionDelay?: number;
    reconnectionRetriesLimit?: number;
}

export interface ServerSentEvent { 
    type: string;
    data?: any;
    lastEventId?: string;
}