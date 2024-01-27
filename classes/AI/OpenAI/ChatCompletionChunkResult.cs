/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChatCompletionChunkResult
 * @created     : Friday Jan 26, 2024 15:30:58 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ChatCompletionChunkResult : BaseResult
{
	public string Id { get; set; }
	public List<ChatCompletionChunkResultChoices> Choices { get; set; }
	public long Created { get; set; }
	public string Model { get; set; }
	public string SystemFingerprint { get; set; }

	public ChatCompletionChunkResult()
	{
		Object = "chat.completion.chunk";		
		Created = ((DateTimeOffset) DateTime.Now).ToUnixTimeSeconds();
		Choices = new();
	}
}

public partial class ChatCompletionChunkResultChoices
{
	public int Index { get; set; }
	public ChatCompletionResultMessage Delta { get; set; }
	public string FinishReason { get; set; }

	public ChatCompletionChunkResultChoices()
	{
	}
}
