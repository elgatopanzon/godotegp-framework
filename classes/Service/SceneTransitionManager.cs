/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SceneTransitionManager
 * @created     : Sunday Nov 12, 2023 18:46:04 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class SceneTransitionManager : Service
{
	ScreenTransitionManager _transitionManager = ServiceRegistry.Get<ScreenTransitionManager>();
	SceneManager _sceneManager = ServiceRegistry.Get<SceneManager>();

	SceneTransitionManagerConfig _config;

	private string _currentChainId;
	private int _currentChainIdx;

	private string _currentTransitionSceneId;
	private bool _currentAutoContinue;

	public SceneTransitionManager()
	{
		
	}

	public void SetConfig(SceneTransitionManagerConfig config)
	{
		_config = config;

		LoggerManager.LogDebug("Setting config");

		_SetServiceReady(true);
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
	*  Scene transition management methods  *
	*****************************************/
	
	public void TransitionScene(string sceneId, string transitionId, bool autoContinue = true)
	{
		if (_sceneManager.IsValidScene(sceneId) && _transitionManager.IsValidTransitionId(transitionId))
		{
			LoggerManager.LogDebug("Starting scene transition", "", "transition", $"{sceneId} {transitionId}");

			_currentTransitionSceneId = sceneId;
			_currentAutoContinue = autoContinue;

			_transitionManager.StartTransition(transitionId);

			// subscribe to transition events
			_transitionManager.SubscribeOwner<ScreenTransitionStarting>(_On_TransitionScreen_Starting, oneshot: true);

			_transitionManager.SubscribeOwner<ScreenTransitionShown>(_On_TransitionScreen_Shown);


			_transitionManager.SubscribeOwner<ScreenTransitionFinished>((e) => {
					this.Emit(e);
				}, oneshot: true);
		}
		else
		{
			throw new InvalidSceneTransitionException($"The scene ID {sceneId} or transition ID {transitionId} is invalid!");
		}
	}

	/******************************
	*  Transition chain methods  *
	******************************/
	
	public void StartChain(string chainId)
	{
		if (IsValidChainId(chainId))
		{
			LoggerManager.LogDebug("Starting transition chain", "", "chain", chainId);

			_currentChainId = chainId;
			_currentChainIdx = -1;

			ContinueChain();
		}
		else
		{
			throw new InvalidTransitionChainException($"The chain ID {chainId} is not a valid transition chain!");
		}
	}

	public void ContinueChain()
	{
		_currentChainIdx++;

		// check if we're at the start of the chain
		if (_currentChainIdx == 0)
		{
			LoggerManager.LogDebug("Starting scene transition chain", "", "chainId", _currentChainId);

			this.Emit<SceneTransitionChainStarted>();
		}

		List<SceneTransitionChainItem> transitions = _config.TransitionChains[_currentChainId];

		// check if we reached the end of the chain yet
		if (transitions.Count > _currentChainIdx)
		{
			var t = transitions[_currentChainIdx];
			LoggerManager.LogDebug("Continuing scene transition chain", "", "chain", $"{_currentChainId} {_currentChainIdx} {t.Scene} {t.Transition}");

			TransitionScene(t.Scene, t.Transition);

			// subscribe to transition finished before emitting chain continued
			// event
			this.SubscribeOwner<ScreenTransitionFinished>((e) => {
				this.Emit<SceneTransitionChainContinued>();
				}, oneshot: true, isHighPriority: true);
		}
		else
		{
			LoggerManager.LogDebug("Scene transition chain finished", "", "chainId", _currentChainId);

			this.Emit<SceneTransitionChainFinished>();
		}
	}

	public bool IsValidChainId(string chainId)
	{
		return _config.TransitionChains.ContainsKey(chainId);
	}

	/**********************
	*  Callback methods  *
	**********************/
	
	public void _On_TransitionScreen_Starting(IEvent e)
	{
		this.Emit(e);
	}
	public void _On_TransitionScreen_Shown(IEvent e)
	{
		this.Emit(e);
		LoggerManager.LogDebug("Loading scene transition scene", "", "transition", $"{_currentTransitionSceneId}");

		_sceneManager.LoadScene(_currentTransitionSceneId);

		_sceneManager.SubscribeOwner<SceneLoaded>((e) => {
			if (_currentAutoContinue)
			{
				_transitionManager.ContinueTransition();
			}
		}, oneshot: true);
	}
	public void _On_TransitionScreen_Finished(IEvent e)
	{
		
	}

	/****************
	*  Exceptions  *
	****************/
	
	public class InvalidSceneTransitionException : Exception
	{
		public InvalidSceneTransitionException() {}
		public InvalidSceneTransitionException(string message) : base(message) {}
		public InvalidSceneTransitionException(string message, Exception inner) : base(message, inner) {}
	}

	public class InvalidTransitionChainException : Exception
	{
		public InvalidTransitionChainException() {}
		public InvalidTransitionChainException(string message) : base(message) {}
		public InvalidTransitionChainException(string message, Exception inner) : base(message, inner) {}
	}
}

