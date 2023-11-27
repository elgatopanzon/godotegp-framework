namespace GodotEGP.Config;

using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Validated;

public partial class LoggerConfig : VObject
{
	private readonly VValue<Logging.Message.LogLevel> _logLevel;

	public Logging.Message.LogLevel LogLevel
	{
		get { return _logLevel.Value; }
		set { _logLevel.Value = value; }
	}

	private readonly VValue<Dictionary<string, Logging.Message.LogLevel>> _logLevelOverrides;

	public Dictionary<string, Logging.Message.LogLevel> LogLevelOverrides
	{
		get { return _logLevelOverrides.Value; }
		set { _logLevelOverrides.Value = value; }
	}

	public LoggerConfig(VObject parent = null) : base(parent)
	{
        _logLevel = AddValidatedValue<Logging.Message.LogLevel>(this)
            .Default((OS.IsDebugBuild()) ? Logging.Message.LogLevel.Debug : Logging.Message.LogLevel.Info)
            .AllowedValues(Logging.Message.LogLevel.GetValues<Logging.Message.LogLevel>())
        	.ChangeEventsEnabled()
            ;

        _logLevelOverrides = AddValidatedValue<Dictionary<string, Logging.Message.LogLevel>>(this)
            .Default(new Dictionary<string, Logging.Message.LogLevel>() {
					{"EventManager", Logging.Message.LogLevel.Info},
    				{"EventFilter", Logging.Message.LogLevel.Info},
    				{"EventQueue", Logging.Message.LogLevel.Info},
    				{"VValue", Logging.Message.LogLevel.Info},
    				{"VObject", Logging.Message.LogLevel.Info},
    				{"VNative", Logging.Message.LogLevel.Info},
    				{"ObjectPool", Logging.Message.LogLevel.Info},
            	})
        	.ChangeEventsEnabled()
            ;
	}

	public Logging.Message.LogLevel GetMatchingLogLevelOverride(string match)
	{
		foreach (KeyValuePair<string, Logging.Message.LogLevel> levelOverride in LogLevelOverrides)
		{
			if (match.Contains(levelOverride.Key))
			{
				return levelOverride.Value;
			}
		}

		return LogLevel;
	}
}
