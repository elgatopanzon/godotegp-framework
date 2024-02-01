/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : TokenizedString
 * @created     : Wednesday Jan 31, 2024 19:19:20 CST
 */

namespace GodotEGP.AI.GatoGPT;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class TokenizedString
{
	public int Id { get; set; }
	public string Token { get; set; }
}

