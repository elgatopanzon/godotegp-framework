/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CompletionRequest
 * @created     : Sunday Jan 21, 2024 20:40:20 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class CompletionRequest : CompletionRequestBase
{
	public object Prompt { get; set; }
	public int BestOf { get; set; }
	public string Suffix { get; set; }
	public bool Echo { get; set; }

	public CompletionRequest()
	{
		Echo = false;
		Prompt = new();
		BestOf = 1;
		Suffix = "";
	}
}
