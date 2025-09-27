// Simplified Cloudflare Workers TypeScript definitions
// This version works with Glutinum 0.12.0 without stack overflow

export interface Headers {
    append(name: string, value: string): void;
    delete(name: string): void;
    get(name: string): string | null;
    has(name: string): boolean;
    set(name: string, value: string): void;
}

export declare const Headers: {
    new(init?: Record<string, string>): Headers;
};

export interface Request {
    readonly method: string;
    readonly url: string;
    readonly headers: Headers;
    text(): Promise<string>;
    json<T = any>(): Promise<T>;
    arrayBuffer(): Promise<ArrayBuffer>;
}

export interface Response {
    readonly status: number;
    readonly statusText: string;
    readonly ok: boolean;
    readonly headers: Headers;
    text(): Promise<string>;
    json<T = any>(): Promise<T>;
    arrayBuffer(): Promise<ArrayBuffer>;
}

export interface ResponseInit {
    status?: number;
    statusText?: string;
    headers?: Record<string, string> | Headers;
}

export declare const Response: {
    new(body?: string | ArrayBuffer | null, init?: ResponseInit): Response;
    json(data: any, init?: ResponseInit): Response;
    redirect(url: string, status?: number): Response;
};

export interface ExecutionContext {
    waitUntil(promise: Promise<any>): void;
    passThroughOnException(): void;
}

export interface KVNamespace {
    get(key: string, type?: "text"): Promise<string | null>;
    get(key: string, type: "json"): Promise<any>;
    get(key: string, type: "arrayBuffer"): Promise<ArrayBuffer | null>;
    put(key: string, value: string | ArrayBuffer, options?: KVPutOptions): Promise<void>;
    delete(key: string): Promise<void>;
    list(options?: KVListOptions): Promise<KVListResult>;
}

export interface KVPutOptions {
    expiration?: number;
    expirationTtl?: number;
    metadata?: any;
}

export interface KVListOptions {
    limit?: number;
    prefix?: string;
    cursor?: string;
}

export interface KVListResult {
    keys: Array<{ name: string; expiration?: number; metadata?: any }>;
    list_complete: boolean;
    cursor?: string;
}

export interface Env {
    [key: string]: any;
}

export type FetchHandler = (request: Request, env: Env, ctx: ExecutionContext) => Response | Promise<Response>;