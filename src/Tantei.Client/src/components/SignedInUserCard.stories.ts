import type { Meta, StoryObj } from "@storybook/vue3-vite";

import type { CurrentUserResponse } from "@/api/gen/types/CurrentUserResponse";

import SignedInUserCard from "./SignedInUserCard.vue";

const registered: CurrentUserResponse = {
  discordUserId: "191243925786820608",
  username: "kudou",
  avatarUrl: null,
  isRegistered: true,
  isWebAdmin: false,
};

const meta = {
  title: "Components/SignedInUserCard",
  component: SignedInUserCard,
} satisfies Meta<typeof SignedInUserCard>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Registered: Story = {
  args: { user: registered },
};

export const NotRegistered: Story = {
  args: { user: { ...registered, isRegistered: false } },
};

export const WebAdmin: Story = {
  args: { user: { ...registered, isWebAdmin: true } },
};
