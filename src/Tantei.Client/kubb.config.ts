import { defineConfig } from "kubb";
import { adapterOas } from "@kubb/adapter-oas";
import { pluginBarrel } from "@kubb/plugin-barrel";
import { pluginFetch } from "@kubb/plugin-fetch";
import { pluginTs } from "@kubb/plugin-ts";
import { pluginVueQuery } from "@kubb/plugin-vue-query";

process.env["KUBB_DISABLE_TELEMETRY"] = "1";

const group = { type: "tag", name: ({ group }: { group: string }) => group.toLowerCase() } as const;

const exclude = [{ type: "path", pattern: "/api/auth/sign-in" }] as const;

export default defineConfig({
  root: ".",
  input: "./openapi.json",
  output: {
    path: "./src/api/gen",
    clean: true,
    barrel: { type: "named", nested: true },
  },
  adapter: adapterOas(),
  plugins: [
    pluginTs({ output: { path: "types" }, group }),
    pluginFetch({ output: { path: "clients" }, group, exclude: [...exclude] }),
    pluginVueQuery({ output: { path: "hooks" }, group, exclude: [...exclude], hooks: true }),
    pluginBarrel(),
  ],
});
