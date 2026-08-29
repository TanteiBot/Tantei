<script setup lang="ts">
import LangSwitcher from "@/components/LangSwitcher.vue";
import ThemeToggle from "@/components/ThemeToggle.vue";

const sourceUrl = "https://github.com/TanteiBot/Tantei";
const licenseUrl = "https://www.gnu.org/licenses/agpl-3.0.html";
</script>

<template>
  <UFooter class="mt-auto bg-muted border-t border-muted">
    <template #left>
      <div class="text-dimmed flex flex-col items-center gap-1 text-xs sm:items-start">
        <span>{{ $tStrict("footer.copyright") }}</span>
        <ULink :to="licenseUrl" target="_blank" class="text-dimmed hover:text-default">
          {{ $tStrict("footer.license") }}
        </ULink>
      </div>
    </template>

    <template #right>
      <UButton
        :to="{ name: '/about' }"
        color="neutral"
        variant="ghost"
        size="sm"
        class="whitespace-nowrap"
      >
        {{ $tStrict("nav.about") }}
      </UButton>
      <UButton
        :to="sourceUrl"
        target="_blank"
        color="neutral"
        variant="ghost"
        size="sm"
        class="whitespace-nowrap"
      >
        {{ $tStrict("footer.source") }}
      </UButton>
      <LangSwitcher />
      <ThemeToggle />
    </template>
  </UFooter>
</template>
