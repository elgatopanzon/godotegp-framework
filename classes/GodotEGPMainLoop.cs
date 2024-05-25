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

using System.Linq;
using System.Threading.Tasks;

using GodotEGP.ECSv4;

#if GODOT
[GlobalClass]
public partial class GodotEGPMainLoop : SceneTree
{
	// command line args
	private string[] _args;

	// main instance of the GodotEGP framework
	private Main _main;

	// string resource path for the startup scene
	private string _startScene;

	// count for number of services that need to be ready
	private int _serviceReadyCount;
	private int _serviceReadyCountCurrent;

	// partial methods to allow partial overriding
	partial void _On_Initialize();
	partial void _On_Process();
	partial void _On_Finalize();

	public GodotEGPMainLoop()
	{
		_args = OS.GetCmdlineArgs();

		LoggerManager.LogInfo("GodotEGP main loop starting");
		LoggerManager.LogInfo("Command line args", "", "args", _args);

		// set the startup scene from the cmd args
		if (_args.Length >= 1)
		{
			_startScene = _args[0];
		}

		_main = new();

		// lazy register ECS service
		ServiceRegistry.Get<ECS>();
	}

	public override void _Initialize()
    {
		LoggerManager.LogInfo("GodotEGP main loop initialize");

		// add the main instance to the scene tree
		Root.AddChild(_main);

		_On_Initialize();

		// unload the automatically loaded scene so we can wait until GodotEGP
		// is ready
		// TODO: load a loading scene here instead while waiting for services
		UnloadCurrentScene();

		// register events for all services which are not ready yet
		_serviceReadyCount = ServiceRegistry.Instance.Services.Count;
		foreach (var service in ServiceRegistry.Instance.Services)
		{
			if (!service.Value.GetReady())
			{
				LoggerManager.LogWarning("Service is not ready", "", "service", service.Key.Name);

				service.Value.SubscribeOwner<ServiceReady>(_On_ServiceReady, oneshot:true);
			}
			else
			{
				_On_ServiceReady(new ServiceReady() { Owner = service.Value });
			}
		}
    }

    public void _On_ServiceReady(ServiceReady e)
    {
		_serviceReadyCountCurrent++;

		LoggerManager.LogInfo("Service is now ready", $"{_serviceReadyCountCurrent} / {_serviceReadyCount}", "service", e.Owner.GetType().Name);

		if (_serviceReadyCountCurrent >= _serviceReadyCount)
		{
			LoggerManager.LogInfo("All services ready!");

			ChangeSceneToFile(_startScene);
		}
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
