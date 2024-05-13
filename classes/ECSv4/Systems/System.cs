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

using System;

public partial class SystemInstance
{
	// per-system delta time
	public DateTime LastUpdateTime { get; set; } = DateTime.Now;
	public double SystemDeltaTime
	{
		get {
			return (DateTime.Now.Ticks - LastUpdateTime.Ticks) / 10000000f;
		}
	}

	public double DeltaTime { get; set; }
	public ISystem System { get; set; }
	public Entity SystemEntity { get; set; }
	public Entity QueryEntity { get; set; }

	// update the system and call the ISystem Update() method
	public void Update(Entity entity, int index, ECS core, double deltaTime)
	{
		// update the frame-based delta time
		DeltaTime = deltaTime;

		// call the system update
		System.Update(entity, index, this, core);
	}
}
