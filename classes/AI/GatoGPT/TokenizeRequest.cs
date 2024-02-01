/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : TokenizeRequest
 * @created     : Wednesday Jan 31, 2024 19:06:02 CST
 */

namespace GodotEGP.AI.GatoGPT;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.AI.OpenAI;

public partial class TokenizeRequest : RequestBase
{
	public string Model { get; set; }
	public string Content { get; set; }
}
