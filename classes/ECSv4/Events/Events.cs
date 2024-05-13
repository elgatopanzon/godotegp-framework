/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Events
 * @created     : Thursday May 02, 2024 22:18:45 CST
 */

namespace GodotEGP.ECSv4.Events;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4;

public partial class EcsEntityEvent : Event 
{
	private Entity Entity;
}

public partial class EcsOnAddComponent : EcsEntityEvent {};
public partial class EcsOnSetComponent : EcsEntityEvent {};
public partial class EcsOnRemoveComponent : EcsEntityEvent {};
