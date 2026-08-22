import type { Meta, StoryObj } from "@storybook/vue3-vite";
import ApiPing from "./ApiPing.vue";

const meta = {
  title: "Components/ApiPing",
  component: ApiPing,
  parameters: {
    docs: {
      description: {
        component:
          'Renders the API wiring check card in its idle state. Storybook has no backend, so pressing "Ping API" resolves to the error state.',
      },
    },
  },
} satisfies Meta<typeof ApiPing>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {};
