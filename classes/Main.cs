namespace GodotEGP;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.State;
using System;
using System.Collections.Generic;
using GodotEGP.Objects.Validated;
using Newtonsoft.Json;
using System.Net.Http;

using GodotEGP.Service;
using GodotEGP.Logging;
using GodotEGP.Event;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filters;
using GodotEGP.Config;
using GodotEGP.Handler;
using GodotEGP.DAL.Operations;

public partial class Main : Node
{
	public Main()
	{
		// create instance of ServiceRegistry
		AddChild(new ServiceRegistry());

		// register LoggerManager singleton as service to trigger ready state
		// ServiceRegistry.Instance.RegisterService(LoggerManager.Instance);

		// create instance of ConfigHandler to handle setting configs
		AddChild(new ConfigHandler());

		// trigger lazy load ConfigManager to trigger initial load
		ServiceRegistry.Get<DataService>();
		ServiceRegistry.Get<ConfigManager>();
		ServiceRegistry.Get<InputManager>();
		ServiceRegistry.Get<ResourceManager>();
		ServiceRegistry.Get<SceneManager>();
		ServiceRegistry.Get<SceneTransitionManager>();
		ServiceRegistry.Get<ScreenTransitionManager>();
		ServiceRegistry.Get<ScriptService>();
		ServiceRegistry.Get<NodeManager>();
		ServiceRegistry.Get<SaveDataManager>();
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoggerManager.LogInfo("GodotEGP ready!");
	}

	public void _On_ConfigManager_Ready(IEvent e)
	{
		LoggerManager.LogDebug("ConfigManager service ready");

	}
}
