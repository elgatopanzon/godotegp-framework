/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ModelResult
 * @created     : Sunday Jan 21, 2024 21:07:56 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;

public partial class ModelResult : BaseResult
{
	public string Id { get; set; }
	public long Created { get; set; }
	public string OwnedBy { get; set; }

	public ModelResult()
	{
		Created = ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds();
		Object = "model";
		OwnedBy = "local";
	}
}
