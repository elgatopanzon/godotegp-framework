/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ScriptInterpretter
 * @created     : Wednesday Nov 15, 2023 17:32:08 CST
 */

namespace GodotEGP.Scripting;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.State;
using GodotEGP.Resource;
using GodotEGP.Scripting.Functions;

public partial class ScriptInterpretter : Node
{
	// state classes for process state machine
	public class Process : HStateMachine {}
	public class Preparing : HStateMachine {}
	public class Running : HStateMachine {}
	public class Waiting : HStateMachine {}
	public class Finished : HStateMachine {}

	private Process _processState = new Process();
	private Preparing _statePreparing = new Preparing();
	private Running _stateRunning = new Running();
	private Waiting _stateWaiting = new Waiting();
	private Finished _stateFinished = new Finished();

	private const int STATE_RUNNING = 0;
	private const int STATE_WAITING = 1;
	private const int STATE_FINISHED = 2;

	// gamescript related properties
	private Dictionary<string, Resource<GameScript>> _gameScripts;
	private GameScript _gameScript;
	private string[] _currentScriptLinesSplit;
	private int _scriptLineCounter = 0;

	public int ScriptLinerCounter
	{
		get { return _scriptLineCounter; }
		set { _scriptLineCounter = value; }
	}

	private string[] _scriptPipeQueue = new string[] {};

	private List<ScriptProcessResult> _scriptLineResults = new List<ScriptProcessResult>();
	private ScriptProcessResult _scriptLineResult;

	public ScriptProcessResult CurrentScriptResult
	{
		get { return _scriptLineResult; }
		set { _scriptLineResult = value; }
	}

	private string[] _scriptParams;
	private string _gameScriptName;
	private List<string> _gameScriptFunctionNames = new();

	// script function properties
	private Dictionary<string, IScriptFunction> _scriptFunctions = new Dictionary<string, IScriptFunction>();

	// holds session variables used by the script
	Dictionary<string, object> _scriptVars = new Dictionary<string, object>() { { "?", "-1" } };

	public Dictionary<string, object> ScriptVars
	{
		get { return _scriptVars; }
		set { _scriptVars = value; }
	}

	// child script properties
	private ScriptInterpretter _childScript;
	private int _childScriptHash = 0;
	private bool _childScriptKeepEnv = false;

	public bool ChildKeepEnv
	{
		get { return _childScriptKeepEnv; }
		set { _childScriptKeepEnv = value; }
	}

	private ScriptInterpretter _parentScript;
	public ScriptInterpretter ParentScript
	{
		get { return _parentScript; }
		set { _parentScript = value; }
	}

	private bool _processFinished;
	public bool ProcessFinished
	{
		get { return _processFinished; }
		set { _processFinished = value; }
	}

	// used by parent to obtain results
	public string Stdout
	{
		get { 
			return string.Join("\n", _scriptLineResults.Where(x => x.Result.Length > 0 && x.ResultProcessMode != ResultProcessMode.DISCARD).Select(x => x.Stdout));
		}
	}
	public string Stderr
	{
		get { 
			return string.Join("\n", _scriptLineResults.Where(x => x.Result.Length > 0 && x.ResultProcessMode != ResultProcessMode.DISCARD).Select(x => x.Stderr));
		}
	}
	public int ReturnCode
	{
		get { 
			return _scriptLineResult.ReturnCode;
		}
	}

	private double _deltaCounter = 0;
	private double _deltaTimeStep = 0.03333;
	// 0.5 = 2 per sec
	// 0.05 = 20 per sec
	// 0.025 = 40 per sec
	// 0.0125 = 80 per sec

	public ScriptInterpretter(Dictionary<string, Resource<GameScript>> gameScripts, Dictionary<string, IScriptFunction> scriptFuncs, string[] scriptParams = null)
	{
		_scriptParams = scriptParams;
		_gameScripts = gameScripts;
		_scriptFunctions = scriptFuncs;

		// setup process sub-states
		_statePreparing.OnEnter = _State_Preparing_OnEnter;
		_stateRunning.OnEnter = _State_Running_OnEnter;
		_stateRunning.OnUpdate = _State_Running_OnUpdate;
		_stateWaiting.OnEnter = _State_Waiting_OnEnter;
		_stateWaiting.OnUpdate = _State_Waiting_OnUpdate;
		_stateFinished.OnEnter = _State_Finished_OnEnter;

		_processState.AddState(_statePreparing);	
		_processState.AddState(_stateRunning);	
		_processState.AddState(_stateWaiting);	
		_processState.AddState(_stateFinished);	

		// create state transitions
		_processState.AddTransition(_statePreparing, _stateRunning, STATE_RUNNING);
		_processState.AddTransition(_stateRunning, _stateWaiting, STATE_WAITING);
		_processState.AddTransition(_stateWaiting, _stateRunning, STATE_RUNNING);
		_processState.AddTransition(_stateRunning, _stateFinished, STATE_FINISHED);
		_processState.AddTransition(_stateFinished, _statePreparing, STATE_RUNNING);
        //
	}

	/*******************
	*  Godot methods  *
	*******************/
	
	public override void _Process(double delta)
	{
		// for (_deltaCounter += delta; _deltaCounter >= _deltaTimeStep; _deltaCounter -= _deltaTimeStep)
		// {
		// }
		_processState.Update();
	}

	/****************************
	*  Script running methods  *
	****************************/
	
	public void RunScript(string scriptName)
	{
		if (_gameScripts.TryGetValue(scriptName, out Resource<GameScript> gs))
		{
			Reset();

			_gameScript = gs.Value;
			_gameScriptName = scriptName;

			LoggerManager.LogDebug($"[{_gameScriptName}] running");

			// Start the state machine
			if (_processFinished)
			{
				_processFinished = true;
				_processState.Transition(STATE_RUNNING);
			}
			else {
				_processState.Enter();
			}
		}
		else
		{
			throw new InvalidScriptResourceException($"The game script '{scriptName}' is not a valid GameScript resource!");
		}
	}

	public void RunScriptContent(string scriptContent)
	{
		RegisterFunctionFromContent("eval", "# script from content\n"+scriptContent+"\n# end script from content");

		// run the created resource
		RunScript("eval");
	}

	public void RegisterFunctionFromContent(string func, string scriptContent)
	{
		var scriptResource = new Resource<GameScript>();
		scriptResource.Value = new GameScript();
		scriptResource.Value.ScriptContent = scriptContent;

		_gameScripts[func] = scriptResource;

		_gameScriptFunctionNames.Add(func);
	}

	public bool IsValidScriptName(string script)
	{
		return _gameScripts.ContainsKey(script);
	}

	public bool IsValidFunction(string func)
	{
		return (
			IsValidScriptName(func) || // scripts as function name
			_scriptFunctions.ContainsKey(func)
			);
	}

	public int CurrentScriptLineCount()
	{
		return _currentScriptLinesSplit.Count();
	}

	public void Reset()
	{
		_scriptLineResult = null;
		_scriptLineCounter = 0;
		_scriptLineResults.Clear();
		_currentScriptLinesSplit = new string[]{};
	}

	public void BroadcastScriptOutput(ScriptProcessResult result, int lineNumber = 0)
	{
		if (lineNumber == 0)
		{
			lineNumber = _scriptLineCounter;
		}
		if (result.ResultProcessMode == ResultProcessMode.NORMAL && result.Result.Length > 0)
		{
			_scriptLineResults.Add(result);

			foreach (string line in result.Result.Split('\n'))
			{
				LoggerManager.LogDebug("Broadcasting script output", GetHashCode().ToString(), lineNumber.ToString(), line);

				this.Emit<ScriptInterpretterOutput>((e) => e.SetResult(new ScriptResultOutput(result, line, lineNumber, _gameScript)));
			}
		}
	}

	/****************************
	*  State changed callbacks  *
	****************************/

	public void _State_Preparing_OnEnter()
	{
		// look for funcs in the script
		string scriptContentWithoutFunctions = _gameScript.ScriptContent;

		var scriptFunctions = ParseScriptLineFunction(scriptContentWithoutFunctions);
		if (scriptFunctions.Count > 0)
		{
			foreach (var func in scriptFunctions)
			{
				LoggerManager.LogDebug("Function found in line", "", "func", func);
				RegisterFunctionFromContent(func.FuncName, func.ScriptContent);

				// remove function content from original string
				scriptContentWithoutFunctions = scriptContentWithoutFunctions.Replace(func.RawContent, new String('\n', func.LineCount - 1));
			}
		}


		_currentScriptLinesSplit = scriptContentWithoutFunctions.Split(new char[] {'\n', '\r'}, StringSplitOptions.None);
		LoggerManager.LogDebug("Script line count", "", "count", _currentScriptLinesSplit.Count());


		_processState.Transition(STATE_RUNNING);
		_processState.Update();
	}

	public void _State_Running_OnEnter()
	{
		this.Emit<ScriptInterpretterRunning>();
	}

	public void _State_Running_OnUpdate()
	{
		ResultProcessMode previousResultProcessMode = ResultProcessMode.NORMAL;
		if (_scriptLineResult != null)
		{
			previousResultProcessMode = _scriptLineResult.ResultProcessMode;
		}

		if (_scriptLineCounter >= _currentScriptLinesSplit.Count() || _scriptLineCounter < 0)
		{
			_processState.Transition(STATE_FINISHED); // end of the script
			return;
		}

		// retrive the current script line
		string linestr = _currentScriptLinesSplit[_scriptLineCounter].Trim();

		// skip lines
		if (linestr.StartsWith("#") || linestr.Length == 0)
		{
			_scriptLineCounter++;

			_processState.Update();

			return;
		}

		// process the line if it's not empty
		// TODO: figure out why/how to remove empty lines, or just let them
		// happen
		if (linestr.Length > 0)
		{
			
			_scriptLineResult = InterpretLine(linestr);

			LoggerManager.LogDebug($"[{_gameScriptName}] Line {_scriptLineCounter +1}", "", "line", $"[{_scriptLineResult.ReturnCode}] {_scriptLineResult.Result}");

			if (previousResultProcessMode == ResultProcessMode.ASYNC)
			{
				LoggerManager.LogDebug($"[{_gameScriptName}] Line {_scriptLineCounter +1} can we fix?", "", "line", $"[{_scriptLineResult.ReturnCode}] {_scriptLineResult.Result}");
			}
			ExecuteVariableAssignment("?", _scriptLineResult.ReturnCode.ToString());

			// increase script line after processing
			if (_scriptPipeQueue.Count() == 0)
			{
				_scriptLineCounter++;
			}


			if (_scriptLineResult.ResultProcessMode == ResultProcessMode.ASYNC)
			{
				// we are waiting for something, so switch processing mode
				_processState.Transition(STATE_WAITING);

				return;
			}
			else
			{
				// add process result to results list
				if (_scriptPipeQueue.Count() == 0)
				{
					BroadcastScriptOutput(_scriptLineResult);
				}

				// trigger another update to process the next line
				_processState.Update();

				return;
			}

		}
	}

	public void _State_Waiting_OnEnter()
	{
		this.Emit<ScriptInterpretterWaiting>();
	}

	public void _State_Waiting_OnUpdate()
	{
		// _processState.Transition(STATE_RUNNING);
		// if we have a child script, then we're waiting for it
		if (_childScript != null)
		{
			// check if it's finished
			if (_childScript.ProcessFinished)
			{
				// copy the childScript's stdout
				string childStdout = _childScript.Stdout;
				if (_childScript.ReturnCode != 0)
				{
					childStdout = _childScript.Stderr;
					_scriptLineResult.ReturnCode = _childScript.ReturnCode;
					_scriptLineResult.Stderr = _childScript.Stderr;
				}
				LoggerManager.LogDebug($"Child script finished", "", "childStdout", childStdout);

				// assign the variable to the childstd result after replacing
				// the result in the script content
				AssignVariableValue("func"+_childScript.GetHashCode().ToString(), childStdout.Replace("$func"+GetHashCode()+"\n", ""));

				// perform variable substitution to replace the line with the
				// child's result
				if (_childScript.ScriptVars.ContainsKey("STDIN"))
				{
					LoggerManager.LogDebug($"[{_gameScriptName}] Line {_scriptLineCounter} (async)", "", "stdin", _childScript.ScriptVars["STDIN"]);
					_scriptVars["STDIN"] = _childScript.ScriptVars["STDIN"];
					_scriptVars["?"] = _childScript.ScriptVars["?"];
				}
				_scriptLineResult = ExecuteVariableSubstitution("func"+_childScript.GetHashCode(), _scriptLineResult);


				LoggerManager.LogDebug($"Child script processed", "", "lineRes", _scriptLineResult);

				LoggerManager.LogDebug($"[{_gameScriptName}] Line {_scriptLineCounter} (async)", "", "asyncLine", $"[{_scriptLineResult.ReturnCode}] {_scriptLineResult.Result}");

				ExecuteVariableAssignment("?", _childScript.ReturnCode.ToString());

				// if there's any unparsed vars, trigger the line for
				// re-processing
				var resultUnparsedVars = ParseVarSubstitutions(_scriptLineResult.Stdout);
				if (resultUnparsedVars.Count > 0)
				{
					LoggerManager.LogDebug("Async line contains unparsed variables", "", "vars", resultUnparsedVars);
					_scriptLineCounter--;
					LoggerManager.LogDebug("Async line to reprocess", "", "line", _currentScriptLinesSplit[_scriptLineCounter]);

					_currentScriptLinesSplit[_scriptLineCounter] = _scriptLineResult.Stdout;
					_scriptLineResult.ResultProcessMode = ResultProcessMode.DISCARD;
				}

				// fix: when a block statement contains one or more nested line,
				// reprocess the line to evaluate the final block statement
				bool asyncBlockStatement = false;
				if (ParseBlockStatementOpening(_scriptLineResult.Result, parseConditions: true) != null)
				{
					LoggerManager.LogDebug("Found block statement opening line in async result");
					_scriptLineCounter--;
					_currentScriptLinesSplit[_scriptLineCounter] = _scriptLineResult.Stdout;
					_scriptLineResult.ResultProcessMode = ResultProcessMode.DISCARD;

					asyncBlockStatement = true;
				}

				// implement a hack to clear the current line so the loop
				// doesn't continue
				if (_childScript.ReturnCode == -100)
				{
					LoggerManager.LogDebug("Break called, overriding loop line", "", "line", _scriptLineCounter);
					LoggerManager.LogDebug("Break called, overriding loop line", "", "line", _currentScriptLinesSplit.Count());
					LoggerManager.LogDebug("Break called, overriding loop line", "", "line", _currentScriptLinesSplit[_scriptLineCounter - 1]);
					_currentScriptLinesSplit[_scriptLineCounter-1] = "while (( 0 != 0 )); do";
				}

				// if the pipe queue is empty broadcast the final result
				// HACK: if we're just printing the returncode (used by if
				// function) then also add the result
				if ((_scriptPipeQueue.Count() == 0 || _scriptPipeQueue.Contains("printreturncode")) && (_currentScriptLinesSplit.ElementAtOrDefault(_scriptLineCounter) != null && _currentScriptLinesSplit[_scriptLineCounter] != "printreturncode"))
				{
					if (_childScript.ScriptVars.ContainsKey("STDIN") && asyncBlockStatement)
					{
						LoggerManager.LogDebug("Broadcast override (printreturncode)", GetHashCode().ToString(), "stdin", _childScript._scriptVars["STDIN"]);

						// LoggerManager.LogDebug("Broadcast override for pipe", GetHashCode().ToString(), "stdout", _scriptLineResult.Stdout);
						ScriptProcessResult overrideRes = new(_childScript.ReturnCode, _childScript._scriptVars["STDIN"].ToString(), _childScript.Stderr);
						BroadcastScriptOutput(overrideRes, _scriptLineCounter + 1);
					}
					BroadcastScriptOutput(_scriptLineResult);
				}
				else
				{
					if (_scriptPipeQueue.Count() == 0)
					{
						BroadcastScriptOutput(_scriptLineResult);
					}
				}
				if (_scriptPipeQueue.Count() == 0)
				{
					_scriptVars["STDIN"] = "";
				}

				_childScriptHash = _childScript.GetHashCode();

				// clear child script instance since we're done with it
				_childScript.QueueFree();
				_childScript = null;
				_childScriptKeepEnv = false;


				// resume execution
				_processState.Transition(STATE_RUNNING);
				_processState.Update();
			}
		}
	}

	public void _State_Finished_OnEnter()
	{
		_processFinished = true;
		LoggerManager.LogDebug($"[{_gameScriptName}] finished");

		this.Emit<ScriptInterpretterFinished>();
	}

	/*********************************
	*  Execute script line methods  *
	*********************************/

	// main script process execution functions
	public ScriptProcessResult ExecuteFunctionCall(string func, params string[] funcParams)
	{
		if (_scriptFunctions.ContainsKey(func))
		{
			ScriptProcessResult res;

			try
			{
				res = _scriptFunctions[func].Call(this, funcParams);
			}
			catch (System.Exception e)
			{
				res = new ScriptProcessResult(127, "", e.Message);
			}

			return res;
		}

		// check if the function name is a valid script
		else if (IsValidScriptName(func))
		{
			LoggerManager.LogDebug("Executing script as function", "", "script", func);

			// create a child script interpreter instance to run the script
			_childScript = new ScriptInterpretter(_gameScripts, _scriptFunctions, scriptParams: funcParams);
			_childScript._parentScript = this;
			LoggerManager.LogDebug("Creating child script", "", "stdin", GetVariableValue("STDIN"));
			AddChild(_childScript);

			// set child vars to match ours
			// if (_childScriptKeepEnv || _gameScriptFunctionNames.Contains(func))
			// {
				_childScript._scriptVars = _scriptVars;
			// }
			if (_scriptVars.ContainsKey("STDIN"))
			{
				_childScript._scriptVars["STDIN"] = _scriptVars["STDIN"];
				_childScript._scriptVars["?"] = _scriptVars["?"];
			}

			LoggerManager.LogDebug("Creating child script", "", "stdin", _childScript.GetVariableValue("STDIN"));

			_childScript.RunScript(func);

			// return a wait mode process
			return new ScriptProcessResult(0, "$func"+_childScript.GetHashCode(), resultProcessMode: ResultProcessMode.ASYNC);
		}

		return new ScriptProcessResult(127, "", $"command not found: {func}");
	}

	public ScriptProcessResult ExecuteVariableAssignment(string varName, string varValue)
	{
		// parse variable name and keys
		string arrayIndexPattern = @"(?<=\[)(.+?)(?=\])";
		Match m = Regex.Match(varName, arrayIndexPattern);

		string dictKey = "";

		// parse the dictionary key name
		if (m.Groups.Count > 0 && m.Groups[0].Value.Length > 0)
		{
			dictKey = m.Groups[1].Value;
			varName = varName.Replace("["+dictKey+"]", String.Empty);
			dictKey = dictKey.Trim('\"', '\"');

			LoggerManager.LogDebug($"Set dictionary value {varName}[{dictKey}]", "", "value", varValue);

			if (_scriptVars.ContainsKey(varName))
			{
				var dict = (Dictionary<string, object>) _scriptVars[varName];

				LoggerManager.LogDebug($"Dictionary exists {varName}", "", "dict", dict);

				dict[dictKey] = varValue;

				return new ScriptProcessResult(0);
			}
			else
			{
				return new ScriptProcessResult(1, "", $"{varName}: assignment to invalid subscript range");
			}

		}
		else
		{
			AssignVariableValue(varName, varValue);
			return new ScriptProcessResult(0);
		}

	}

	public void AssignVariableValue(string varName, string varValue)
	{
		object varValueFinal = null;

		if (varValue.StartsWith('(') && varValue.EndsWith(')'))
		{
			string valueStripped = varValue.Trim('(', ')');
			var functionCallParams = ParseFunctionCalls("f "+valueStripped, verifyFunctionName: false);
			if (functionCallParams.Count > 0)
			{
				if (functionCallParams[0] is ScriptProcessFunctionCall fc)
				{
					varValueFinal = fc.Params.Select((value,index) => new { value, index}).ToDictionary(value => Convert.ToInt32(value.index).ToString(), value => (object) value.value);
				}
			}
		}
		else
			varValueFinal = varValue;

		LoggerManager.LogDebug("Setting variable value", "", varName, varValueFinal);

		_scriptVars[varName] = varValueFinal;
	}

	public ScriptProcessResult ExecuteVariableSubstitution(string varName, ScriptProcessResult res)
	{
		var varValue = GetVariableValue(varName).ToString();
		return new ScriptProcessResult(0, res.Result.Replace("${"+varName+"}", varValue).Replace("$"+varName, varValue));
	}

	public string GetVariableValue(string varName)
	{
		string varValue = "";

		// string arrayIndexPattern = @"\[""?([0-9a-zA-Z_@\-])""?\]";
		string arrayIndexPattern = @"(?<=\[)(.+?)(?=\])";
		Match m = Regex.Match(varName, arrayIndexPattern);

		string dictKey = "";

		LoggerManager.LogDebug("Getting var value", "", "varName", varName);

		// parse the dictionary key name
		if (m.Groups.Count > 0 && m.Groups[1].Value.Length > 0)
		{
			dictKey = m.Groups[1].Value;
			varName = varName.Replace("["+dictKey+"]", String.Empty);
			dictKey = dictKey.Trim('\"', '\"');

			LoggerManager.LogDebug($"Get dictionary value {varName}", "", "key", dictKey);
		}

		// check if we have an assigned var
		if (_scriptVars.TryGetValue(varName.Replace("!", String.Empty).Replace("[", String.Empty).Replace("]", String.Empty), out object obj))
		{
			// if we are looking for a dictionary key, try and get it
			if (dictKey.Length > 0)
			{
				var dict = (Dictionary<string, object>) obj;

				if (dict.ContainsKey(dictKey))
				{
					varValue = (string) dict[dictKey];
				}
				else if (dictKey == "@")
				{
					if (varName.StartsWith("!"))
						varValue = (string) string.Join("\n", dict.Select(x => "\""+x.Key+"\"").ToArray());
					else
						varValue = (string) string.Join(" ", dict.Select(x => "\""+x.Value+"\"").ToArray());
				}
			}
			else
				varValue = (string) obj;
		}

		// check if it's a special var
		else
		{
			// positional arguments
			if (int.TryParse(varName, out int i))
			{
				if (i == 0)
				{
					varValue = _gameScriptName;
				}
				else {
					if (_scriptParams.Length >= i)
					{
						varValue = _scriptParams[i-1];
					}
				}
			}
			// arguments as string
			else if (varName == "*")
			{
				varValue = _scriptParams.Join(" ");
			}
			// arguments count
			else if (varName == "#")
			{
				varValue = _scriptParams.Count().ToString();
			}
			// last return code
			// else if (varName == "?" && _scriptLineResult != null)
			// {
			// 	varValue = _scriptLineResult.ReturnCode.ToString();
			// }
		}

		LoggerManager.LogDebug("Get var value", "", varName, varValue);

		return varValue;
	}

	/************************************
	*  Line interpertretation methods  *
	************************************/

	// accepts a pure string containing the script content to process for
	// interpretation
	public List<ScriptProcessResult> InterpretLines(string scriptLines)
	{
		List<ScriptProcessResult> processes = new List<ScriptProcessResult>();

		_currentScriptLinesSplit = scriptLines.Split(new string[] {"\\n"}, StringSplitOptions.None);

		while (_scriptLineCounter < _currentScriptLinesSplit.Count())
		{
			string linestr = _currentScriptLinesSplit[_scriptLineCounter].Trim();

			if (linestr.Length > 0)
			{
				processes.Add(InterpretLine(linestr));
			}

			_scriptLineCounter++;
		}

		return processes;
	}

	// accepts a single script line and generates a list of process objects to
	// achieve the final rendered result for each line
	public ScriptProcessResult InterpretLine(string line, bool verifyFunctionName = false)
	{
		// TODO: split and process lines with ; and pipes
		// var lineSplitPipe = line.Split(new string[] {"|"}, StringSplitOptions.None).Select(x => x.Trim()).ToArray();

		List<string> lineSplitPipe = new();
		string patternPipeSplit = @"(?=[^|])(?:[^|]*\([^)]+\))*[^|]*";
		MatchCollection matches = Regex.Matches(line, patternPipeSplit);

		foreach (Match m in matches)
		{
			lineSplitPipe.Add(m.Value.Trim());
		}

		if (lineSplitPipe.Count() > 1)
		{
			LoggerManager.LogDebug("Pipe found", "", "lines", lineSplitPipe);

			_scriptPipeQueue = lineSplitPipe.ToArray();
		}
		if (_scriptPipeQueue.Count() > 0)
		{
			LoggerManager.LogDebug("Pipe queue count", "", "pipeQueue", _scriptPipeQueue);
			// move previous command output to fake stdin and erase it
			if (lineSplitPipe.Count() == 1)
			{
				ExecuteVariableAssignment("STDIN", _scriptLineResult.Stdout);
				_scriptLineResult.Stdout = "";
			}

			// set current line content to queued pipe command
			line = _scriptPipeQueue[0];
			_scriptPipeQueue = _scriptPipeQueue.Skip(1).ToArray();
			_currentScriptLinesSplit[_scriptLineCounter] = line;

			LoggerManager.LogDebug("Overriding line with queued pipe command", "", "pipeLine", line);
		}

		// execution and parse order
		// 1. parse printed vars to real values in unparsed line
		// parse var names in expressions (( )) and replace with actual
		// values e.g. number or string
		// 2. parse nested lines as a normal line, replacing the executed result
		// 3. parse variable assignments
		// 4. parse function calls
		// 5. parse if/while/for calls

		ScriptProcessResult lineResult = new ScriptProcessResult(0, line);

		// list of process operations to do to this script line
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		// first thing, parse and replace variable names with values
		while (ParseVarSubstitutions(lineResult.Result).Count > 0)
		{
			foreach (ScriptProcessVarSubstitution lineProcess in ParseVarSubstitutions(lineResult.Result))
			{
				lineResult = ExecuteVariableSubstitution(lineProcess.Name, lineResult);
				LoggerManager.LogDebug("Parsed var sub result", "", "lineResult", lineResult.Stdout);
				// _currentScriptLinesSplit[_scriptLineCounter] = lineResult.Stdout;
				// _scriptLineCounter--;
				// lineResult.ResultProcessMode = ResultProcessMode.DISCARD;
				// return lineResult;
				break;
			}
		}
		// processes.AddRange(ParseVarSubstitutions(line));


		// third thing, parse nested script lines and replace values
		lineResult = ParseNestedLines(lineResult.Result);
		if (lineResult.ReturnCode != 0 || lineResult.ResultProcessMode == ResultProcessMode.ASYNC)
		{
			LoggerManager.LogDebug("Nested line process failed or async");
			return lineResult;
		}
		// foreach (ScriptProcessNestedProcess lineProcess in ParseNestdLines(lineResult.Stdout))
		// {
		// 	// TODO: process nested lines
		// 	// lineResult = ExecuteVariableSubstitution(lineProcess.Name, lineResult);
		// }
		// processes.AddRange(ParseNestdLines(line));

		// second thing, parse and replace expressions with values
		foreach (ScriptProcessExpression lineProcess in ParseExpressions(lineResult.Result))
		{
			// TODO: implement expression processing
			// lineResult = ExecuteExpression(lineProcess.Expression, lineResult);
			LoggerManager.LogDebug("Expression found", "", "exp", lineProcess);
			try
			{
				var expressionRes = ExecuteScriptExpression(lineProcess.Expression.Trim());
				LoggerManager.LogDebug("Expression render", "", "expRes", expressionRes);
				lineResult.Stdout = lineResult.Stdout.Replace("(("+lineProcess.Expression.Trim()+"))", expressionRes.ToString());
			}
			catch (System.Exception e)
			{
				lineResult.ReturnCode = 127;
				lineResult.Stderr = e.Message;

				break;
			}
		}
		// processes.AddRange(ParseExpressions(line));

		// parse variable assignments
		var varAssignmentProcesses = ParseVarAssignments(lineResult.Result);
		// processes.AddRange(varAssignmentProcesses);
		foreach (ScriptProcessVarAssignment lineProcess in varAssignmentProcesses)
		{
			lineResult = ExecuteVariableAssignment(lineProcess.Name, lineProcess.Value);
		}
		if (lineResult.ReturnCode != 0)
		{
			return lineResult;
		}

		// process if statement
		var ifStatementParsed = ParseBlockStatementOpening(lineResult.Result);
		if (ifStatementParsed != null)
		{
			LoggerManager.LogDebug("Found block statement", "", "if", ifStatementParsed.Stdout);

			if (ifStatementParsed.ReturnCode == 0)
			{
				LoggerManager.LogDebug("Enter block statement!", "", "line", lineResult.Stdout);
			}
			else
			{
				LoggerManager.LogDebug("Skip block statement", "", "line", lineResult.Stdout);
			}

			var parsedBlockLines = ParseBlockStatementLines(ifStatementParsed.ReturnCode, lineResult.Stdout);

			LoggerManager.LogDebug("Parsed block lines", "", "lines", parsedBlockLines);
			if (parsedBlockLines.Count > 0)
			{
				string tempFuncName = $"{_scriptLineCounter}_{GetHashCode()}";
				RegisterFunctionFromContent(tempFuncName, parsedBlockLines.ToArray().Join("\n"));
				
				// set line content to the temp function
				lineResult.Stdout = tempFuncName;
			}
			else
			{
				// trigger reprocessing
				lineResult.Stdout = "";
				return lineResult;
			}

			lineResult.ResultProcessMode = ResultProcessMode.DISCARD;
		}

		// var blockProcess = ParseBlockProcessLine(line, _currentScriptLinesSplit);
		// if (blockProcess != null)
		// {
		// 	// processes.AddRange(new List<ScriptProcessOperation>() {blockProcess});
		// 	LoggerManager.LogDebug("Evaluating block procress line", "", "line", blockProcess);
		// }
		// else
		// {
			// if var assignments are 0, then try to match function calls
			// NOTE: this is because the regex matches both var assignments in lower
			// case AND function calls
			if (varAssignmentProcesses.Count == 0)
			{
				foreach (ScriptProcessFunctionCall lineProcess in ParseFunctionCalls(lineResult.Result, verifyFunctionName: verifyFunctionName))
				{
					lineResult = ExecuteFunctionCall(lineProcess.Function, lineProcess.Params.ToArray());
				}
				// processes.AddRange(ParseFunctionCalls(line));
			}
		// }

		// if there's no processes until now, just return the plain object with
		// no processing attached
		// if (processes.Count == 0)
		// {
		// 	processes.Add(new ScriptProcessOperation(line));
		// }
		
		// LoggerManager.LogDebug("Line result", "", "res", lineResult);

		return lineResult;
	}


	/**************************************
	*  Parse if/while/for block methods  *
	**************************************/

	public List<string> ParseBlockStatementLines(int currentLineReturnCode = 0, string overrideScriptLine = "")
	{
		List<string> parsedLines = new();
		int currentStartLine = _scriptLineCounter;

		// look ahead to extract the lines inside the if block, and the lines
		// inside any else block, until we reach a fi/done
		
		int blockProcessState = -1;
		bool isLoopMode = false;
		bool isForLoopMode = false;

		if (_currentScriptLinesSplit[currentStartLine].StartsWith("for "))
		{
			isForLoopMode = true;
			isLoopMode = true;

			string scriptLine = _currentScriptLinesSplit[currentStartLine];
			if (overrideScriptLine.Length > 0)
			{
				scriptLine = overrideScriptLine;
			}

			var parsedFuncs = ParseFunctionCalls(scriptLine.Replace("; do", String.Empty), false);
			LoggerManager.LogDebug("Found for loop conditions", "", "for", scriptLine);

			if (parsedFuncs.Count > 0 && parsedFuncs[0] is ScriptProcessFunctionCall fc)
			{
				string variableName = fc.Params[0];
				List<string> loopItems = fc.Params.Skip(2).Where(x => x.Length > 0).ToList();

				// TODO: expand bash sequence { .. } into list of values
				// and replace the loopItems with it

				LoggerManager.LogDebug("Parsed for loop content", "", variableName, loopItems);

				string forParamsVarName = $"{currentStartLine}_for_{GetHashCode()}";
				string forParamsVarNameIdx = $"{forParamsVarName}idx";
				string forParamsVarNameFunc = $"{forParamsVarName}func";
				_scriptVars[forParamsVarName] = loopItems.Select((value,index) => new { value, index}).ToDictionary(value => Convert.ToInt32(value.index).ToString(), value => (object) value.value);

				LoggerManager.LogDebug("For loop variable", "", forParamsVarName, _scriptVars[forParamsVarName]);

				// rewrite line into a while loop
				_currentScriptLinesSplit[currentStartLine] = $"while (( ${forParamsVarNameIdx} < {loopItems.Count} ))";
				RegisterFunctionFromContent(forParamsVarNameFunc, $"{variableName}=\"${{{forParamsVarName}[${{{forParamsVarNameIdx}}}]}}\"\n{forParamsVarNameIdx}=((${forParamsVarNameIdx} + 1))");

				int injectPipeLine = currentStartLine + 2;
				if (scriptLine.Contains("; do"))
				{
					_currentScriptLinesSplit[currentStartLine] += "; do";
					injectPipeLine = currentStartLine + 1;
				}
				_scriptVars[forParamsVarNameIdx] = "0";
				ExecuteVariableAssignment(variableName, loopItems[0]);

				_currentScriptLinesSplit[injectPipeLine] = $"{forParamsVarNameFunc} | " + _currentScriptLinesSplit[injectPipeLine];

				_childScriptKeepEnv = true;
			}
		}

		// skip past the then/do line if it's on the same line
		if (_currentScriptLinesSplit[currentStartLine].Contains("; then") ||
				_currentScriptLinesSplit[currentStartLine].Contains("; do"))
		{
			blockProcessState++;
			LoggerManager.LogDebug("Single opener block statement found");

			if (_currentScriptLinesSplit[currentStartLine].Contains("; do"))
			{
				isLoopMode = true;
			}
		}

		int tempLineCounter = _scriptLineCounter + 1;
		while (tempLineCounter < _currentScriptLinesSplit.Count())
		{
			string nextLine = _currentScriptLinesSplit[tempLineCounter].Trim();

			// look for then/do, and continue
			if (blockProcessState == -1)
			{
				if (nextLine == "then" || nextLine == "do")
				{
					blockProcessState++;

					if (nextLine == "do")
					{
						isLoopMode = true;
					}
				}
			}

			// look for lines or end
			// 1 = if content
			else if (blockProcessState >= 0)
			{
				// if its an end, stop the loop
				if ((nextLine == "fi" && isLoopMode == false) || (nextLine == "done" && isLoopMode == true))
				{
					_scriptLineCounter = tempLineCounter;

					// inject a goto into the return script to return to the
					// current line of the loop to continue executing it
					if (nextLine == "done" && currentLineReturnCode == 0)
					{
						_scriptLineCounter = currentStartLine - 1;
						LoggerManager.LogDebug("End of loop, returning to line", "", "line", _currentScriptLinesSplit[_scriptLineCounter+1]);
					}
					break;
				}

				var ifStatementParsed = ParseBlockStatementOpening(nextLine);
				if (ifStatementParsed != null && currentLineReturnCode == 1 && isLoopMode == false)
				{
					LoggerManager.LogDebug("Parsed elif, exiting out");
					_scriptLineCounter = tempLineCounter - 1;
					break;
				}

				if (nextLine == "else" && isLoopMode == false)
				{
					blockProcessState++;
					tempLineCounter++;
					continue;
				}

				if (blockProcessState == currentLineReturnCode)
				{
					parsedLines.Add(nextLine);
				}
			}

			tempLineCounter++;
		}

		return parsedLines;
	}

	public ScriptProcessResult ParseBlockStatementOpening(string line, bool parseConditions = true)
	{
		string patternBlockStatement = @"^(if|elif|while|for)\[?(.+)*\]*";
		Match isBlockStatement = Regex.Match(line, patternBlockStatement, RegexOptions.Multiline);

		if (isBlockStatement.Groups.Count >= 3 && parseConditions)
		{
			string statementType = isBlockStatement.Groups[1].Value;
			string statementCondition = isBlockStatement.Groups[2].Value.Trim();


			var conditions = ParseProcessBlockConditions(statementCondition);

			// found an if statement with no matching condition (assume it's a
			// function return code check?)
			if (conditions.Count == 0)
			{
				// strip trailing then/do from the condition
				bool conditionIsReverse = statementCondition.StartsWith("! ");
				string nonParsingCondition = statementCondition.Replace("; then", "").Replace("; do", "").Replace("! ", "");
				LoggerManager.LogDebug("Block statement non-parsing condition", "", "conditionLine", nonParsingCondition);

				var parsedFunctionCondition = ParseFunctionCalls(nonParsingCondition, verifyFunctionName: false);
				if (parsedFunctionCondition.Count > 0 && statementType != "for")
				{
					LoggerManager.LogDebug("Function found in non-parsing condition", "", "func", parsedFunctionCondition);

					// replace with a nested call and pipe to printreturncode
					// and compare as a string
					// TODO: implement this in a better way!
					line = line.Replace(statementCondition, $"(( $({nonParsingCondition} | printreturncode) {(conditionIsReverse ? "!" : "")}= 0)); {((statementType == "if" || statementType == "elif") ? "then" : "do")}");
					LoggerManager.LogDebug("Replaced statement line", "", "line", line);
					_currentScriptLinesSplit[_scriptLineCounter] = line;
					_scriptLineCounter--;

				}
				if (line.StartsWith("for"))
				{
					return new ScriptProcessResult(0);
				}

			}

			LoggerManager.LogDebug($"Parsed {statementType} start", "", "line", line);
			LoggerManager.LogDebug($"Parsed {statementType} start", "", "conditions", conditions);

			int conditionTrueCount = 0;

			int andOr = -1;
			for (int i = 0; i < conditions.Count; i++)
			{
				var condition = conditions[i];
				ScriptProcessOperation functionParse = condition.Item1[0];

				// set andOr type to AND or OR mode
				if (andOr == -1 && (condition.Item2 == "||"))
				{
					// LoggerManager.LogDebug($"Condition {i} compare type", "", "compareType", "OR");
					andOr = 1;
				}
				else if (andOr == -1 && (condition.Item2 == ""))
				{
					// LoggerManager.LogDebug($"Condition {i} compare type", "", "compareType", "AND");
					andOr = 0;
				}

				if (functionParse is ScriptProcessFunctionCall fc)
				{
					List<string> conditionParams = fc.Params;
					string conditionType = fc.Function;

					// LoggerManager.LogDebug($"Condition {i} {conditionType}", "", "condition", conditionParams);

					if (conditionType == "expr")
					{
						var conditionParseRes = (dynamic) ExecuteScriptExpression(conditionParams[0]);
						LoggerManager.LogDebug($"Condition {i} expr result", "", "res", conditionParseRes);

						if (conditionParseRes is bool rb && rb == true)
						{
							conditionTrueCount++;
						}
					}
					else if (conditionType == "condition")
					{
						LoggerManager.LogDebug("Evaluating script condition", "", "condition", conditionParams);

						bool reverseCondition = (conditionParams[0] == "!");
						if (reverseCondition)
						{
							conditionParams = conditionParams.Skip(1).ToList();
						}

						// if there's just a single param, replace it with a -n
						if (conditionParams.Count() == 1)
						{
							conditionParams.Add("-n");
							conditionParams.Reverse();
						}

						bool conditionRes = false;
						conditionParams[1] = (conditionParams[1] as string).Trim();

						// check commands
						switch (conditionParams[0])
						{
							case "-v":
								LoggerManager.LogDebug("-v", "", "cond", conditionParams[1]);
								if (_scriptVars.ContainsKey(conditionParams[1]))
								{
									conditionRes = true;
								}
								break;
							case "-z":
								LoggerManager.LogDebug("-z", "", "cond", conditionParams[1]);
								if ((conditionParams[1] as string).Length == 0)
								{
									conditionRes = true;
								}
								break;
							case "-n":
								LoggerManager.LogDebug("-n", "", "cond", conditionParams[1]);
								if ((conditionParams[1] as string).Length != 0)
								{
									conditionRes = true;
								}
								break;

							default:
								break;
						}

						// operators
						if (conditionParams.Count == 3)
						{
							switch (conditionParams[1])
							{
								case "=":
									if ((conditionParams[0] as string) == (conditionParams[2] as string))
									{
										conditionRes = true;
									}
									break;
								case "!=":
									if ((conditionParams[0] as string) != (conditionParams[2] as string))
									{
										conditionRes = true;
									}
									break;
								case "-eq":
									if (double.Parse(conditionParams[0]) == double.Parse(conditionParams[2]))
									{
										conditionRes = true;
									}
									break;
								case "-ne":
									if (double.Parse(conditionParams[0]) != double.Parse(conditionParams[2]))
									{
										conditionRes = true;
									}
									break;
								case "-lt":
									if (double.Parse(conditionParams[0]) < double.Parse(conditionParams[2]))
									{
										conditionRes = true;
									}
									break;
								case "-gt":
									if (double.Parse(conditionParams[0]) > double.Parse(conditionParams[2]))
									{
										conditionRes = true;
									}
									break;
								case "-le":
									if (double.Parse(conditionParams[0]) <= double.Parse(conditionParams[2]))
									{
										conditionRes = true;
									}
									break;
								case "-ge":
									if (double.Parse(conditionParams[0]) >= double.Parse(conditionParams[2]))
									{
										conditionRes = true;
									}
									break;

								default:
									break;
							}
						}

						LoggerManager.LogDebug(conditionRes);

						if ((reverseCondition) ? !conditionRes : conditionRes)
						{
							conditionTrueCount++;
						}
					}
				}
			}

			bool conditionTrue = false;
			if ((conditionTrueCount == conditions.Count && andOr == 0))
			{
				conditionTrue = true;
			}
			else if ((conditionTrueCount >= 1 && andOr == 1))
			{
				conditionTrue = true;
			}

			return new ScriptProcessResult((conditionTrue) ? 0 : 1, line);
		}
		else if (isBlockStatement.Groups.Count >= 3 && parseConditions == false)
		{
			return new ScriptProcessResult(0);
		}

		return null;
	}

	public object ExecuteScriptExpression(string expression)
	{
		System.Data.DataTable table = new System.Data.DataTable();
		return table.Compute(expression.ToString().Replace("!=", "NOT ="), null);
	}

	// parse a line starting with if/while/for as a block of script to be
	// treated up the stack as a single process object
	public ScriptProcessOperation ParseBlockProcessLine(string line, string[] scriptLines)
	{
		string patternBlockProcess = @"^(if|while|for)\[?(.+)*\]*";
		Match isBlockProcess = Regex.Match(line, patternBlockProcess, RegexOptions.Multiline);

		string fullScriptLine = "";

		if (isBlockProcess.Groups.Count >= 3)
		{
			string blockProcessType = isBlockProcess.Groups[1].Value;
			string blockProcessCondition = isBlockProcess.Groups[2].Value.Trim();

			List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)> blockConditions = new List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)>();

			// look over the next lines and build up the process block
			List<List<ScriptProcessOperation>> currentBlockProcesses = new List<List<ScriptProcessOperation>>();
			List<(List<ScriptProcessOperation>, string)> currentBlockCondition = null;
			List<(List<ScriptProcessOperation>, string)> prevBlockCondition = null;

			while (true)
			{
				string forwardScriptLineRaw = scriptLines[_scriptLineCounter];
				string forwardScriptLine = forwardScriptLineRaw.Trim();

				var forwardLineConditions = ParseProcessBlockConditions(forwardScriptLine);

				_scriptLineCounter++;

				if (Regex.Match(forwardScriptLine, patternBlockProcess, RegexOptions.Multiline).Groups.Count >= 3 && forwardScriptLine != line)
				{
					LoggerManager.LogDebug("Nested if found!", "", "line", $"{forwardScriptLine} {line}");
					_scriptLineCounter--;
					var parsedNestedBlock = ParseBlockProcessLine(forwardScriptLine, scriptLines);
					currentBlockProcesses.Add(new List<ScriptProcessOperation> {parsedNestedBlock});
					fullScriptLine += parsedNestedBlock.ScriptLine;
					_scriptLineCounter++;

					continue;
				}

				fullScriptLine += forwardScriptLineRaw+"\n";

				// if we have conditional matches, it's an elif or a nested if
				if (forwardLineConditions.Count > 0)
				{
					LoggerManager.LogDebug("Block conditions found in line", "", "line", forwardScriptLine);

					// set current condition to the one we just found
					if (currentBlockProcesses.Count > 0)
					{
						blockConditions.Add((currentBlockCondition, currentBlockProcesses));
					}

					currentBlockProcesses = new List<List<ScriptProcessOperation>>();
					currentBlockCondition = forwardLineConditions;
				}

				// expected when we have entered a conditional statement block
				else if (forwardScriptLine == "else")
				{
					LoggerManager.LogDebug("else line");


					// reset current processes list to account for the next
					// upcoming lines
					blockConditions.Add((currentBlockCondition, currentBlockProcesses));
					currentBlockProcesses = new List<List<ScriptProcessOperation>>();
					currentBlockCondition = null;

					continue;
				}

				// expected when we have entered a conditional statement block
				else if (forwardScriptLine == "then" || forwardScriptLine == "do")
				{
					LoggerManager.LogDebug("then/do line", "", "conditions", currentBlockCondition);


					// reset current processes list to account for the next
					// upcoming lines
					currentBlockProcesses = new List<List<ScriptProcessOperation>>();

					continue;
				}

				// end of the current block, let's exit the loop
				else if (forwardScriptLine == "fi" || forwardScriptLine == "done")
				{
					LoggerManager.LogDebug("fi/done line, reached end of block");

					// // add previous condition processes if there are any
					if (currentBlockProcesses.Count > 0)
					{
						blockConditions.Add((currentBlockCondition, currentBlockProcesses));
					}

					_scriptLineCounter--;

					LoggerManager.LogDebug("Block conditions list", "", "blockConditions", blockConditions);
					return new ScriptProcessBlockProcess(fullScriptLine, blockProcessType, blockConditions);
				}

				// we should be capturing lines as processes here
				else
				{
					// currentBlockProcesses.Add(new List<ScriptProcessOperation> {new ScriptProcessOperation(InterpretLine(forwardScriptLine).Result)});
					currentBlockProcesses.Add(new List<ScriptProcessOperation> {new ScriptProcessOperation(forwardScriptLine)});
				}


			}

		}

		return null;
	}

	// return the processed conditions from an if/while/for block
	public List<(List<ScriptProcessOperation>, string)> ParseProcessBlockConditions(string scriptLine)
	{
		string patternBlockProcessCondition = @"\[(.*?)\] ?(\|?\|?)";
		string patternBlockProcessConditionExpression = @"\(\((.*?)\)\) ?(\|?\|?)";

		MatchCollection blockProcessConditionMatches = Regex.Matches(scriptLine, patternBlockProcessCondition, RegexOptions.Multiline);
		MatchCollection blockProcessConditionMatchesExpressions = Regex.Matches(scriptLine, patternBlockProcessConditionExpression, RegexOptions.Multiline);

		List<(List<ScriptProcessOperation>, string)> conditionsList = new List<(List<ScriptProcessOperation>, string)>();

		// process condition brackets
		foreach (Match match in blockProcessConditionMatches)
		{
			string blockConditionInside = match.Groups[1].Value;
			string blockConditionCompareType = match.Groups[2].Value;

			// var interpretted = InterpretLine(blockConditionInside.Trim());
			conditionsList.Add((ParseFunctionCalls("condition " + InterpretLine(blockConditionInside.Trim(), true).Stdout, verifyFunctionName: false), blockConditionCompareType.Trim()));
		}

		// process expression matches
		foreach (Match match in blockProcessConditionMatchesExpressions)
		{
			string blockConditionInside = match.Groups[1].Value;
			string blockConditionCompareType = match.Groups[2].Value;

			// var interpretted = InterpretLine(blockConditionInside.Trim());
			conditionsList.Add((ParseFunctionCalls("expr \"" + InterpretLine(blockConditionInside.Trim(), true).Stdout+ "\"", verifyFunctionName: false), blockConditionCompareType.Trim()));
		}

		return conditionsList;
	}


	/********************************
	*  Parse script lines methods  *
	********************************/

	// parse function name and content in script line
	public List<(string FuncName, string ScriptContent, int LineCount, string RawContent)> ParseScriptLineFunction(string lines)
	{
		string patternScriptFunction = @"^([a-zA-Z0-9_]+)\(\) \{\n(^[^{}\r]+$)*\n\}";
		MatchCollection matches = Regex.Matches(lines, patternScriptFunction, RegexOptions.Multiline);

		List<(string FuncName, string ScriptContent, int LineCount, string RawContent)> funcs = new();

		foreach (Match match in matches)
		{
			Match m = match;

			if (match.Groups.Count >= 2)
			{
				funcs.Add((match.Groups[1].Value.Trim(), match.Groups[2].Value, 2 + match.Groups[2].Value.Split('\n').Length, match.Groups[0].Value));
			}

			m = m.NextMatch();
		}

		return funcs;
	}

	// return script processed lines from nested $(...) lines in a script line
	public ScriptProcessResult ParseNestedLines(string line)
	{
		List<(string, ScriptProcessResult)> processes = new List<(string, ScriptProcessResult)>();

		// string patternNestedLine = @"((?<=\$\()[^""\n]*(?=\)))";
		// string patternNestedLine = @"((?<=\$\()[^""\n](?=\)))|((?<=\$\()[^""\n]*(?=\)))";
		// string patternNestedLine = @"((?<=\$\()[^""\n](?=\)))|((?<=\$\()[^""\n\)\(]*(?=\)))";
		string patternNestedLine = @"((?<=\$\()[^\n](?=\)))|((?<=\$\()[^\n\)\(]*(?=\)))";

		ScriptProcessResult lineResult = new ScriptProcessResult(0, line);

		MatchCollection nl = Regex.Matches(line, patternNestedLine, RegexOptions.Multiline);
		foreach (Match match in nl.Reverse())
		{
			if (match.Groups.Count >= 1)
			{
				string nestedLine = match.Groups[0].Value;

				List<string> nestedLines = new List<string>();

				// LoggerManager.LogDebug("Nested line matche", "", "nestedLine", $"{nestedLine}");
				string tempFuncName = $"nested_{_scriptLineCounter}_{GetHashCode()}";
				RegisterFunctionFromContent(tempFuncName, nestedLine);

				LoggerManager.LogDebug("Nested line inner", "", "tempFuncName", tempFuncName);
				var lineResultInner = ExecuteFunctionCall(tempFuncName);
				LoggerManager.LogDebug("Nested line inner", "", "funcResult", lineResultInner);
				lineResult.ResultProcessMode = lineResultInner.ResultProcessMode;
				// processes.Add((nestedLine, lineResultInner));
				//
				lineResult.Stdout = lineResult.Result.Replace($"$({nestedLine})", lineResultInner.Result);
				LoggerManager.LogDebug("Nested line inner", "", "originalLine", nestedLine);
				LoggerManager.LogDebug("Nested line inner", "", "finalResult", lineResult);
				return lineResult;

				// stop processing them so we can process one async call at once
				if (lineResultInner.ResultProcessMode == ResultProcessMode.ASYNC)
				{
					break;
				}
			}
		}

		if (processes.Count > 0)
		{
			foreach ((string, ScriptProcessResult) nestedRes in processes)
			{
				lineResult.Stdout = lineResult.Result.Replace($"$({nestedRes.Item1})", nestedRes.Item2.Result);
				lineResult.Stderr = nestedRes.Item2.Stderr;
				lineResult.ReturnCode = nestedRes.Item2.ReturnCode;
				lineResult.ResultProcessMode = nestedRes.Item2.ResultProcessMode;

				if (lineResult.ReturnCode != 0 || lineResult.ResultProcessMode == ResultProcessMode.ASYNC)
				{
					break;
				}
			}
			
			LoggerManager.LogDebug("Nested lines result", "", "res", lineResult);
		}

		return lineResult;
	}

	// parse list of required variable substitutions in a script line
	public List<ScriptProcessOperation> ParseVarSubstitutions(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		string patternVarSubstitution = @"\$\{?([a-zA-Z0-9_@""!\[\]']+)\}?|\$([#?@*])";
		MatchCollection varSubstitutionMatches = Regex.Matches(line, patternVarSubstitution);

		foreach (Match match in varSubstitutionMatches.Reverse())
		{
			if (match.Groups.Count >= 3)
			{
				// match special var if group 2 isn't empty
				if (match.Groups[2].Value != "")
					processes.Add(new ScriptProcessVarSubstitution(line, match.Groups[2].Value));
				else
				{
					// TODO: fix the actual regex
					string varmatch = match.Groups[1].Value;
					if (varmatch.EndsWith("["))
					{
						varmatch = varmatch.TrimEnd('[');	
					}
					if (varmatch.EndsWith("\""))
					{
						varmatch = varmatch.TrimEnd('\"');	
					}
					processes.Add(new ScriptProcessVarSubstitution(line, varmatch));
				}
			}
		}

		return processes;
	}

	// parse expressions in a script line
	public List<ScriptProcessOperation> ParseExpressions(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		string patternExpression = @"\(\((.+)\)\)";
		MatchCollection expressionMatches = Regex.Matches(line, patternExpression);

		foreach (Match match in expressionMatches)
		{
			processes.Add(new ScriptProcessExpression(line, match.Groups[1].Value));
		}

		return processes;
	}

	// parse variable assignments with names and values in a script line
	public List<ScriptProcessOperation> ParseVarAssignments(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		line = line.Replace("\n", "##NL##");

		string patternIsVarAssignment = @"^[a-zA-Z0-9_\[\]""]+=.+";
		Match isVarAssignment = Regex.Match(line, patternIsVarAssignment);

		// check if it's a variable
		if (isVarAssignment.Success)
		{
			// string patternVars = @"(^\b[A-Z]+)=([\w.]+)"; // matches VAR assignments without quotes
			// string patternVars = @"(^\b[A-Z]+)=[""](\w.+)[""|\w.^]"; //
			// string patternVars = @"(^\b[A-Z_]+)=(([\w.]+)|""(.+)"")"; // matches VAR assignments with and without quotes
			string patternVars = @"(^\b[a-zA-Z0-9_\[\]""]+)=(([\w.]+)|.+)"; // matches VAR assignments with and without quotes, keeping the quotes

			// matches VAR assignments in strings
			MatchCollection m = Regex.Matches(line, patternVars, RegexOptions.Multiline);
			foreach (Match match in m)
			{
				if (match.Groups.Count >= 3)
				{
					string varName = match.Groups[1].Value;
					string varValue = match.Groups[2].Value;

					if (varValue.StartsWith("\"") && varValue.EndsWith("\""))
					{
						varValue = varValue.Trim('\"');
					}

					varValue = varValue.Replace("##NL##", "\n");

					processes.Add(new ScriptProcessVarAssignment(line, varName, varValue));
					// LoggerManager.LogDebug("Variable assignment match", "", "assignment", execLine);
				}
			}
		}

		return processes;
	}

	// parse function calls with params in a script line
	public List<ScriptProcessOperation> ParseFunctionCalls(string line, bool verifyFunctionName = true)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		// string patternFuncCall = @"(^\b[a-z_]+) (""[^""]+"")$"; // matches function calls with single param in quotes but not without
		// string patternFuncCall = @"(^\b[a-z_]+) ((""[^""]+"")$|(\w.+))"; // matches function calls with single param in quotes, and multiple param without quotes as single string (can be split?)
		// string patternFuncCall = @"(^\b[a-z_]+) ((""[^""]+"")$|(\w.+)|(([\w.]+)|.+))"; // matches single param
		string patternFuncCall = @"(^\b[a-z0-9_]+) *((""[^""]+"")*$|(\w.+)|(([\w.]+)|.+))"; // matches single param

		MatchCollection fm = Regex.Matches(line, patternFuncCall, RegexOptions.Multiline);
		foreach (Match match in fm)
		{
			if (match.Groups.Count >= 3)
			{
				string funcName = match.Groups[1].Value;
				string funcParamsStr = match.Groups[2].Value;

				List<string> funcParams = new List<string>();

				// foreach (Match fmatches in Regex.Matches(funcParamsStr, @"(?<="")[^""\n]*(?="")|[\w]+"))
				// foreach (Match fmatches in Regex.Matches(funcParamsStr, @"((?<="")[^""\n]*(?=""))|([\S]+)"))
				foreach (Match fmatches in Regex.Matches(funcParamsStr, @"((?<="")[^""\n]*(?=""))|([\w\d!£\$%\^&*\(\{})-=+_'><?/\\;,.\n]+)"))
				{
					Match nm = fmatches;

					if (nm.Groups[0].Value != " ")
					{
						funcParams.Add(nm.Groups[0].Value);
					}

					nm = nm.NextMatch();
				}

				if (IsValidFunction(funcName) || verifyFunctionName == false)
				{
					processes.Add(new ScriptProcessFunctionCall(line, funcName, funcParams));
					// LoggerManager.LogDebug("Function call match", "", "call", $"func name: {funcName}, params: [{string.Join("|", funcParams.ToArray())}]");
				}
				// else {
				// 	processes.Add(new ScriptProcessFunctionCall(line, "echo", new List<string>() { $"err: command not found: {funcName}" }));
				// }

			}
		}

		return processes;
	}

	/****************
	*  Exceptions  *
	****************/
	
	public class InvalidScriptResourceException : Exception
	{
		public InvalidScriptResourceException() {}
		public InvalidScriptResourceException(string message) : base(message) {}
		public InvalidScriptResourceException(string message, Exception inner) : base(message, inner) {}
	}
}

/**************************************
*  Script process operation classes  *
**************************************/

public class ScriptProcessOperation
{
	private string _scriptLine;
	public string ScriptLine
	{
		get { return _scriptLine; }
		set { _scriptLine = value; }
	}

	private string _type;
	public string ProcessType
	{
		get { return _type; }
		set { _type = value; }
	}

	public ScriptProcessOperation(string scriptLine)
	{
		_scriptLine = scriptLine;
		_type = this.GetType().Name;
	}
}

public class ScriptProcessVarAssignment : ScriptProcessOperation
{
	private string _varName;
	public string Name
	{
		get { return _varName; }
		set { _varName = value; }
	}

	private string _varValue;
	public string Value
	{
		get { return _varValue; }
		set { _varValue = value; }
	}

	public ScriptProcessVarAssignment(string scriptLine, string varName, string varValue) : base(scriptLine)
	{
		_varName = varName;
		_varValue = varValue;
	}
}

public class ScriptProcessVarSubstitution : ScriptProcessOperation
{
	private string _varName;
	public string Name
	{
		get { return _varName; }
		set { _varName = value; }
	}

	public ScriptProcessVarSubstitution(string scriptLine, string varName) : base(scriptLine)
	{
		_varName = varName;
	}
}

public class ScriptProcessNestedProcess : ScriptProcessOperation
{
	private List<ScriptProcessOperation> _nestedProcesses;
	public List<ScriptProcessOperation> Processes
	{
		get { return _nestedProcesses; }
		set { _nestedProcesses = value; }
	}

	public ScriptProcessNestedProcess(string scriptLine, List<ScriptProcessOperation> nestedProcesses) : base(scriptLine)
	{
		_nestedProcesses = nestedProcesses;
	}
}

public class ScriptProcessExpression : ScriptProcessOperation
{
	private string _expression;
	public string Expression
	{
		get { return _expression; }
		set { _expression = value; }
	}

	public ScriptProcessExpression(string scriptLine, string expression) : base(scriptLine)
	{
		_expression = expression;
	}
}

public class ScriptProcessFunctionCall : ScriptProcessOperation
{
	private string _funcName;
	public string Function
	{
		get { return _funcName; }
		set { _funcName = value; }
	}

	private List<string> _funcParams;
	public List<string> Params
	{
		get { return _funcParams.Select(x => x.Trim()).ToList(); }
		set { _funcParams = value; }
	}

	public ScriptProcessFunctionCall(string scriptLine, string funcName, List<string> funcParams) : base(scriptLine)
	{
		_funcName = funcName;
		_funcParams = funcParams;
	}
}

public class ScriptProcessBlockProcess : ScriptProcessOperation
{
	private string _blockType;
	public string Type
	{
		get { return _blockType; }
		set { _blockType = value; }
	}

	private List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>> BlockProcesses)> _blockProcesses;
	public List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)> Processes
	{
		get { return _blockProcesses; }
		set { _blockProcesses = value; }
	}

	public ScriptProcessBlockProcess(string scriptLine, string blockType, List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)> blockProcesses) : base(scriptLine)
	{
		_blockType = blockType;
		_blockProcesses = blockProcesses;
	}
}


/**********************************
*  Script process result classes  *
**********************************/

public enum ResultProcessMode
{
	NORMAL,
	ASYNC,
	DISCARD
}

public class ScriptProcessResult
{
	private int _returnCode;
	public int ReturnCode
	{
		get { return _returnCode; }
		set { _returnCode = value; }
	}

	private string _stdout;
	public string Stdout
	{
		get { return _stdout; }
		set { _stdout = value; }
	}

	private string _stderr;
	public string Stderr
	{
		get { return _stderr; }
		set { _stderr = value; }
	}

	public string Result
	{
		get { return GetResult(); }
	}

	private object _rawResult;
	public object RawResult
	{
		get { return _rawResult; }
		set { _rawResult = value; }
	}

	private ResultProcessMode _resultProcessMode;
	public ResultProcessMode ResultProcessMode
	{
		get { return _resultProcessMode; }
		set { _resultProcessMode = value; }
	}

	public ScriptProcessResult(int returnCode, string stdout = "", string stderr = "", object rawResult = null, ResultProcessMode resultProcessMode = ResultProcessMode.NORMAL)
	{
		_returnCode = returnCode;
		_stdout = stdout;
		_stderr = stderr;
		_resultProcessMode = resultProcessMode;
	}

	public string GetResult()
	{
		if (_returnCode == 0)
		{
			return _stdout;
		}
		else if (_returnCode != 0 && _stderr.Length > 0)
		{
			
			return $"err {_returnCode}: {_stderr}";
		}
		else
		{
			return _stdout;
		}
	}
}

public class ScriptResultOutput
{
	private ScriptProcessResult _processResult;
	public ScriptProcessResult ProcessResult
	{
		get { return _processResult; }
		set { _processResult = value; }
	}

	private GameScript _gameScript;
	public GameScript Script
	{
		get { return _gameScript; }
		set { _gameScript = value; }
	}

	private string _output;
	public string Output
	{
		get { return _output; }
		set { _output = value; }
	}

	private int _lineNumber;
	public int LineNumber
	{
		get { return _lineNumber; }
		set { _lineNumber = value; }
	}

	public ScriptResultOutput(ScriptProcessResult processResult, string output, int lineNumber, GameScript gameScript)
	{
		_processResult = processResult;
		_output = output;
		_lineNumber = lineNumber;
		_gameScript = gameScript;
	}
}
