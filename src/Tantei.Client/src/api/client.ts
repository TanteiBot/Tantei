import createClient from "openapi-fetch";
import type { components, paths } from "./schema";

export const api = createClient<paths>();

/** Response of `GET /api/ping`. */
export type PingResponse = components["schemas"]["PingResponse"];

/** One entry of `GET /api/getUpdateTimes`. */
export type UpdateProviderStatus = components["schemas"]["UpdateProviderStatusResponse"];
