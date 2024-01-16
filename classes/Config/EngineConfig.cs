namespace GodotEGP.Config;

using GodotEGP.Objects.Validated;

public partial class EngineConfig : VConfig
{
	partial void InitConfigParams();

	private readonly VNative<LoggerConfig> _loggerManager;

	public LoggerConfig LoggerManager
	{
		get { return _loggerManager.Value; }
		set { _loggerManager.Value = value; }
	}

	internal readonly VNative<SaveDataManagerConfig> _saveDataManager;

	public SaveDataManagerConfig SaveDataManager
	{
		get { return _saveDataManager.Value; }
		set { _saveDataManager.Value = value; }
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
        _loggerManager = AddValidatedNative<LoggerConfig>(this)
        	.Default(new LoggerConfig(this))
        	.ChangeEventsEnabled();

		_saveDataManager = AddValidatedNative<SaveDataManagerConfig>(this)
		    .Default(new SaveDataManagerConfig())
		    .ChangeEventsEnabled();

		_sceneTransitionManager = AddValidatedNative<SceneTransitionManagerConfig>(this)
		    .Default(new SceneTransitionManagerConfig())
		    .ChangeEventsEnabled();

		_inputManager = AddValidatedNative<InputManagerConfig>(this)
		    .Default(new InputManagerConfig())
		    .ChangeEventsEnabled();

		InitConfigParams();
	}
}
