import { computed, type WritableComputedRef } from "vue";
import { createI18n } from "vue-i18n";
import en from "./locales/en.json";
import ukMessages from "./locales/uk.json";

type MessageSchema = typeof en;

const uk: MessageSchema = ukMessages;

export const SUPPORTED_LOCALES = ["en", "uk"] as const;
export type Locale = (typeof SUPPORTED_LOCALES)[number];

export const DEFAULT_LOCALE: Locale = "en";
const STORAGE_KEY = "tantei.locale";

function isLocale(value: string | null | undefined): value is Locale {
  return (
    value !== null &&
    value !== undefined &&
    (SUPPORTED_LOCALES as readonly string[]).includes(value)
  );
}

function resolveInitialLocale(): Locale {
  const stored = localStorage.getItem(STORAGE_KEY);
  if (isLocale(stored)) {
    return stored;
  }

  for (const language of navigator.languages) {
    const candidate = language.split("-", 1)[0]?.toLowerCase();
    if (isLocale(candidate)) {
      return candidate;
    }
  }

  return DEFAULT_LOCALE;
}

export const i18n = createI18n({
  legacy: false,
  locale: resolveInitialLocale(),
  fallbackLocale: DEFAULT_LOCALE,
  messages: { en, uk },
});

export function setLocale(locale: Locale): void {
  i18n.global.locale.value = locale;
  localStorage.setItem(STORAGE_KEY, locale);
  document.documentElement.setAttribute("lang", locale);
}

export const currentLocale: WritableComputedRef<Locale> = computed({
  get: () => {
    const value = i18n.global.locale.value;
    return isLocale(value) ? value : DEFAULT_LOCALE;
  },
  set: setLocale,
});

document.documentElement.setAttribute("lang", i18n.global.locale.value);
