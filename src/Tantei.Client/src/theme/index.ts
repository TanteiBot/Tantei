import { computed, ref, type WritableComputedRef } from "vue";

export const THEMES = ["light", "dark"] as const;
export type Theme = (typeof THEMES)[number];

const STORAGE_KEY = "tantei.theme";

function isTheme(value: string | null | undefined): value is Theme {
  return value === "light" || value === "dark";
}

function systemTheme(): Theme {
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

function resolveInitialTheme(): Theme {
  const stored = localStorage.getItem(STORAGE_KEY);
  return isTheme(stored) ? stored : systemTheme();
}

function applyTheme(theme: Theme): void {
  document.documentElement.classList.toggle("dark", theme === "dark");
}

const theme = ref<Theme>(resolveInitialTheme());

export function setTheme(value: Theme): void {
  theme.value = value;
  localStorage.setItem(STORAGE_KEY, value);
  applyTheme(value);
}

export const currentTheme: WritableComputedRef<Theme> = computed({
  get: () => theme.value,
  set: setTheme,
});

applyTheme(theme.value);
