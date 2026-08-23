import type { App } from "vue";

import { i18n } from ".";
import type en from "./locales/en.json";

type Join<Prefix extends string, Key extends string> = Prefix extends "" ? Key : `${Prefix}.${Key}`;

type MessagePaths<T, Prefix extends string = ""> = {
  [K in keyof T & string]: T[K] extends string
    ? Join<Prefix, K>
    : MessagePaths<T[K], Join<Prefix, K>>;
}[keyof T & string];

export type TranslationKey = MessagePaths<typeof en>;

export function tStrict(key: TranslationKey, named?: Record<string, unknown>): string {
  return named === undefined ? i18n.global.t(key) : i18n.global.t(key, named);
}

export const strictTranslate = {
  install(app: App): void {
    app.config.globalProperties.$tStrict = tStrict;
  },
};

declare module "vue" {
  interface ComponentCustomProperties {
    $tStrict: typeof tStrict;
  }
}
