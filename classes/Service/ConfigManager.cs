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
using GodotEGP.DAL.Endpoints;
using GodotEGP.Objects.Extensions;

public partial class ConfigManager : Service
{
	private string _configBaseDir = "Config";
	public string ConfigBaseDir
	{
		get { return _configBaseDir; }
		set { _configBaseDir= value; }
	}

	public bool UseGlobalConfig { get; set; } = true;

	private List<String> _configDataDirs { get; set; }

	private Dictionary<Type, Config.ConfigObject> _configObjects = new Dictionary<Type, Config.ConfigObject>();

	private Dictionary<string, FileSystemWatcher> _filesystemWatchers { get; set; }
	private bool _configReload { get; set; }

	private bool _serviceReadyInitial { get; set; }

	public ConfigManager() : base()
	{
		_configDataDirs = new List<string>();
		_filesystemWatchers = new();
		_configReload = false;

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
		if (_configReload)
		{
			_configReload = false;
			LoggerManager.LogDebug("Reloading config files");

			DiscoveryConfigFiles();
		}
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
			Config.ConfigLoader configLoader = new Config.ConfigLoader(fileQueue);

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

			this.Emit<ConfigManagerLoaderCompleted>((eee) => eee.SetConfigObjects(ec.ConfigObjects));
		}
	}

	public void _On_ConfigManagerLoaderError(IEvent e)
	{
		if (e is ConfigManagerLoaderError ee)
		{
			this.Emit<ConfigManagerLoaderError>();
			throw ee.RunWorkerCompletedEventArgs.Error;
		}
	}

	public void MergeConfigObjects(List<Config.ConfigObject> configObjects)
	{
    	foreach (Config.ConfigObject obj in configObjects)
    	{
        	Type type = obj.RawValue.GetType();

        	if (GetConfigObjectInstance(type).RawValue is VObject vo)
        	{
        		LoggerManager.LogDebug("Merging config object", "", "objEndpoint", obj.DataEndpoint);
        		vo.MergeFrom(obj.RawValue as VObject);
        		// LoggerManager.LogDebug("Merged config object", "", "obj", vo);
        	}

			// set the data endpoint
        	GetConfigObjectInstance(type).DataEndpoint = obj.DataEndpoint;
    	}
	}

	public bool RegisterConfigObjectInstance(Type configInstanceType, Config.ConfigObject configFileObject)
	{
		// return true if we added the object
		if (_configObjects.TryAdd(configInstanceType, configFileObject))
		{
			LoggerManager.LogDebug("Registering config file object", "", "obj", configInstanceType.Name);

			return true;
		}

		return false;
	}

	public void SetConfigObjectInstance(Config.ConfigObject configFileObject)
	{
		_configObjects[configFileObject.GetType()] = configFileObject;
	}

	public Config.ConfigObject GetConfigObjectInstance(Type configInstanceType)
	{
		if(!_configObjects.TryGetValue(configInstanceType, out Config.ConfigObject obj))
		{
			LoggerManager.LogDebug("Creating config file object", "", "objType", configInstanceType.FullName);

			obj = Config.ConfigObject.Create(configInstanceType.ToString());
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

	public void SaveConfigObjectInstance(Type configInstanceType, IDataEndpoint dataEndpoint = null)
	{
		// generate default filepath for type we are saving
		if (dataEndpoint == null)
		{
			dataEndpoint = GetDefaultSaveEndpoint(configInstanceType);
		}

		Config.ConfigObject configObject = GetConfigObjectInstance(configInstanceType);

		configObject.DataEndpoint = dataEndpoint;
		configObject.Save();

		configObject.SubscribeOwner<DataOperationComplete>(_On_SaveConfigCompleted, isHighPriority:true, oneshot:true);
		configObject.SubscribeOwner<DataOperationError>(_On_SaveConfigError, isHighPriority:true, oneshot:true);
	}

	public IDataEndpoint GetDefaultSaveEndpoint(Type configInstanceType, string configName = null)
	{
		// return last used endpoint if no config name is provided
		if (configName == null)
		{
			return GetConfigObjectInstance(configInstanceType).DataEndpoint;
		}

		return new FileEndpoint(Path.Combine(OS.GetUserDataDir(), _configBaseDir, configInstanceType.Namespace+"."+configInstanceType.Name, configName));
	}

	public void Save<T>(IDataEndpoint dataEndpoint = null) where T : VObject
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
		if (!_serviceReadyInitial && UseGlobalConfig)
		{
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

		// setup filesystem watchers
		foreach (var dataDir in _configDataDirs)
		{
			string watchPath = Path.Combine(dataDir, _configBaseDir);

			if (!_filesystemWatchers.ContainsKey(dataDir) && Directory.Exists(watchPath))
			{
				var watcher = new FileSystemWatcher(watchPath);

				watcher.NotifyFilter = NotifyFilters.Attributes
                                 		| NotifyFilters.CreationTime
                                 		| NotifyFilters.DirectoryName
                                 		| NotifyFilters.FileName
                                 		| NotifyFilters.LastWrite
                                 		| NotifyFilters.Size;

				watcher.Changed += _On_ConfigDirChanged;
				watcher.Created += _On_ConfigDirChanged;
				watcher.Deleted += _On_ConfigDirChanged;
				watcher.Renamed += _On_ConfigDirChanged;
				watcher.Error += _On_ConfigDirError;
				watcher.Filter = "*.json";

				watcher.IncludeSubdirectories = true;
        		watcher.EnableRaisingEvents = true;

				_filesystemWatchers.Add(dataDir, watcher);
			}
		}
	
		_serviceReadyInitial = true;
	}

	private void _On_ConfigDirChanged(object sender, FileSystemEventArgs e)
    {
    	QueueConfigReload();

        LoggerManager.LogDebug($"Config dir changed: {e.FullPath}");

        this.Emit<ConfigManagerDirectoryChanged>((ee) => ee.SetDirectory(e.FullPath));
    }

    private void QueueConfigReload()
    {
    	_configReload = true;
    }

    private void _On_ConfigDirError(object sender, ErrorEventArgs e)
    {
        LoggerManager.LogError("Config dir watcher error", "", "error", e.GetException());

        this.Emit<ConfigManagerDirectoryError>();
    }

	private void _On_SaveConfigCompleted(DataOperationComplete e)
	{
        LoggerManager.LogError("Config save completed", "", "object", e.Owner.GetType());

        this.Emit<ConfigManagerSaveCompleted>((ee) => ee.SetConfigObject((ConfigObject) e.Owner));
	}
	private void _On_SaveConfigError(DataOperationError e)
	{
        LoggerManager.LogError("Config save failed", "", "error", e.Exception);

        this.Emit<ConfigManagerSaveError>((ee) => ee.Exception = e.Exception);
	}
}
