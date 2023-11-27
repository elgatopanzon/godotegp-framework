namespace GodotEGP.Service;

using Godot;
using System;

using GodotEGP.Event.Events;
using GodotEGP.Objects.Extensions;

public partial class Service : Godot.Node
{
	public bool ServiceReady { get; set; }

	public Service()
	{
		Name = this.GetType().Name;
	}

	public bool GetReady()
	{
		return ServiceReady;
	}

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
	public virtual void _OnServiceRegistered()
	{
	}

	// Called when service is deregistered from manager
	public virtual void _OnServiceDeregistered()
	{
		// LoggerManager.LogDebug($"Service deregistered!", "", "service", this.GetType().Name);
	}

	// Called when service is considered ready
	public virtual void _OnServiceReady()
	{
	}

	// Sets service as ready
	public virtual void _SetServiceReady(bool readyState)
	{
		this.ServiceReady = readyState;

		if (readyState)
			this.Emit<ServiceReady>();
	}

	public virtual void Changed()
	{
		this.Emit<ValidatedValueChanged>();
	}
}
