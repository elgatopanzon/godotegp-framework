/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SceneManager
 * @created     : Saturday Nov 11, 2023 22:36:53 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filters;
using GodotEGP.Config;
using GodotEGP.Resource;

public partial class SceneManager : Service
{
	private Dictionary<string, ResourceObject<PackedScene>> _sceneDefinitions = new Dictionary<string, ResourceObject<PackedScene>>();

	private string _currentSceneId {
		get {
			return _sceneIdHistory.Peek();
		}
		set {
			_sceneIdHistory.Push(value);
		}
	}
	private Node _currentSceneInstance;

	private Stack<string> _sceneIdHistory = new Stack<string>();

	public SceneManager()
	{
		
	}

	public void SetConfig(Dictionary<string, ResourceObject<PackedScene>> config)
	{
		LoggerManager.LogDebug("Setting scene definition config", "", "scenes", config.Keys);
		
		_sceneDefinitions = config;

		if (!GetReady())
		{
			_SetServiceReady(true);
		}

		SetSceneIdFromLoadedScene();
	}

	/*******************
	*  Godot methods  *
	*******************/
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// _SetServiceReady(true);
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


	/******************************
	*  Scene management methods  *
	******************************/

	public void SetSceneIdFromLoadedScene()
	{
		// check for any loaded scenes such as the main scene, and if they match
		// any scene resources set the current scene ID
		var currentScene = GetTree().CurrentScene;
		
		if (currentScene != null && currentScene.SceneFilePath.Length > 0 && SceneDefinitionExists(currentScene.SceneFilePath))
		{
			// find the scene resource with matching ID to the one with the
			// current path, so we can find the short ID of it
			// TODO: improve this code somehow
			foreach (var sceneD in _sceneDefinitions)
			{
				if (sceneD.Key == currentScene.SceneFilePath)
				{
					foreach (var sceneDD in _sceneDefinitions)
					{
						if (sceneDD.Value == sceneD.Value && sceneDD.Key != currentScene.SceneFilePath)
						{
							_currentSceneId = sceneDD.Key;

							LoggerManager.LogDebug("Setting current scene id to loaded scene", "", "currentSceneId", _currentSceneId);
						}
					}
				}
			}

			LoggerManager.LogDebug("Setting current scene instance");

			_currentSceneInstance = currentScene;
		}

	}
	
	public void LoadScene(string sceneId)
	{
		if (SceneDefinitionExists(sceneId))
		{
			LoggerManager.LogDebug("Loading scene", "", "sceneId", sceneId);

			// if there's already a current scene, then load it deferred
			if (_currentSceneInstance != null)
			{
				LoggerManager.LogDebug("Current scene type", "", "sceneType", _currentSceneInstance.GetType().Name);

				// UnloadManagedScenes();
				_currentSceneInstance.SubscribeSignal("tree_exited", false, _On_NodeRemoved, oneshot: true);
				_currentSceneInstance.QueueFree();
				this.Emit<SceneUnloaded>((e) => e.SetSceneId(_currentSceneId));

			}
			// if there's no current scene, just call deferred add
			else
			{
				CallDeferred("AddCurrentScene");
			}

			_currentSceneId = sceneId;
			_currentSceneInstance = GetSceneInstance(sceneId);
		}
		else
		{
			if (Godot.FileAccess.FileExists(sceneId))
			{
				// create a defition for it
				AddSceneDefinition(sceneId);

				// load it again
				LoadScene(sceneId);
			}

			// if the scene definition doesn't exist and it's not a valid scene
			// file
			else
			{
				throw new InvalidSceneException($"Invalid scene ID {sceneId}");
			}
		}
	}

	public void AddSceneDefinition(string scenePath)
	{
		// add a scene definition
		if (!_sceneDefinitions.TryGetValue(scenePath, out var resource))
		{
			LoggerManager.LogDebug("Adding definition for scene path", "", "scenePath", scenePath);

			_sceneDefinitions.Add(scenePath, new() {
				Value = GD.Load<PackedScene>(scenePath)
			});
		}
	}

	public void AddCurrentScene()
	{
		AddChild(_currentSceneInstance);
		this.Emit<SceneLoaded>((e) => {
				e.SetSceneId(_currentSceneId);
				e.SetSceneInstance(_currentSceneInstance);
			});
	}

	public Node GetSceneInstance(string sceneId)
	{
		if (SceneDefinitionExists(sceneId))
		{
			if (_sceneDefinitions[sceneId].RawValue is PackedScene ps)
			{
				return ps.Instantiate();
			}
		}

		return null;
	}

	public bool SceneDefinitionExists(string sceneId)
	{
		return _sceneDefinitions.ContainsKey(sceneId);
	}

	public void UnloadManagedScenes()
	{
		foreach (Node node in ServiceRegistry.Get<NodeManager>().GetSceneTreeNodes())
		{
			if (node.SceneFilePath.Length > 0 && SceneDefinitionExists(node.SceneFilePath))
			{
				node.QueueFree();
			}
		}
	}

	public void LoadPreviousScene()
	{
		if (_sceneIdHistory.TryPop(out string prevId))
		{
			LoggerManager.LogDebug("Loading previous scene", "", "scene", prevId);

			LoadScene(prevId);
		}
	}

	public void ReloadCurrentScene()
	{
		LoadScene(_currentSceneId);

		// prevent history from including the reload
		_sceneIdHistory.Pop();
	}

	/**********************
	*  Callback methods  *
	**********************/
	
	public void _On_NodeRemoved(IEvent e)
	{
		CallDeferred("AddCurrentScene");
	}

	/****************
	*  Exceptions  *
	****************/
	
	public class InvalidSceneException : Exception
	{
		public InvalidSceneException() {}
		public InvalidSceneException(string message) : base(message) {}
		public InvalidSceneException(string message, Exception inner) : base(message, inner) {}
	}
}

