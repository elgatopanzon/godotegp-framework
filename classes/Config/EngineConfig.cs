namespace GodotEGP.Config;

using GodotEGP.Objects.Validated;

public partial class EngineConfig : VConfig
{
	private readonly VNative<LoggerConfig> _loggerManagerConfig;

	public LoggerConfig LoggerManager
	{
		get { return _loggerManagerConfig.Value; }
		set { _loggerManagerConfig.Value = value; }
	}

	internal readonly VNative<SaveDataManagerConfig> _saveDataManagerConfig;

	public SaveDataManagerConfig SaveDataManager
	{
		get { return _saveDataManagerConfig.Value; }
		set { _saveDataManagerConfig.Value = value; }
	}

	internal readonly VNative<SceneTransitionManagerConfig> _sceneTransitionManager;

	public SceneTransitionManagerConfig SceneTransitionManager
	{
		get { return _sceneTransitionManager.Value; }
		set { _sceneTransitionManager.Value = value; }
	}

	internal readonly VNative<InputManagerConfig> _inputManager;

	public InputManagerConfig InputManager
	{
		get { return _inputManager.Value; }
		set { _inputManager.Value = value; }
	}

	public EngineConfig()
	{
        _loggerManagerConfig = AddValidatedNative<LoggerConfig>(this)
        	.Default(new LoggerConfig(this))
        	.ChangeEventsEnabled();

		_saveDataManagerConfig = AddValidatedNative<SaveDataManagerConfig>(this)
		    .Default(new SaveDataManagerConfig())
		    .ChangeEventsEnabled();

		_sceneTransitionManager = AddValidatedNative<SceneTransitionManagerConfig>(this)
		    .Default(new SceneTransitionManagerConfig())
		    .ChangeEventsEnabled();

		_inputManager = AddValidatedNative<InputManagerConfig>(this)
		    .Default(new InputManagerConfig())
		    .ChangeEventsEnabled();
	}
}
