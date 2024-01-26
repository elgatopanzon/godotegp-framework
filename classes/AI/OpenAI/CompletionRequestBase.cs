/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CompletionRequestBase
 * @created     : Sunday Jan 21, 2024 20:41:57 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using Newtonsoft.Json;

public partial class CompletionRequestBase : RequestBase
{
	public string Model { get; set; }
	public double FrequencyPenalty { get; set; }

	[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	public int? MaxTokens { get; set; }
	public int N { get; set; }
	public double PresencePenalty { get; set; }
	public int Seed { get; set; }

	[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	public object? Stop { get; set; }
	public bool Stream { get; set; }
	public double Temperature { get; set; }
	public double TopP { get; set; }
	public string User { get; set; }

	public CompletionRequestBase()
	{
		FrequencyPenalty = 0;
		N = 1;
		PresencePenalty = 0;
		Seed = -1;
		Stream = false;
		Temperature = 1;
		TopP = 0.95;
		User = "";
	}
}

