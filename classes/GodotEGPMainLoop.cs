/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : MainLoop
 * @created     : Monday May 20, 2024 12:09:52 CST
 */

namespace GodotEGP.MainLoop;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4;

#if GODOT
[GlobalClass]
public partial class GodotEGPMainLoop : SceneTree
{
	private Main _main;

	partial void _On_Initialize();
	partial void _On_Process();
	partial void _On_Finalize();

	public GodotEGPMainLoop()
	{
		LoggerManager.LogInfo("GodotEGP main loop starting");

		_main = new();

		// lazy register ECS service
		ServiceRegistry.Get<ECS>();
	}

	public override void _Initialize()
    {
		LoggerManager.LogInfo("Main loop initialize");

		_On_Initialize();
    }

    public override bool _Process(double delta)
    {
    	_On_Process();

    	return base._Process(delta);
    }

    private new void _Finalize()
    {
		LoggerManager.LogInfo("Main loop finalize");

		_On_Finalize();
    }
    
}
#endif
