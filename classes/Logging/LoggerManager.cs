namespace GodotEGP.Logging;

using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;

using GodotEGP.Objects.Extensions;
using GodotEGP.Config;
using GodotEGP.Service;
using GodotEGP.Logging;
using GodotEGP.Logging.Destination;
using GodotEGP.Event.Events;

/// <summary>
/// Manage instances of <c>Logger</c> objects based on class type.
/// </summary>
public partial class LoggerManager : Service
{
	// Lazy singleton instance
	private static readonly Lazy<LoggerManager> _instance = 
		new Lazy<LoggerManager>(
			() => new LoggerManager(), isThreadSafe: true
		);

	public static LoggerManager Instance {
		get { return _instance.Value; }
	}

	private LoggerConfig _loggerConfig;
	private LoggerConfig Config
	{
		get { 
			return _loggerConfig;
		}
		set { 
			_loggerConfig = value;

			OnConfigObjectUpdated();
		}
	}

	// Default LoggerDestinationCollection instance used for new Logger
	// instances
	private DestinationCollection _loggerDestinationCollectionDefault;

	public DestinationCollection LoggerDestinationCollectionDefault
	{
		get { 
			if (_loggerDestinationCollectionDefault == null)
			{
				_loggerDestinationCollectionDefault = new DestinationCollection();

				// Add Godot console as default destination
				_loggerDestinationCollectionDefault.AddDestination(new GodotConsole());
			}

			return _loggerDestinationCollectionDefault;
		}
		set { _loggerDestinationCollectionDefault = value; }
	}

	private LoggerManager() {
		// use default values for logger config
		AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
		{
			LogError(eventArgs.Exception.GetType().Name, eventArgs.Exception.TargetSite.Name, "exceptionData", eventArgs.Exception.Data);
			LogError(eventArgs.Exception.Message, eventArgs.Exception.TargetSite.Name, "stackTrace", eventArgs.Exception.StackTrace);
		};
	}

	public override void _Ready()
	{
		_SetServiceReady(true);
	}

	public override void _OnServiceRegistered()
	{
		Instance._loggerConfig = new LoggerConfig();
	}

	// Dictionary of Logger instances
	private Dictionary<Type, Logger> _loggers = new Dictionary<Type, Logger>();

	/// <summary>
	/// Log a message with the given <c>LogLevel</c>.
	/// Automatically creates or forwards Log request to <c>Logger</c> instance.
	/// </summary>
	private static void _Log(Message.LogLevel logLevel = Message.LogLevel.Debug, 
			string logMessage = "", 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			string sourceFilename = "",
			int sourceLineNumber = 0
		)
	{
		// StackFrame instance 2 back, because that's the caller of the outter
		// logging method not this object
		StackFrame frame = new StackFrame(2);
		Type sourceType = frame.GetMethod().DeclaringType;
		string sourceMethodName = frame.GetMethod().Name;
		string sourceName = sourceType.Name;

		// Queue the LoggerMessage object for logging if the message is within
		// the allowed current log level value
		var currentLogLevel = Message.DefaultLogLevel;
		if (Instance.Config != null)
		{
			currentLogLevel = Instance.Config.GetMatchingLogLevelOverride(sourceType.Name);
		}

		if (logLevel >= currentLogLevel)
		{
			GetLoggerInstance(sourceType).ProcessLoggerMessage(new Message(logLevel, logMessage, logCustom, logDataName, logData, sourceName, sourceMethodName, sourceFilename, sourceLineNumber));
		}
	}

	public static Logger GetLoggerInstance(Type loggerType)
	{
        if (!Instance._loggers.TryGetValue(loggerType, out var obj) || obj is not Logger logger)
        {
            logger = new Logger(Instance.LoggerDestinationCollectionDefault);
            Instance._loggers.Add(loggerType, logger);

            LoggerManager.LogDebug($"Creating Logger instance", "", "instanceName", loggerType.FullName);
        }

        return logger;
	}

	public static void SetLoggerDestinationCollection<T>(DestinationCollection ldc)
	{
		GetLoggerInstance(typeof(T)).LoggerDestinationCollection = ldc;
	}

	public void SetConfig(LoggerConfig config)
	{
		Config = config;
	}

	// update config object in various moving parts
	public void OnConfigObjectUpdated()
	{
		// LoggerManager.LogDebug("Config updated", "", "config", Config);
		LoggerManager.LogDebug("Config updated");
	}

	/***********************************
	*  Logging methods for LogLevels  *
	***********************************/
	public static void LogTrace( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(Message.LogLevel.Trace, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogDebug( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(Message.LogLevel.Debug, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogInfo( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(Message.LogLevel.Info, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogWarning( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(Message.LogLevel.Warning, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogError( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(Message.LogLevel.Error, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
	public static void LogCritical( 
			object logMessage, 
			string logCustom = "", 
			string logDataName = "", 
			object logData = null,
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilename = "",
        	[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
		)
	{
		_Log(Message.LogLevel.Critical, logMessage.ToString(), logCustom, logDataName, logData, sourceFilename, sourceLineNumber);
	}
}

