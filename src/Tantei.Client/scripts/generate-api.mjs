import { spawnSync } from "node:child_process";
import { createRequire } from "node:module";
import { dirname, resolve } from "node:path";

const require = createRequire(import.meta.url);

function binOf(packageName) {
  const manifestPath = require.resolve(`${packageName}/package.json`);
  const { bin } = require(manifestPath);
  return resolve(dirname(manifestPath), typeof bin === "string" ? bin : bin[packageName]);
}

const steps = [
  [binOf("kubb"), ["generate"]],
  [binOf("oxfmt"), ["--write", "src/api/gen"]],
];

for (const [script, args] of steps) {
  const { status, error } = spawnSync(process.execPath, [script, ...args], {
    stdio: "inherit",
    env: { ...process.env, KUBB_DISABLE_TELEMETRY: "1" },
  });

  if (error) {
    throw error;
  }

  if (status !== 0) {
    process.exit(status ?? 1);
  }
}
