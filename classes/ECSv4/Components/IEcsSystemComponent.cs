/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IEcsSystemComponent
 * @created     : Saturday Jan 25, 2025 20:09:51 CST
 */

namespace GodotEGP.ECSv4.Components;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4.Queries;

public partial interface IEcsSystemComponent : ITagComponent 
{
	// system's update method
	public static abstract void Update(double deltaTimeSys, double deltaTime, ECS core, Query query);
}
