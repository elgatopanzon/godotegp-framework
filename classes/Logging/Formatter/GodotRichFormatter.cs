namespace GodotEGP.Logging.Formatter;

using Godot;
using System;

public partial class GodotRichFormatter : TextFormatter
{
	public GodotRichFormatter()
	{
		_separator = "[bgcolor=black][color=white] | [/color][/bgcolor]";

		RegisterPropertyFormatter("TicksMsec", (s, lm) => {
			return $"[bgcolor=black][color=white]{s}[/color][/bgcolor]";
		});
		RegisterPropertyFormatter("Created", (s, lm) => {
			return $"[bgcolor=black][color=white]{s}[/color][/bgcolor]";
		});
		RegisterPropertyFormatter("Level", (s, lm) => {
			string color = "white";
			switch (lm.Level)
			{
				case Logging.Message.LogLevel.Trace:
					color = "purple";
					s = $"[b]{s}[/b]";
				break;
				case Logging.Message.LogLevel.Debug:
					color = "green";
					s = $"[b]{s}[/b]";
				break;
				case Logging.Message.LogLevel.Info:
					color = "cyan";
					s = $"[b]{s}[/b]";
				break;
				case Logging.Message.LogLevel.Warning:
					color = "orange";
					s = $"[b]{s}[/b]";
				break;
				case Logging.Message.LogLevel.Error:
					color = "red";
					s = $"[b]{s}[/b]";
				break;
				case Logging.Message.LogLevel.Critical:
					color = "white";
					s = $"[b][bgcolor=red]{s}[/bgcolor][/b]";
				break;
			}

			return $"[bgcolor=black][color={color}]{s}[/color][/bgcolor]";
		});

		RegisterPropertyFormatter("Text", (s, lm) => {
			string color = "white";
			switch (lm.Level)
			{
				case Logging.Message.LogLevel.Error:
					color = "red";
				break;
				case Logging.Message.LogLevel.Critical:
					color = "red";
				break;
			}

			return $"[bgcolor=black][color={color}]{s}[/color][/bgcolor]";
		});
		RegisterPropertyFormatter("SourceName", (s, lm) => {
			return $"[bgcolor=black][color=orange]{s}[/color][/bgcolor]";
		});
		RegisterPropertyFormatter("Custom", (s, lm) => {
			return $"[bgcolor=black][color=cyan]{s}[/color][/bgcolor]";
		});

		// Line break at Level if console isn't wide
		RegisterPropertyFormatter("Level", (s, lm) => {
			if (Console.WindowWidth <= 140 && Console.WindowWidth != 0)
			{
				return "\n" + s;
			}
			else
			{
				return s;
			}
			});
	}
	public override object Format(Logging.Message loggerMessage)
	{
		return base.Format(loggerMessage);
	}

	public override string FormatDataStrings(string dataName, string dataJson)
	{
        return $"[bgcolor=black][color=cyan]{dataName}[/color][/bgcolor][bgcolor=black][color=white]=[/color][/bgcolor][bgcolor=black][color=pink]{dataJson}[/color][/bgcolor]".Replace("\n", "");
	}

	public override string FormatDataTraceString(string dataString, Logging.Message loggerMessage)
	{
		return $"[bgcolor=black][color=white]TRACE: {loggerMessage.SourceFilename}:[color=white]{loggerMessage.SourceLineNumber}[/color][color=cyan] [{loggerMessage.SourceName}.{loggerMessage.SourceMethodName}()][/color][/color][/bgcolor]";
	}
}

