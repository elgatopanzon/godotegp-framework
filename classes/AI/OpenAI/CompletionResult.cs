/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CompletionResult
 * @created     : Sunday Jan 21, 2024 20:32:22 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class CompletionResult : CompletionBaseResult<CompletionChoiceResult>
{
	public CompletionResult()
	{
		Object = "text_completion";
	}
}

