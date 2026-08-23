import { defineConfigWithVueTs, vueTsConfigs } from "@vue/eslint-config-typescript";
import pluginOxlint from "eslint-plugin-oxlint";
import pluginStorybook from "eslint-plugin-storybook";
import pluginVue from "eslint-plugin-vue";
import { globalIgnores } from "eslint/config";

const vueFormattingRulesOff = Object.fromEntries(
  Object.entries(pluginVue.rules ?? {})
    .filter(([, rule]) => rule.meta?.type === "layout")
    .map(([name]) => [`vue/${name}`, "off"] as const),
);

export default defineConfigWithVueTs(
  {
    name: "app/files-to-lint",
    files: ["**/*.{vue,ts,mts,tsx}"],
  },

  {
    name: "app/linter-options",
    linterOptions: {
      reportUnusedDisableDirectives: "error",
    },
  },

  globalIgnores([
    "**/dist/**",
    "**/dist-ssr/**",
    "**/coverage/**",
    "**/storybook-static/**",
    "src/api/gen/**",
    "typed-router.d.ts",
  ]),

  ...pluginVue.configs["flat/recommended"],

  {
    name: "app/vue-formatting-owned-by-oxfmt",
    rules: vueFormattingRulesOff,
  },

  vueTsConfigs.recommendedTypeChecked,

  {
    name: "app/type-aware-parser",
    files: ["**/*.{vue,ts,mts,tsx}"],
    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },

  {
    name: "app/pages",
    files: ["src/pages/**/*.vue"],
    rules: {
      "vue/multi-word-component-names": "off",
    },
  },

  ...pluginStorybook.configs["flat/recommended"],

  ...pluginOxlint.buildFromOxlintConfigFile(".oxlintrc.json"),
);
