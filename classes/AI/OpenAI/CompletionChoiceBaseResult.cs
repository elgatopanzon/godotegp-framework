/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CompletionChoiceBaseResult
 * @created     : Sunday Jan 21, 2024 20:38:21 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class CompletionChoiceBaseResult
{
	public string FinishReason { get; set; }
	public int Index { get; set; }
	public object Logprobs { get; set; } // TODO
}

