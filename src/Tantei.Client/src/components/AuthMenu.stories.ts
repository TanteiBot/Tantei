import type { Meta, StoryObj } from "@storybook/vue3-vite";
import type { CurrentUserResponse } from "@/api/gen/types/CurrentUserResponse";
import AuthMenu from "./AuthMenu.vue";

const user: CurrentUserResponse = {
  discordUserId: "191243925786820608",
  username: "kudou",
  avatarUrl: null,
  isRegistered: true,
  isWebAdmin: false,
};

const meta = {
  title: "Components/AuthMenu",
  component: AuthMenu,
} satisfies Meta<typeof AuthMenu>;

export default meta;

type Story = StoryObj<typeof meta>;

export const SignedOut: Story = {
  args: { user: null },
};

export const SignedIn: Story = {
  args: { user },
};

export const Loading: Story = {
  args: { user: null, isLoading: true },
};
