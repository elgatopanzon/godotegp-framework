namespace GodotEGP.Service;

using Godot;
using System;

using GodotEGP.Logging;

public partial class SystemService : Service
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_SetServiceReady(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
		LoggerManager.LogDebug($"Service registered!", "", "service", this.GetType().Name);
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
	}
}

