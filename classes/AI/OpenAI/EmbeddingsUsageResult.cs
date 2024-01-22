/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EmbeddingsUsageResult
 * @created     : Sunday Jan 21, 2024 20:27:22 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class EmbeddingsUsageResult
{
	public int PromptTokens { get; set; }
	public int TotalTokens { 
		get {
			return PromptTokens;
		}
	}
}

