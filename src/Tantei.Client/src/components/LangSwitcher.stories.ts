import type { Meta, StoryObj } from "@storybook/vue3-vite";
import LangSwitcher from "./LangSwitcher.vue";

const meta = {
  title: "Components/LangSwitcher",
  component: LangSwitcher,
} satisfies Meta<typeof LangSwitcher>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {};
