/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChatCompletionResult
 * @created     : Sunday Jan 21, 2024 20:59:48 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ChatCompletionResult : CompletionBaseResult<ChatCompletionResultChoice>
{
	public ChatCompletionResult()
	{
		Object = "chat.completion";
	}
}

public partial class ChatCompletionResultChoice : CompletionChoiceBaseResult
{
	public ChatCompletionResultMessage Message { get; set; }
}

public partial class ChatCompletionResultToolCall
{
	public string Id { get; set; }
	public string Type { get; set; }
	public ChatCompletionResultToolFunction Function { get; set; }

	public ChatCompletionResultToolCall()
	{
		Function = new();
	}
}

public partial class ChatCompletionResultToolCallRaw
{
	public string Function { get; set; }
	public Dictionary<string, object> Arguments { get; set; }

	public ChatCompletionResultToolCallRaw()
	{
		Arguments = new();
	}
}

public partial class ChatCompletionResultToolFunction
{
	public string Name { get; set; }
	public string Arguments { get; set; }

	public ChatCompletionResultToolFunction()
	{
		
	}
}
