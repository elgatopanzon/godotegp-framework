/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IEcsSystem
 * @created     : Saturday Jan 25, 2025 23:02:27 CST
 */

namespace GodotEGP.ECSv4.Systems;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4.Queries;

public partial interface IEcsSystem 
{
	// system's update method
	public static abstract void Update(double deltaTimeSys, double deltaTime, ECS core, Query query);
}
