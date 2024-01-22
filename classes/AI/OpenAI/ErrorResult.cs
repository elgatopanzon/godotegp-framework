/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ErrorResult
 * @created     : Sunday Jan 14, 2024 19:34:21 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ErrorResult
{
	public ErrorResultObj Error { get; set; }
}

public partial class ErrorResultObj
{
	public string Message { get; set; } = "";
	public string Type { get; set; } = "";
	public string Param { get; set; } = "";
	public string Code { get; set; } = "";
}
