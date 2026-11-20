import { defaultConfig } from "./config";
import * as core from "./core";
import { createClientCore, createLazyClient, toRequestInit } from "./core";
export * from "./core";

/**
 * The native request and response that every result and error carries.
 */
export type NativeRequest = Request;
export type NativeResponse = Response;

/**
 * Extra `fetch` init for a call or client, such as `cache`, `redirect`, or Next.js's `next`.
 */
export type NativeOptions = RequestInit & { next?: Record<string, unknown> };

export type ResponseError<
  TError = unknown,
  TRequest = NativeRequest,
  TResponse = NativeResponse,
> = core.ResponseError<TError, TRequest, TResponse>;
export const ResponseError = core.ResponseError as {
  new <TError = unknown, TRequest = NativeRequest, TResponse = NativeResponse>(
    config: ConstructorParameters<typeof core.ResponseError<TError, TRequest, TResponse>>[0],
  ): core.ResponseError<TError, TRequest, TResponse>;
  is: typeof core.ResponseError.is;
  // `instanceof` narrows to this type, so it carries the native request and response.
  prototype: core.ResponseError<unknown, NativeRequest, NativeResponse>;
};

export type RequestConfig<
  TBody = unknown,
  TRequest = NativeRequest,
  TResponse = NativeResponse,
> = core.RequestConfig<TBody, TRequest, TResponse> & {
  options?: NativeOptions;
};
export type Options<
  TData extends core.DataShape,
  ThrowOnError extends boolean = true,
  TRequest = NativeRequest,
  TResponse = NativeResponse,
> = core.Options<TData, ThrowOnError, TRequest, TResponse> & { options?: NativeOptions };
export type ClientConfig<TRequest = NativeRequest, TResponse = NativeResponse> = core.ClientConfig<
  TRequest,
  TResponse
> & { options?: NativeOptions };
export type ResolvedRequest = core.ResolvedRequest & { options?: NativeOptions };
export type RequestResult<
  TResponses,
  ThrowOnError extends boolean = true,
  TRequest = NativeRequest,
  TResponse = NativeResponse,
  THeaders = unknown,
> = core.RequestResult<TResponses, ThrowOnError, TRequest, TResponse, THeaders>;
export type UnwrappedResult<
  TResponses,
  ThrowOnError extends boolean = true,
  TRequest = NativeRequest,
  TResponse = NativeResponse,
  THeaders = unknown,
> = core.UnwrappedResult<TResponses, ThrowOnError, TRequest, TResponse, THeaders>;
export type ResponseErrorConfig<TError = unknown> = core.ResponseError<
  TError,
  NativeRequest,
  NativeResponse
>;
export type TransportResult<
  TRequest = NativeRequest,
  TResponse = NativeResponse,
> = core.TransportResult<TRequest, TResponse>;
export type DecodedResponse<
  TData = unknown,
  TRequest = NativeRequest,
  TResponse = NativeResponse,
> = core.DecodedResponse<TData, TRequest, TResponse>;
export type Transport<TRequest = NativeRequest, TResponse = NativeResponse> = core.Transport<
  TRequest,
  TResponse
>;
export type Interceptors<TRequest = NativeRequest, TResponse = NativeResponse> = core.Interceptors<
  TRequest,
  TResponse
>;
export type ClientInstance<
  TRequest = NativeRequest,
  TResponse = NativeResponse,
> = core.ClientInstance<TRequest, TResponse>;
export type StreamResult<
  TResponses,
  ThrowOnError extends boolean = true,
  TEvent = core.ServerSentEvent<core.SuccessOf<TResponses>>,
  TRequest = NativeRequest,
  TResponse = NativeResponse,
  THeaders = unknown,
> = core.StreamResult<TResponses, ThrowOnError, TEvent, TRequest, TResponse, THeaders>;
export type ResponseErrorOf<
  TResponses,
  TRequest = NativeRequest,
  TResponse = NativeResponse,
  THeaders = unknown,
> = core.ResponseErrorOf<TResponses, TRequest, TResponse, THeaders>;
export const isResponseError = core.isResponseError as <
  TResponses = unknown,
  TRequest = NativeRequest,
  TResponse = NativeResponse,
  THeaders = unknown,
>(
  error: unknown,
) => error is ResponseErrorOf<TResponses, TRequest, TResponse, THeaders>;

export const defaultTransport = async (request: core.ResolvedRequest): Promise<TransportResult> => {
  const nativeRequest = new Request(request.url, toRequestInit(request));
  // `Request` drops the non-standard `next` field, so Next.js reads it from the init.
  const nextInit: NativeOptions | undefined = request.options?.next
    ? { next: request.options.next }
    : undefined;
  const response = await globalThis.fetch(nativeRequest, nextInit);

  return { webResponse: response, request: nativeRequest, response };
};

/**
 * The client every generated operation uses unless a call passes its own. It is created on first use.
 */
export const client: ClientInstance = createLazyClient((config) =>
  createClientCore({ defaultTransport, ...config }),
);
client.setConfig(defaultConfig);

export const createClient: ClientInstance["createClient"] = client.createClient;
