/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChatCompletionRequest
 * @created     : Sunday Jan 21, 2024 20:46:36 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using Newtonsoft.Json;

public partial class ChatCompletionRequest : CompletionRequestBase
{
	public ChatCompletionRequestResponseFormat ResponseFormat { get; set; }
	public List<ChatCompletionRequestMessage> Messages { get; set; }

	[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	public List<ChatCompletionRequestTool> Tools { get; set; }

	[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	public object? ToolChoice { get; set; }

	[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	public ChatCompletionRequestGatoGPTExtended? Extended { get; set; }

	public ChatCompletionRequest()
	{
		ResponseFormat = new();
		// ToolChoice = (string) "auto";

		// the Completion API has 16 as default, while the Chat Completion API
		// has it set to null (presumably unlimited/max context length)
		// MaxTokens = -1;
	}

	public string GetToolChoice()
	{
		if (ToolChoice is string ts)
			return ts;
		else
			return "";
	}

	public ChatCompletionRequestToolChoice GetToolChoiceObject()
	{
		if (ToolChoice is ChatCompletionRequestToolChoice dto)
		{
			return dto;
		}

		return new();
	}
}

public partial class ChatCompletionRequestToolChoice
{
	public string Type { get; set; }
	public ChatCompletionRequestToolChoiceFunction Function { get; set; }

	public ChatCompletionRequestToolChoice()
	{
		Function = new();
	}
}

public partial class ChatCompletionRequestToolChoiceFunction
{
	public string Name { get; set; }
}

public partial class ChatCompletionRequestTool
{
	public string Type { get; set; }
	public ChatCompletionRequestFunction Function { get; set; }

	public ChatCompletionRequestTool()
	{
		Function = new();
	}
}

public partial class ChatCompletionRequestFunction
{
	public string Description { get; set; }
	public string Name { get; set; }
	public object Parameters { get; set; }

	public ChatCompletionRequestFunction()
	{
		Description = "";
		Parameters = new();
	}

	public ChatCompletionRequestFunctionParameters GetParametersDto()
	{
		return JsonConvert.DeserializeObject<ChatCompletionRequestFunctionParameters>(Parameters.ToString());
	}
}

public partial class ChatCompletionRequestFunctionParameters
{
	public string Type { get; set; }
	public Dictionary<string, ChatCompletionRequestFunctionProperty> Properties { get; set; }
	public List<string> Required { get; set; }

	public ChatCompletionRequestFunctionParameters()
	{
		Properties = new();
		Required = new();
	}
}

public partial class ChatCompletionRequestFunctionProperty
{
	public string Type { get; set; }
	public string Description { get; set; }
	public List<string> Enum { get; set; }

	public ChatCompletionRequestFunctionProperty()
	{
		Type = "string";
		Description = "";
		Enum = new();
	}
}

public partial class ChatCompletionRequestResponseFormat
{
	public string Type { get; set; }

	public ChatCompletionRequestResponseFormat()
	{
		Type = "text";
	}
}

public partial class ChatCompletionRequestMessage : ChatCompletionResultMessage
{
	[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	public string Name { get; set; }

	public ChatCompletionRequestMessage()
	{
	}
}

public partial class ChatCompletionRequestGatoGPTExtended
{
	public GatoGPTExtendedModel Model { get; set; }
	public GatoGPTExtendedInference Inference { get; set; }
}

public partial class GatoGPTExtendedModel
{
	public int? NCtx { get; set; }
	public int? NBatch { get; set; }
	public int? NGpuLayers { get; set; }
	public string? Backend { get; set; }
	public bool? PromptCache { get; set; }
	public double? RopeFreqBase { get; set; }
	public double? RopeFreqScale { get; set; }
}
public partial class GatoGPTExtendedInference
{
	public int? NThreads { get; set; }
	public int? NKeep { get; set; }
	public int? TopK { get; set; }
	public double? Tfs { get; set; }
	public double? Typical { get; set; }
	public double? RepeatPenalty { get; set; }
	public int? RepeatLastN { get; set; }
	public bool? Vision { get; set; }
	public string? GrammarResourceId { get; set; }

	public string? ChatMessageTemplate { get; set; }
	public string? ChatMessageGenerationTemplate { get; set; }
	public string? PrePrompt { get; set; }

	public string? CfgNegativePrompt { get; set; }
	public double? CfgScale { get; set; }

	public string? PromptCacheId { get; set; }

	public List<string>? Samplers { get; set; }
}
