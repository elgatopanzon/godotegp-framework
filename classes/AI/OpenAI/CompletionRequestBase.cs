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

public partial class CompletionRequestBase
{
	public string Model { get; set; }
	public bool Echo { get; set; }
	public double FrequencyPenalty { get; set; }
	public int MaxTokens { get; set; }
	public int N { get; set; }
	public double PresencePenalty { get; set; }
	public int Seed { get; set; }
	public object Stop { get; set; }
	public bool Stream { get; set; }
	public double Temperature { get; set; }
	public double TopP { get; set; }
	public string User { get; set; }

	public CompletionRequestBase()
	{
		Echo = false;
		FrequencyPenalty = 0;
		MaxTokens = 16;
		N = 1;
		PresencePenalty = 0;
		Seed = -1;
		Stop = new();
		Stream = false;
		Temperature = 1;
		TopP = 0.95;
		User = "";
	}
}

