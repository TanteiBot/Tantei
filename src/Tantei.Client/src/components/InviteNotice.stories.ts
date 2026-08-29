import type { Meta, StoryObj } from "@storybook/vue3-vite";

import { inviteMode } from "@/api/gen/types/InviteMode";

import InviteNotice from "./InviteNotice.vue";

const meta = {
  title: "Components/InviteNotice",
  component: InviteNotice,
} satisfies Meta<typeof InviteNotice>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Private: Story = {
  args: { mode: inviteMode.Private },
};

export const SemiPrivate: Story = {
  args: { mode: inviteMode.SemiPrivate },
};

export const Public: Story = {
  args: { mode: inviteMode.Public },
};
