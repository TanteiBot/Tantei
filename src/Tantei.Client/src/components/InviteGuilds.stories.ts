import type { Meta, StoryObj } from "@storybook/vue3-vite";

import type { InvitableGuildResponse } from "@/api/gen/types/InvitableGuildResponse";
import { inviteEligibility } from "@/api/gen/types/InviteEligibility";
import { inviteMode } from "@/api/gen/types/InviteMode";

import InviteGuilds from "./InviteGuilds.vue";

const guilds: InvitableGuildResponse[] = [
  { guildId: "225980711166967808", name: "Anime Club", iconUrl: null },
  { guildId: "336642139381301249", name: "Reading Group", iconUrl: null },
];

const meta = {
  title: "Components/InviteGuilds",
  component: InviteGuilds,
} satisfies Meta<typeof InviteGuilds>;

export default meta;

type Story = StoryObj<typeof meta>;

export const BlockedByPrivate: Story = {
  args: { mode: inviteMode.Private, eligibility: inviteEligibility.NotAllowed, guilds: [] },
};

export const BlockedBySemiPrivate: Story = {
  args: { mode: inviteMode.SemiPrivate, eligibility: inviteEligibility.NotAllowed, guilds: [] },
};

export const NothingToInvite: Story = {
  args: { mode: inviteMode.Public, eligibility: inviteEligibility.Allowed, guilds: [] },
};

export const Invitable: Story = {
  args: { mode: inviteMode.Public, eligibility: inviteEligibility.Allowed, guilds },
};

export const Unavailable: Story = {
  args: { mode: inviteMode.SemiPrivate, eligibility: inviteEligibility.Unknown, guilds: [] },
};
