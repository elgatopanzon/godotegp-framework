/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableConfigurableParam
 * @created     : Monday Apr 01, 2024 14:26:23 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ChainableConfigurableParam
{
	public string Name { get; set; }
	public string Id { get; set; }
	public string Description { get; set; }
}
