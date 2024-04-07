/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CompletionBaseResult
 * @created     : Sunday Jan 21, 2024 20:34:37 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Collections.Generic;

public partial class CompletionBaseResult<TChoiceDto> : BaseResult
{
	public string Id { get; set; }
	public List<TChoiceDto> Choices { get; set; }
	public long Created { get; set; }
	public string Model { get; set; }
	public string SystemFingerprint { get; set; }
	public CompletionUsageResult Usage { get; set; }

	public CompletionBaseResult()
	{
		Choices = new();
		Usage = new();
	}
}


public partial class CompletionUsageResult
{
	public int PromptTokens { get; set; }
	public int CompletionTokens { get; set; }
	public int TotalTokens { 
		get {
			return PromptTokens + CompletionTokens;
		}
	}
}
