/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : System
 * @created     : Wednesday May 08, 2024 15:14:51 CST
 */

namespace GodotEGP.ECSv4.Systems;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Queries;

using System;
using System.Diagnostics;

public partial class SystemInstance
{
	public ISystem System { get; set; }
	public Entity SystemEntity { get; set; }
	public Entity QueryEntity { get; set; }

	// update the system and call the ISystem Update() method
	public void Update(Entity entity, int index, ECS core, double deltaTime, QueryResult result)
	{
		// call the system update
		System.Update(entity, index, this, deltaTime, core, result.Query);
	}
}
