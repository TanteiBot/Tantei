import type { Meta, StoryObj } from "@storybook/vue3-vite";

import type { LicenseResponse } from "@/api/gen/types/LicenseResponse";

import LicenseList from "./LicenseList.vue";

const mitText = `MIT License

Copyright (c) 2021-present Floating UI contributors

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction.`;

const withText: LicenseResponse = {
  name: "@floating-ui/core",
  version: "1.8.0",
  identifier: "MIT",
  text: mitText,
  url: "https://spdx.org/licenses/MIT.html",
};

const withTextButNoIdentifier: LicenseResponse = {
  name: "SourceGear.sqlite3",
  version: "3.50.4.5",
  identifier: null,
  text: mitText,
  url: null,
};

const withIdentifierAndUrl: LicenseResponse = {
  name: "Serilog",
  version: "4.4.0",
  identifier: "Apache-2.0",
  text: null,
  url: "https://licenses.nuget.org/Apache-2.0",
};

const withUrlOnly: LicenseResponse = {
  name: "JikanDotNet",
  version: "2.10.4",
  identifier: null,
  text: null,
  url: "https://github.com/Ervie/jikan.net/blob/master/LICENSE",
};

const withNothing: LicenseResponse = {
  name: "Unattributed.Package",
  version: "1.0.0",
  identifier: null,
  text: null,
  url: null,
};

const meta = {
  title: "Components/LicenseList",
  component: LicenseList,
} satisfies Meta<typeof LicenseList>;

export default meta;

type Story = StoryObj<typeof meta>;

export const WithLicenseText: Story = {
  args: { licenses: [withText] },
};

export const WithIdentifierAndLink: Story = {
  args: { licenses: [withIdentifierAndUrl] },
};

export const WithLinkOnly: Story = {
  args: { licenses: [withUrlOnly] },
};

export const WithoutLicenseInformation: Story = {
  args: { licenses: [withNothing] },
};

export const WithLicenseTextButNoIdentifier: Story = {
  args: { licenses: [withTextButNoIdentifier] },
};

export const Mixed: Story = {
  args: {
    licenses: [withText, withTextButNoIdentifier, withIdentifierAndUrl, withUrlOnly, withNothing],
  },
};

export const Empty: Story = {
  args: { licenses: [] },
};
