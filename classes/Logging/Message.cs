namespace GodotEGP.Logging;

using Godot;
using System;
using System.Collections.Generic;

public partial class Message
{
	public DateTime Created { get; set; }
	public ulong TicksMsec { get; set; }
	public LogLevel Level { get; set; }
	public string Text { get; set; }
	public string Custom { get; set; }
	public string DataName { get; set; }
	public object Data { get; set; }

	public string SourceName { get; set; }
	public string SourceMethodName { get; set; }
	public string SourceFilename { get; set; }
	public int SourceLineNumber { get; set; }

	public Dictionary<string, object> Formatted = new Dictionary<string, object>();

	public enum LogLevel {
		Trace = 0,
		Debug = 1,
		Info = 2,
		Warning = 3,
		Error = 4,
		Critical = 5
	}

	public static LogLevel DefaultLogLevel
	{
		get { 
			if (OS.IsDebugBuild())
			{
				return LogLevel.Debug;
			}
			else
			{
				return LogLevel.Info;
			}
		}
	}

	public Message(LogLevel level, string text, string custom = "", string dataName = "", object data = null, string sourceName = "", string sourceMethodName = "", string sourceFilename = "", int sourceLineNumber = 0)
	{
		Created = DateTime.Now;
		TicksMsec = Time.GetTicksMsec();
		Level = level;
		Text = text;
		Custom = custom;
		DataName = dataName;
		Data = data;
		SourceName = sourceName;
		SourceMethodName = sourceMethodName;
		SourceFilename = sourceFilename;
		SourceLineNumber = sourceLineNumber;
	}
}
