/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ScriptService
 * @created     : Thursday Nov 16, 2023 14:19:07 CST
 */

namespace GodotEGP.Service;

using System;
using System.Linq;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Scripting;
using GodotEGP.Resource;
using GodotEGP.Scripting.Functions;

public partial class ScriptService : Service
{
	private Dictionary<string, Resource<GameScript>> _gameScripts = new();

	private string _scriptFunctionsNamespace = "GodotEGP.Scripting.Functions";

	private Dictionary<string, IScriptFunction> _scriptFunctions = new Dictionary<string, IScriptFunction>();
	public Dictionary<string, IScriptFunction> ScriptFunctions
	{
		get { return _scriptFunctions; }
		set { _scriptFunctions = value; }
	}

	private Dictionary<string, ScriptInterpretter> _sessions = new();

	public ScriptService()
	{
	}

	public void SetConfig(Dictionary<string, Resource<GameScript>> gameScripts)
	{
		LoggerManager.LogDebug("Setting config");

		_gameScripts = gameScripts;

		if (!GetReady())
		{
			_SetServiceReady(true);
		}
	}

	/*******************
	*  Godot methods  *
	*******************/
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	/*********************
	*  Service methods  *
	*********************/
	
	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
		// create instance of function objects and register them
		var scriptFunctionClasses = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .Where(t => t.IsClass && t.Namespace == _scriptFunctionsNamespace);

		foreach (Type functionType in scriptFunctionClasses)
		{
			LoggerManager.LogDebug("Registering function", "", "func", $"{functionType.Name.ToLower()} as {functionType}");
			_scriptFunctions.Add(functionType.Name.ToLower(), (IScriptFunction) Activator.CreateInstance(functionType));
		}
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
		// LoggerManager.LogDebug($"Service deregistered!", "", "service", this.GetType().Name);
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
	}

	/********************************************
	*  Script interpretter management methods  *
	********************************************/
	
	// run a script in a managed instance, destroyed once finished
	public void RunScript(string scriptName)
	{
		if (IsValidScriptName(scriptName))
		{
			LoggerManager.LogDebug("Running script", "", "name", scriptName);

			// create interpretter instance
			var si = CreateInterpretterInstance();
			
			AddChild(si);

	 		si.RunScript(scriptName);
		}
		else {
			throw new InvalidScriptNameException($"Invalid script name: {scriptName}");
		}
	}

	public void RunScriptContent(string scriptContent)
	{
		LoggerManager.LogDebug("Running script content as script");

		var scriptResource = new Resource<GameScript>();
		scriptResource.Value = new GameScript();
		scriptResource.Value.ScriptContent = scriptContent;

		string tempScriptName = scriptContent.GetHashCode().ToString();

		_gameScripts[tempScriptName] = scriptResource;

		RunScript(tempScriptName);
	}
	
	public ScriptInterpretter CreateInterpretterInstance()
	{
		var si = new ScriptInterpretter(_gameScripts, _scriptFunctions);

		si.SubscribeOwner<ScriptInterpretterRunning>(_On_ScriptInterpretter_Running, isHighPriority: true);
		si.SubscribeOwner<ScriptInterpretterWaiting>(_On_ScriptInterpretter_Waiting, isHighPriority: true);
		si.SubscribeOwner<ScriptInterpretterFinished>(_On_ScriptInterpretter_Finished, isHighPriority: true);
		si.SubscribeOwner<ScriptInterpretterOutput>(_On_ScriptInterpretter_Output, isHighPriority: true);

		return si;
	}

	public ScriptInterpretter CreateSession(string sessionName = "default")
	{
		if (!SessionExists(sessionName))
		{
			LoggerManager.LogDebug("Creating session", "", "sessionName", sessionName);

			var sessionInterpretter = CreateInterpretterInstance();

			_sessions[sessionName] = sessionInterpretter;

			AddChild(sessionInterpretter);

			return sessionInterpretter;
		}
		else
		{
			throw new InterpretterSessionExistsException($"Session already exists with the name '{sessionName}'!");
		}

	}

	public void DestroySession(string sessionName = "default")
	{
		if (SessionExists(sessionName))
		{
			LoggerManager.LogDebug("Removing session", "", "sessionName", sessionName);

			_sessions[sessionName].QueueFree();
			_sessions.Remove(sessionName);
		}
	}

	public ScriptInterpretter GetSession(string sessionName = "default")
	{
		if (_sessions.TryGetValue(sessionName, out var ses))
		{
			return ses;
		}

		throw new InvalidSessionNameException($"Invalid session name: {sessionName}");
	}

	public bool SessionExists(string sessionName)
	{
		return _sessions.ContainsKey(sessionName);
	}

	public bool IsValidScriptName(string scriptName)
	{
		return _gameScripts.ContainsKey(scriptName);
	}

	/*********************************
	*  Function management methods  *
	*********************************/
	
	public void RegisterFunctionCallback(Func<ScriptInterpretter, object[], ScriptProcessResult> callbackFunction, string functionName)
	{
		var cbf = new CallbackAsFunction();
		cbf.SetCallbackFunction(callbackFunction);

		_scriptFunctions[functionName] = (IScriptFunction) cbf;
	}

	/***************
	*  Callbacks  *
	***************/
	
	public void _On_ScriptInterpretter_Running(IEvent e)
	{
		if (e is ScriptInterpretterRunning er)
		{
			LoggerManager.LogDebug("Script event running");

			this.Emit<ScriptRunning>(ee => ee.SetResult(er.Result).SetInterpretter(er.Owner as ScriptInterpretter));
		}	
	}

	public void _On_ScriptInterpretter_Waiting(IEvent e)
	{
		if (e is ScriptInterpretterWaiting er)
		{
			LoggerManager.LogDebug("Script event waiting");

			this.Emit<ScriptWaiting>(ee => ee.SetResult(er.Result).SetInterpretter(er.Owner as ScriptInterpretter));
		}	
	}

	public void _On_ScriptInterpretter_Finished(IEvent e)
	{
		if (e is ScriptInterpretterFinished er)
		{
			LoggerManager.LogDebug("Script event finished");

			// remove the instance if it's not a managed session
			if (!_sessions.ContainsValue(er.Owner as ScriptInterpretter))
			{
				(e.Owner as ScriptInterpretter).QueueFree();
			}

			this.Emit<ScriptFinished>(ee => ee.SetResult(er.Result).SetInterpretter(er.Owner as ScriptInterpretter));
		}	
	}

	public void _On_ScriptInterpretter_Output(IEvent e)
	{
		if (e is ScriptInterpretterOutput er)
		{
			LoggerManager.LogDebug("Script event output", "", "e", er.Result.Output);

			this.Emit<ScriptOutput>(ee => ee.SetResult(er.Result).SetInterpretter(er.Owner as ScriptInterpretter));
		}	
	}

	/****************
	*  Exceptions  *
	****************/

	public class InvalidScriptNameException : Exception
	{
		public InvalidScriptNameException() { }
		public InvalidScriptNameException(string message) : base(message) { }
		public InvalidScriptNameException(string message, Exception inner) : base(message, inner) { }
		protected InvalidScriptNameException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
	}

	public class InterpretterSessionExistsException : Exception
	{
		public InterpretterSessionExistsException() { }
		public InterpretterSessionExistsException(string message) : base(message) { }
		public InterpretterSessionExistsException(string message, Exception inner) : base(message, inner) { }
		protected InterpretterSessionExistsException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
	}

	public class InvalidSessionNameException : Exception
	{
		public InvalidSessionNameException() { }
		public InvalidSessionNameException(string message) : base(message) { }
		public InvalidSessionNameException(string message, Exception inner) : base(message, inner) { }
		protected InvalidSessionNameException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
	}
}

