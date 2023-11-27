namespace GodotEGP.Handler;

using Godot;

using GodotEGP.Service;
using GodotEGP.Config;
using GodotEGP.Logging;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;
using GodotEGP.Objects.Extensions;

using GodotEGP.Resource;

public partial class ConfigHandler : Handler
{
	public ConfigHandler()
	{
		// subscribe to services ready so that we can set initial configs for
		// services
		ServiceRegistry.Get<EventManager>().Subscribe<ServiceReady>(_On_ConfigManager_Ready).Filters(new OwnerObjectType(typeof(ConfigManager)));

		ServiceRegistry.Get<EventManager>().Subscribe<ServiceReady>(_On_ResourceManager_Ready).Filters(new OwnerObjectType(typeof(ResourceManager)));
	}

	public void _On_ConfigManager_Ready(IEvent e)
	{
		// subscribe to changes on EngineConfig now that configmanage is ready
		EngineConfig ec = ServiceRegistry.Get<ConfigManager>().Get<EngineConfig>();
		ec.SubscribeOwner<ValidatedValueChanged>(_On_EngineConfig_ValueChanged, isHighPriority: true);

		// set save data manager config
		ServiceRegistry.Get<SaveDataManager>().SetConfig(ec.SaveDataManager);

		// set resource manager config
		ServiceRegistry.Get<ResourceManager>().SetConfig(ServiceRegistry.Get<ConfigManager>().Get<ResourceDefinitionConfig>());

		// set inputmanager mapping config
		ServiceRegistry.Get<InputManager>().SetMappingConfig(ServiceRegistry.Get<ConfigManager>().Get<GlobalConfig>().InputMapping);

		// trigger EngineConfig changed event
		_On_EngineConfig_Changed(ec);
	}
	public void _On_ResourceManager_Ready(IEvent e)
	{
		// set scene definitions from loaded resources
		ServiceRegistry.Get<SceneManager>().SetConfig(ServiceRegistry.Get<ResourceManager>().GetResources<PackedScene>());

		// set scene definitions for ScreenTransitionService using TryGetCategory
		if (ServiceRegistry.Get<ResourceManager>().TryGetCategory("TransitionScenes", out var sceneResources))
		{
			ServiceRegistry.Get<ScreenTransitionManager>().SetConfig(sceneResources);
		}

		// set game scripts resources in ScriptService
		ServiceRegistry.Get<ScriptService>().SetConfig(ServiceRegistry.Get<ResourceManager>().GetResources<GameScript>());
	}

	public void _On_EngineConfig_ValueChanged(IEvent e)
	{
		if (e is ValidatedValueChanged ev)
		{
			if (ev.Owner is EngineConfig ec)
			{
				_On_EngineConfig_Changed(ec);
			}
		}
	}

	public void _On_EngineConfig_Changed(EngineConfig ec)
	{
		// set logger manager config
		LoggerManager.Instance.SetConfig(ec.LoggerManager);

		// set scene transition config
		ServiceRegistry.Get<SceneTransitionManager>().SetConfig(ec.SceneTransitionManager);

		// set input manager config
		ServiceRegistry.Get<InputManager>().SetConfig(ec.InputManager);
	}
}
