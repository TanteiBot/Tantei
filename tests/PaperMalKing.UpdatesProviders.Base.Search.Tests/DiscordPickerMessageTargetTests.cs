// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base.Search.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The Discord client owns the injected HTTP client and handler")]
[SuppressMessage(
	"Security",
	"S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
	Justification = "The test replaces DSharpPlus transport internals with an in-process HTTP handler")]
[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "The task sources are controlled by the test")]
public sealed class DiscordPickerMessageTargetTests
{
	private const ulong ChannelId = 42UL;

	[Test]
	public async Task APublicPostUsesTheDiscordChannelDeliveryPath()
	{
		var handler = new RecordingDiscordHandler();
		using var client = CreateClient(handler);
		var target = new DiscordPickerMessageTarget(_originalInteraction: null!, CreateChannel(client));

		await target.SendPublicAsync(new DiscordEmbedBuilder().WithTitle("Monster"));

		await Assert.That(handler.Method).IsEqualTo(HttpMethod.Post);
		await Assert.That(handler.Path).IsEqualTo($"/api/v10/channels/{ChannelId}/messages");
		await Assert.That(handler.Body).Contains("Monster");
	}

	[Test]
	public async Task CancellationStopsWaitingForTheDiscordChannelSend()
	{
		var handler = new RecordingDiscordHandler { PauseResponse = true, };
		using var client = CreateClient(handler);
		var target = new DiscordPickerMessageTarget(_originalInteraction: null!, CreateChannel(client));
		using var cancellation = new CancellationTokenSource();
		var sending = target.SendPublicAsync(new DiscordEmbedBuilder().WithTitle("Monster"), cancellation.Token);
		await handler.RequestStarted.Task;

		cancellation.Cancel();
		var cancelled = false;
		try
		{
			await sending;
		}
		catch (OperationCanceledException)
		{
			cancelled = true;
		}
		finally
		{
			handler.AllowResponse.SetResult();
		}

		await Assert.That(cancelled).IsTrue();
	}

	private static DiscordClient CreateClient(HttpMessageHandler handler)
	{
		var client = new DiscordClient(new DiscordConfiguration { Token = "test-token", });
		var apiClient = typeof(BaseDiscordClient).GetProperty("ApiClient", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(client)!;
		var restClient = apiClient.GetType().GetProperty("_rest", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(apiClient)!;
		var httpClientField = restClient.GetType().GetField("<HttpClient>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
		((HttpClient)httpClientField.GetValue(restClient)!).Dispose();
		httpClientField.SetValue(restClient, new HttpClient(handler) { BaseAddress = new("https://discord.com/api/v10/"), });
		return client;
	}

	private static DiscordChannel CreateChannel(BaseDiscordClient client)
	{
		var channel = (DiscordChannel)Activator.CreateInstance(typeof(DiscordChannel), nonPublic: true)!;
		typeof(SnowflakeObject).GetProperty(nameof(SnowflakeObject.Id))!.SetValue(channel, ChannelId);
		typeof(SnowflakeObject).GetProperty("Discord", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(channel, client);
		typeof(DiscordChannel).GetProperty(nameof(DiscordChannel.Type))!.SetValue(channel, ChannelType.Text);
		return channel;
	}

	private sealed class RecordingDiscordHandler : HttpMessageHandler
	{
		public bool PauseResponse { get; init; }

		public HttpMethod? Method { get; private set; }

		public string? Path { get; private set; }

		public string? Body { get; private set; }

		public TaskCompletionSource RequestStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource AllowResponse { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			this.Method = request.Method;
			this.Path = request.RequestUri?.AbsolutePath;
			this.Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			this.RequestStarted.SetResult();
			if (this.PauseResponse)
			{
				await this.AllowResponse.Task.ConfigureAwait(false);
			}

			return new(HttpStatusCode.OK)
			{
				Content = new StringContent(
					"{\"id\":\"1\",\"channel_id\":\"42\",\"author\":{\"id\":\"2\",\"username\":\"Tantei\",\"discriminator\":\"0001\"},\"embeds\":[]}",
					Encoding.UTF8,
					"application/json"),
			};
		}
	}
}
