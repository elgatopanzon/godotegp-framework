/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CommandLineInterface
 * @created     : Thursday Jan 04, 2024 00:37:45 CST
 */

namespace GodotEGP.CLI;

using Godot;
using GodotEGP;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Text.RegularExpressions;
using System.Linq;

public partial class CommandLineInterface
{
	protected string[] _args { get; set; }
	protected Dictionary <string, List<string>> _argsParsed { get; set; }
	protected Dictionary <string, string> _argAliases = new();

	protected Dictionary<string, (Func<Task<int>> Command, string Description, bool includeInHelp)> _commands = new();
	protected Dictionary<string, List<(string Arg, string Example, string Description, bool Required)>> _commandArgs = new();

	public CommandLineInterface(string[] args = null)
	{
		if (args == null)
		{
			args = new string[] {};
		}

		SetArgs(args);

    	_commands.Add("help", (CommandHelp, "Show help text with command usage", true));
		_commandArgs.Add("help", new());

    	LoggerManager.LogDebug("CLI arguments list", "", "args", _args);
    	LoggerManager.LogDebug("CLI arguments parsed", "", "argsParsed", _argsParsed);

		SetLogLevel();
	}

	public void SetArgs(string[] args)
	{
		_args = args;
		_argsParsed = ParseArgs();
	}

	public void SetLogLevel()
	{
		if (_argsParsed.ContainsKey("--log-level"))
		{
			string logLevelString = _argsParsed["--log-level"][0];

			Message.LogLevel logLevel = (Message.LogLevel) Enum.Parse(typeof(Message.LogLevel), logLevelString);

			LoggerManager.SetLogLevel(logLevel);

			LoggerManager.LogInfo("Log level set", "", "logLevel", logLevel.ToString());
		}
	}

	public async Task<int> Run()
	{
		if (_args.Count() >= 1)
		{
			// get the running command and remove from args
			string cmd = _args[0];
			_args = _args.Skip(1).ToArray();
			
			// invoke the matching command
			if (_commands.ContainsKey(cmd))
			{
				return await _commands[cmd].Command();
			}
		}

		return await CommandHelp();
	}

	public Dictionary<string, List<string>> ParseArgs()
	{
		Dictionary<string, List<string>> parsed = new();

		// loop over args matching args with - or --
		string currentCommand = "";
		List<string> currentValues = new();

		foreach (string argPart in _args)
		{
			// looking for - or --
			if (IsCommandSwitch(argPart))
			{
				LoggerManager.LogDebug("Found command arg", "", "cmd", argPart);

				currentCommand = argPart;
				currentValues = new();

				// set command from alias
				if (_argAliases.ContainsKey(currentCommand))
				{
					currentCommand = _argAliases[argPart];
				}
			}
			else
			{
				// if the command is empty, then consider this the main command
				if (currentCommand == "")
				{
					currentCommand = argPart;
				}
				else
				{
					LoggerManager.LogDebug("Adding command value", "", "value", argPart);

					currentValues.Add(argPart);
				}
			}

			// add and reset current command state when we encounter a new
			// command
			if (currentCommand != "")
			{
				if (parsed.ContainsKey(currentCommand))
				{
				}
				else
				{
					parsed.Add(currentCommand, currentValues);
				}
			}

		}

		LoggerManager.LogDebug("Parsed arguments", "", "argsParsed", parsed);

		return parsed;
	}

	public bool IsCommandSwitch(string cmd)
	{
		return Regex.IsMatch(cmd, "^-[-a-zA-Z0-9]+");
	}

	public bool ArgExists(string arg)
	{
		return (_argsParsed.ContainsKey(arg));
	}

	public List<string> GetArgumentValues(string arg)
	{
		return _argsParsed.GetValueOrDefault(arg, new List<string>());
	}

	public string GetArgumentValue(string arg, string defaultVal = "")
	{
		return GetArgumentValues(arg).SingleOrDefault(defaultVal);
	}

	public bool GetArgumentSwitchValue(string arg)
	{
		return _argsParsed.ContainsKey(arg);
	}

	/**************
	*  commands  *
	**************/
	

	public async Task<int> CommandHelp()
	{
		Console.WriteLine($"usage: {System.Reflection.Assembly.GetEntryAssembly().GetName().Name} [command] [options]");
		Console.WriteLine("");

		Console.WriteLine("commands:");
		foreach (var cmd in _commands)
		{
			if (!cmd.Value.includeInHelp)
			{
				continue;
			}
			 
			Console.WriteLine("");
			Console.WriteLine($"{cmd.Key}: {cmd.Value.Description}");
			foreach (var arg in _commandArgs[cmd.Key])
			{
				Console.WriteLine($"\n  {((arg.Arg)+" "+(arg.Example))}\n  {arg.Description}");
			}
		}

		return 0;
	}

}

