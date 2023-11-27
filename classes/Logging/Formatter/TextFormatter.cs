namespace GodotEGP.Logging.Formatter;

using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using GodotEGP.Logging.Formatter;

public partial class TextFormatter : IFormatter
{
	protected string _createdTimeFormat = "dd MMM yyyy HH:mm:ss";
	protected string _separator = " | ";

	protected Dictionary<string, List<Func<string, Logging.Message, string>>> _propertyFormatterFuncs = new Dictionary<string, List<Func<string, Logging.Message, string>>>();

	public TextFormatter()
	{
		SetupDefaultPropertyFormatters();
	}

	public void SetupDefaultPropertyFormatters()
	{
		// Format data to include dataName
		RegisterPropertyFormatter("Data", (s, lm) => FormatData(lm.DataName, lm.Data, lm));

		// Log level to upper case
		RegisterPropertyFormatter("Level", (s, lm) => s.ToUpper());


		// TicksMsec surrounded by []
		RegisterPropertyFormatter("TicksMsec", (s, lm) => $"[{s}]");

		// // Add Trace info to SourceName
		// RegisterPropertyFormatter("SourceName", (s, lm) => {
		// 		if (lm.Level == Logging.Message.LogLevel.Trace)
		// 		{
		// 			s = $"{s}.{lm.SourceMethodName}:L{lm.SourceLineNumber}";
		// 		}
        //
		// 		return s;
		// 	});

		// Add Custom to SourceName
		RegisterPropertyFormatter("SourceName", (s, lm) => {
				if (!String.IsNullOrEmpty(lm.Custom))
				{
					s = $"{s} [{lm.Custom}]";
				}

				return s;
			});


		// Padding for values
		RegisterPropertyFormatter("Level", (s, lm) => s.PadRight(8, ' '));
		RegisterPropertyFormatter("SourceName", (s, lm) => s.PadRight(25, ' '));
		RegisterPropertyFormatter("Text", (s, lm) => s.PadRight(40, ' '));
	}

	public virtual object Format(Logging.Message loggerMessage)
	{
		var preProcessedMessage = PreProcess(loggerMessage);
		var postProcessedMessage = PostProcess(preProcessedMessage);

		return BuildString(postProcessedMessage);
	}

	public string BuildString(Logging.Message loggerMessage)
	{
		var ticksMsec = GetFormattedProperty(loggerMessage, "TicksMsec");
		var created = GetFormattedProperty(loggerMessage, "Created");
		var sourceName = GetFormattedProperty(loggerMessage, "SourceName");
		var sourceMethodName = GetFormattedProperty(loggerMessage, "SourceMethodName");
		var custom = GetFormattedProperty(loggerMessage, "Custom");
		var level = GetFormattedProperty(loggerMessage, "Level");
		var message = GetFormattedProperty(loggerMessage, "Text");
		var dataName = GetFormattedProperty(loggerMessage, "DataName");
		var data = GetFormattedProperty(loggerMessage, "Data");

		// var data = FormatData(dataName, loggerMessage.Data);

		return String.Join($"{_separator}", new List<string> { 
				$"{ticksMsec} {created}",
				$"{sourceName}",
				$"{level}",
				$"{message}",
				$"{data}",
			});
	}

	public Logging.Message PreProcess(Logging.Message loggerMessage)
	{
		loggerMessage.Formatted.Add("TicksMsec", FormatTicksMsec(loggerMessage.TicksMsec));
		loggerMessage.Formatted.Add("Created", FormatCreatedTimestamp(loggerMessage.Created));
		return loggerMessage;
	}

	public Logging.Message PostProcess(Logging.Message loggerMessage)
	{
		return loggerMessage;
	}

	public string GetFormattedProperty(Logging.Message loggerMessage, string propertyName)
	{
        if (!loggerMessage.Formatted.TryGetValue(propertyName, out var obj) || obj is not object propertyValue)
        {
        	propertyValue = loggerMessage.GetType().GetProperty(propertyName).GetValue(loggerMessage, null);

        	if (propertyValue == null)
        	{
        		propertyValue = new Object();
        	}
        }

		// Call custom format function for property if found
        if (_propertyFormatterFuncs.TryGetValue(propertyName, out var formatFuncs))
        {
			foreach (Func<string, Logging.Message, string> formatFunc in formatFuncs)
			{
				propertyValue = formatFunc(propertyValue.ToString(), loggerMessage);
			}
        }

        return propertyValue.ToString();
	}

	public void RegisterPropertyFormatter(string propertyName, Func<string, Logging.Message, string> func)
	{
        if (!_propertyFormatterFuncs.TryGetValue(propertyName, out var obj) || obj is not List<Func<string, Logging.Message, string>> formatFuncs)
        {
        	_propertyFormatterFuncs.Add(propertyName, new List<Func<string, Logging.Message, string>>());
        }

		_propertyFormatterFuncs[propertyName].Add(func);
	}

	public string FormatTicksMsec(ulong ticksMsec)
	{
		string tickHours = (ticksMsec / 3600000).ToString().PadLeft(2, '0');
		string tickMins = (ticksMsec / 60000).ToString().PadLeft(3, '0');
		string tickSecs = (ticksMsec / 1000).ToString().PadLeft(4, '0');
		string tickMsecs = (ticksMsec).ToString().PadLeft(8, '0');

		return $"{tickHours}:{tickMins}:{tickSecs}:{tickMsecs}";
	}

	public string FormatCreatedTimestamp(DateTime createdTime)
	{
		return createdTime.ToString(_createdTimeFormat);
	}

	public string FormatData(string dataName, object data, Logging.Message loggerMessage)
	{
		string dataString = "";

		if (!String.IsNullOrEmpty(dataName))
		{
			var jsonString = JsonConvert.SerializeObject(
        	data, Formatting.Indented,
        	new JsonConverter[] {new StringEnumConverter()});

			dataString = FormatDataStrings(dataName, jsonString);
		}

		if (loggerMessage.Level == Logging.Message.LogLevel.Trace)
		{
			dataString += "\n" + FormatDataTraceString(dataString, loggerMessage);
		}

		return dataString;
	}

	public virtual string FormatDataStrings(string dataName, string dataJson)
	{
        return $"{dataName}={dataJson}".Replace("\n", "");
	}

	public virtual string FormatDataTraceString(string dataString, Logging.Message loggerMessage)
	{
		return $"TRACE: {loggerMessage.SourceFilename}:{loggerMessage.SourceLineNumber}[{loggerMessage.SourceMethodName}]";
	}
}
