/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ModelsResult
 * @created     : Sunday Jan 21, 2024 21:06:35 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ModelsResult : ListBaseResult<ModelResult>
{
	public ModelsResult()
	{
		Data = new();
		Object = "list";
	}
}
