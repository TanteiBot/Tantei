import createClient from "openapi-fetch";
import type { components, paths } from "./schema";

export const api = createClient<paths>();

/** RFC 9457 error body returned by every failing endpoint. */
export type ProblemDetails = components["schemas"]["ProblemDetails"];

/** Response of `GET /api/ping`. */
export type PingResponse = components["schemas"]["PingResponse"];

/** One entry of `GET /api/getUpdateTimes`. */
export type UpdateProviderStatus = components["schemas"]["UpdateProviderStatusResponse"];
