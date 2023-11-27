namespace GodotEGP.Service;

using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using GodotEGP.Objects.Validated;
using GodotEGP.Service;
using GodotEGP.Logging;
using GodotEGP.Config;
using GodotEGP.Event.Events;
using GodotEGP.Data.Endpoint;
using GodotEGP.Objects.Extensions;

public partial class ConfigManager : Service
{
	private string _configBaseDir = "Config";
	public string ConfigBaseDir
	{
		get { return _configBaseDir; }
		set { _configBaseDir= value; }
	}

	private List<String> _configDataDirs { get; set; }

	private Dictionary<Type, Config.Object> _configObjects = new Dictionary<Type, Config.Object>();

	public ConfigManager() : base()
	{
		_configDataDirs = new List<string>();
		AddConfigDataDir(ProjectSettings.GlobalizePath("res://"));
		AddConfigDataDir(OS.GetUserDataDir());
	}

	public void AddConfigDataDir(string dataDir)
	{
		LoggerManager.LogDebug("Adding config data directory", "", "dir", dataDir);
		_configDataDirs.Add(dataDir);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
		DiscoveryConfigFiles();
	}

	public void DiscoveryConfigFiles()
	{
		Queue<Dictionary<string, object>> fileQueue = new Queue<Dictionary<string, object>>();

		foreach (string configDataPath in _configDataDirs)
		{
			string configPath = Path.Combine(configDataPath, _configBaseDir);

			if (Directory.Exists(configPath))
			{
				DirectoryInfo d = new DirectoryInfo(configPath);
				foreach (DirectoryInfo dir in d.GetDirectories())
				{
					string configDirName = dir.ToString().GetFile();
					Type configDirType = Type.GetType(configDirName);

					LoggerManager.LogDebug("Config manager config directory", "", "dirName", configDirName);
					LoggerManager.LogDebug("Type valid", "", "typeValid", configDirType);

					// if it's a valid config object, and if the base type is
					// ValidatedObject, then let's load the content
					if (configDirType != null && configDirType.BaseType.Equals(typeof(VConfig)))
					{
						// trigger creation of base object in the register
						// before queueing files for loading
						GetConfigObjectInstance(Type.GetType(configDirName.ToString()));

						foreach (FileInfo file in dir.GetFiles("*.json").OrderBy((f) => f.ToString()))
						{
							LoggerManager.LogDebug("Queueing file for content load", "", "file", file.ToString());

							fileQueue.Enqueue(new Dictionary<string, object> {{"configType", configDirName}, {"path", file.ToString()}});
						}
					}
				}
			}
			else
			{
				LoggerManager.LogDebug("Config path doesn't exist", "", "path", configPath);
			}
		}

		if (fileQueue.Count > 0)
		{
			// load all the config objects using ConfigManagerLoader
			Config.Loader configLoader = new Config.Loader(fileQueue);

			configLoader.SubscribeOwner<ConfigManagerLoaderCompleted>(_On_ConfigManagerLoaderCompleted, oneshot: true, isHighPriority: true);
			configLoader.SubscribeOwner<ConfigManagerLoaderError>(_On_ConfigManagerLoaderError, oneshot: true, isHighPriority: true);
		}
	}

	public void _On_ConfigManagerLoaderCompleted(IEvent e)
	{
		if (e is ConfigManagerLoaderCompleted ec)
		{
			// LoggerManager.LogDebug("ConfigManager: loader completed cb", "", "e", ec.ConfigObjects);	
			LoggerManager.LogDebug("ConfigManager: loader completed cb", "", "loadedCount", ec.ConfigObjects.Count);	

			MergeConfigObjects(ec.ConfigObjects);

			_SetServiceReady(true);
		}
	}

	public void _On_ConfigManagerLoaderError(IEvent e)
	{
		if (e is ConfigManagerLoaderError ee)
		{
			throw ee.RunWorkerCompletedEventArgs.Error;
		}
	}

	public void MergeConfigObjects(List<Config.Object> configObjects)
	{
    	foreach (Config.Object obj in configObjects)
    	{
        	Type type = obj.RawValue.GetType();

        	if (GetConfigObjectInstance(type).RawValue is VObject vo)
        	{
        		LoggerManager.LogDebug("Merging config object", "", "objEndpoint", obj.DataEndpoint);
        		vo.MergeFrom(obj.RawValue as VObject);
        		// LoggerManager.LogDebug("Merged config object", "", "obj", vo);
        	}
    	}
	}

	public bool RegisterConfigObjectInstance(Type configInstanceType, Config.Object configFileObject)
	{
		// return true if we added the object
		if (_configObjects.TryAdd(configInstanceType, configFileObject))
		{
			LoggerManager.LogDebug("Registering config file object", "", "obj", configInstanceType.Name);

			return true;
		}

		return false;
	}

	public void SetConfigObjectInstance(Config.Object configFileObject)
	{
		_configObjects[configFileObject.GetType()] = configFileObject;
	}

	public Config.Object GetConfigObjectInstance(Type configInstanceType)
	{
		if(!_configObjects.TryGetValue(configInstanceType, out Config.Object obj))
		{
			LoggerManager.LogDebug("Creating config file object", "", "objType", configInstanceType.Name);

			obj = Config.Object.Create(configInstanceType.ToString());
			RegisterConfigObjectInstance(configInstanceType, obj);

			return obj;
		}

		return obj;
	}

	public T GetConfigObjectValue<T>()
	{
		return (T) GetConfigObjectInstance(typeof(T)).RawValue;
	}

	public T Get<T>() where T : VObject
	{
		return (T) GetConfigObjectValue<T>();
	}

	public void SaveConfigObjectInstance(Type configInstanceType, IEndpoint dataEndpoint = null)
	{
		// generate default filepath for type we are saving
		if (dataEndpoint == null)
		{
			dataEndpoint = GetDefaultSaveEndpoint(configInstanceType);
		}

		Config.Object configObject = GetConfigObjectInstance(configInstanceType);

		configObject.DataEndpoint = dataEndpoint;
		configObject.Save();
	}

	public IEndpoint GetDefaultSaveEndpoint(Type configInstanceType)
	{
		return new FileEndpoint(Path.Combine(OS.GetUserDataDir(), _configBaseDir, configInstanceType.Namespace+"."+configInstanceType.Name, "Config.json"));
	}

	public void Save<T>(IEndpoint dataEndpoint = null) where T : VObject
	{
		SaveConfigObjectInstance(typeof(T), dataEndpoint);
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
		// create instance of GlobalConfig if config doesn't exist already
		if (!_configObjects.TryGetValue(typeof(GlobalConfig), out var obj))
		{
			LoggerManager.LogDebug("Creating default config instance", "", "type", typeof(GlobalConfig).Name);

			var globalConfig = GetConfigObjectInstance(typeof(GlobalConfig));
			globalConfig.DataEndpoint = GetDefaultSaveEndpoint(globalConfig.GetType());

			Save<GlobalConfig>();
		}
		else
		{
			// save it so we include any new values
			Save<GlobalConfig>();
		}
	}
}
