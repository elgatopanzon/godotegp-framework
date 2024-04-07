/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ErrorResultExtended
 * @created     : Tuesday Jan 30, 2024 15:55:29 CST
 */

namespace GodotEGP.AI.OpenAI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;

public partial class ErrorResultExtended : BaseResult
{
	public ErrorResultExtendedObj Error { get; set; }
}

public partial class ErrorResultExtendedObj : ErrorResultObj
{
	public Exception Exception { get; set; }
}
