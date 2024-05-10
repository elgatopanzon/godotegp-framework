namespace GodotEGP.Config;

using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Validated;

public partial class LoggerConfig : VObject
{
	partial void InitConfigParams();

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
            .Default(Logging.Message.DefaultLogLevel)
            .AllowedValues(Logging.Message.LogLevel.GetValues<Logging.Message.LogLevel>())
        	.ChangeEventsEnabled()
            ;

        _logLevelOverrides = AddValidatedValue<Dictionary<string, Logging.Message.LogLevel>>(this)
            .Default(new Dictionary<string, Logging.Message.LogLevel>() {
					{"EventManager", Logging.Message.LogLevel.Trace},
    				{"EventFilter", Logging.Message.LogLevel.Trace},
    				{"EventQueue", Logging.Message.LogLevel.Trace},
    				{"VValue", Logging.Message.LogLevel.Trace},
    				{"VObject", Logging.Message.LogLevel.Trace},
    				{"VNative", Logging.Message.LogLevel.Trace},
    				{"ObjectPool", Logging.Message.LogLevel.Trace},
            	})
        	.ChangeEventsEnabled()
            ;

        _logLevelOverrides.MergeCollections = true;

        InitConfigParams();
	}

	public bool GetMatchingLogLevelOverride(string match, out Logging.Message.LogLevel level)
	{
		level = LogLevel;
		foreach (KeyValuePair<string, Logging.Message.LogLevel> levelOverride in LogLevelOverrides)
		{
			if (match.Contains(levelOverride.Key))
			{
				level = levelOverride.Value;
				return true;
			}
		}

		return false;
	}
}
