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

using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class CommandLineInterface
{
	protected string[] _args { get; set; }
	protected Dictionary <string, List<string>> _argsParsed { get; set; }
	protected Dictionary <string, string> _argAliases = new();

	protected Dictionary<string, (Func<Task<int>> Command, string Description, bool includeInHelp)> _commands = new();
	protected Dictionary<string, List<(string Arg, string Example, string Description, bool Required)>> _commandArgs = new();

	protected string _defaultCommand = "help";

	public CommandLineInterface(string[] args = null)
	{
		if (args == null)
		{
			args = new string[] {};
		}

		SetArgs(args);

    	_commands.Add("help", (CommandHelp, "Show help text with command usage", true));
		_commandArgs.Add("help", new());

		SetDefaultCommand(_commands.Keys.FirstOrDefault());

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
		return await _commands[GetParsedCommand()].Command();
	}

	public string GetParsedCommand()
	{
		if (_args.Count() >= 1)
		{
			// get the running command and remove from args
			string cmd = _args[0];
			_args = _args.Skip(1).ToArray();
			
			// invoke the matching command
			if (IsCommand(cmd))
			{
				return cmd;
			}
		}

		// return the first key of the defined commands, usually help
		return _defaultCommand;
	}

	public void SetDefaultCommand(string defaultCommand)
	{
		if (!IsCommand(defaultCommand))
		{
			throw new ArgumentException($"No command definition for '{defaultCommand}'");
		}
		_defaultCommand = defaultCommand;
	}

	public void AddCommandDefinition(string command, Func<Task<int>> commandFunc, string description = "Help text for this command", bool includeInHelp = true)
	{
		_commands[command] = (commandFunc, description, includeInHelp);
	}

	public void AddCommandArg(string command, string argName, string example = "Example for this argument", string description = "Description of this argument", bool required = false)
	{
		if (!_commandArgs.TryGetValue(command, out var args))
		{
			args = new();
			_commandArgs.Add(command, args);
		}

		args.Add((argName, example, description, required));	
	}

	public bool IsCommand(string command)
	{
		return (_commands.ContainsKey(command));
	}

	public Func<Task<int>> GetCommandFunc(string command)
	{
		return _commands[command].Command;
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

	public List<string> GetPositionalValues()
	{
		return GetArgumentValues(GetParsedCommand());
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

