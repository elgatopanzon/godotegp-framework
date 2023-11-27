/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ScreenTransitionManager
 * @created     : Sunday Nov 12, 2023 16:35:35 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;
using System.Linq;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Resource;

public partial class ScreenTransitionManager : Service
{
	private Dictionary<string, ResourceBase> _transitionScenes = new Dictionary<string, ResourceBase>();
	private Dictionary<string, ScreenTransition.ScreenTransition> _transitionSceneInstances = new Dictionary<string, ScreenTransition.ScreenTransition>();

	private string _currentTransitionId;

	public ScreenTransitionManager()
	{
		
	}

	public void SetConfig(Dictionary<string, ResourceBase> config)
	{
		LoggerManager.LogDebug("Setting transition scenes config");
		
		_transitionScenes = config;

		InstanceTransitionScenes();
	}

	/*******************
	*  Godot methods  *
	*******************/

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/*********************
	*  Service methods  *
	*********************/

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

	/*****************************************
	*  Transition scene management methods  *
	*****************************************/
	
	public void InstanceTransitionScenes()
	{
		foreach (var transitionScene in _transitionScenes)
		{
			var sceneInstance = (transitionScene.Value.RawValue as PackedScene).Instantiate();

			// check if no scenes exist already with this type
			// bool instanceExists = false;
			bool instanceExists = _transitionSceneInstances.Any(x => x.Value.GetType() == sceneInstance.GetType());

			if (!instanceExists)
			{
				LoggerManager.LogDebug("Creating instance of transition scene", "", "transitionScene", $"{transitionScene.Key} {sceneInstance.GetType().Name}");

				_transitionSceneInstances.Add(transitionScene.Key,(ScreenTransition.ScreenTransition) sceneInstance);

				AddChild(sceneInstance);
			}
		}

		if (!GetReady())
		{
			_SetServiceReady(true);
		}
	}

	public void StartTransition(string transitionId)
	{
		if (IsValidTransitionId(transitionId))
		{
			_currentTransitionId = transitionId;
			// subscribe to oneshot events to track the stages of the transition
			_transitionSceneInstances[transitionId].SubscribeOwner<ScreenTransitionShowing>(_On_ScreenTransitionShowing, oneshot: true, isHighPriority: true);
			_transitionSceneInstances[transitionId].SubscribeOwner<ScreenTransitionShown>(_On_ScreenTransitionShown, oneshot: true, isHighPriority: true);
			_transitionSceneInstances[transitionId].SubscribeOwner<ScreenTransitionHiding>(_On_ScreenTransitionHiding, oneshot: true, isHighPriority: true);
			_transitionSceneInstances[transitionId].SubscribeOwner<ScreenTransitionHidden>(_On_ScreenTransitionHidden, oneshot: true, isHighPriority: true);

			this.Emit<ScreenTransitionStarting>();

			_transitionSceneInstances[transitionId].Show();
		}
		else
		{
			throw new InvalidScreenTransitionException($"Transition ID {transitionId} is invalid!");
		}
	}

	public void ContinueTransition()
	{
		_transitionSceneInstances[_currentTransitionId].Hide();
	}

	public bool IsValidTransitionId(string transitionId)
	{
		if (_transitionScenes.TryGetValue(transitionId, out var t))
		{
			return true;
		}

		return false;
	}

	/**********************
	*  Callback methods  *
	**********************/
	
	public void _On_ScreenTransitionShowing(IEvent e)
	{
		LoggerManager.LogDebug("Screen transition showing");
	}

	public void _On_ScreenTransitionShown(IEvent e)
	{
		LoggerManager.LogDebug("Screen transition shown");

		this.Emit(e);
	}

	public void _On_ScreenTransitionHiding(IEvent e)
	{
		LoggerManager.LogDebug("Screen transition hiding");
	}

	public void _On_ScreenTransitionHidden(IEvent e)
	{
		LoggerManager.LogDebug("Screen transition hidden");

		this.Emit<ScreenTransitionFinished>();
	}

	/****************
	*  Exceptions  *
	****************/
	
	public class InvalidScreenTransitionException : Exception
	{
		public InvalidScreenTransitionException() {}
		public InvalidScreenTransitionException(string message) : base(message) {}
		public InvalidScreenTransitionException(string message, Exception inner) : base(message, inner) {}
	}
}

public partial class SceneTransitionChainItem {
	public string Scene;
	public string Transition;
}
