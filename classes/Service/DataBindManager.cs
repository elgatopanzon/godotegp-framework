/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : DataBindManager
 * @created     : Monday Nov 13, 2023 15:17:02 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;
using GodotEGP.Config;

using GodotEGP.DataBind;

public partial class DataBindManager : Service
{
	public DataBindManager()
	{
		
	}

	/*******************
	*  Godot methods  *
	*******************/

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_SetServiceReady(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/**********************
	*  Service methods  *
	**********************/

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
		// LoggerManager.LogDebug($"Service deregistered!", "", "service", this.GetType().Name);
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
	}

	/*************************************
	*  Data binding management methods  *
	*************************************/
	
	public void Bind<T>(object objectFirst, Func<T> getterFirstCb, Action<T> setterFirstCb)
	{
		foreach (var cnode in GetChildren())
		{
			if (cnode is DataBinding<T, ValidatedValueChanged> db)
			{
				if (db.ObjectFirst.Equals(objectFirst))
				{
					db.QueueFree();
				}
			}
		}

		var dataBinding = new DataBinding<T, ValidatedValueChanged>(objectFirst, getterFirstCb, setterFirstCb);

		AddChild(dataBinding);
	}

	public void BindSignal<TT, T>(string nodeId, string signalName, bool hasParams, Func<TT, T> getterFirstCb, Action<T> setterFirstCb, bool initialSet = false) where TT : Node
	{
		foreach (var cnode in GetChildren())
		{
			if (cnode is DataBinding<T, GodotSignal> db)
			{
				if (db.ObjectFirst is string str)
				{
					db.QueueFree();
				}
			}
		}
		// get the current node object
		var node = nodeId.Node<TT>();

		// create the data binding
		var dataBinding = new DataBinding<T, GodotSignal>(nodeId, () => getterFirstCb(nodeId.Node<TT>()), setterFirstCb, initialSet: initialSet);

		// unsubscribe from the event
		ServiceRegistry.Get<EventManager>().Unsubscribe(dataBinding.EventSubscription);

		// use connect extension method to ensure that signal is rebound on
		// newly added nodes
		nodeId.Connect(signalName, hasParams, dataBinding._On_ObjectFirst_Changed, isHighPriority: true);

		AddChild(dataBinding);
	}
}

