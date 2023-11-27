/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ScriptingTest
 * @created     : Monday Nov 13, 2023 22:56:15 CST
 */

namespace GodotEGP.Scripting;

// using System;
// using System.Collections.Generic;
// using System.Text.RegularExpressions;
// using System.Linq;
//
// using Godot;
// using GodotEGP.Objects.Extensions;
// using GodotEGP.Logging;
// using GodotEGP.Service;
// using GodotEGP.Event.Events;
// using GodotEGP.Config;
//
// using GodotEGP.State;
// using GodotEGP.Resource;
//
// public partial class ScriptingTest : Node
// {
// 	// state classes for process state machine
// 	public class Process : HStateMachine {}
// 	public class Preparing : HStateMachine {}
// 	public class Running : HStateMachine {}
// 	public class Waiting : HStateMachine {}
// 	public class Finished : HStateMachine {}
//
// 	private Process _processState = new Process();
// 	private Preparing _statePreparing = new Preparing();
// 	private Running _stateRunning = new Running();
// 	private Waiting _stateWaiting = new Waiting();
// 	private Finished _stateFinished = new Finished();
//
// 	private const int STATE_RUNNING = 0;
// 	private const int STATE_WAITING = 1;
// 	private const int STATE_FINISHED = 2;
//
// 	private string _script;
// 	private string[] _currentScriptLinesSplit;
//
// 	private int _scriptLineCounter = 0;
//
// 	private Dictionary<string, Func<int, string>> _functionDefinitions = new Dictionary<string, Func<int, string>>();
//
// 	// holds session variables used by the script
// 	Dictionary<string, object> _scriptVars = new Dictionary<string, object>();
//
// 	private List<ScriptProcessResult> _scriptLineResults = new List<ScriptProcessResult>();
// 	private ScriptProcessResult _scriptLineResult;
//
// 	public ScriptingTest(string script)
// 	{
// 		var scriptResources = ServiceRegistry.Get<ResourceManager>().GetResources<GameScript>();
// 		LoggerManager.LogDebug("script resources", "", "resources", scriptResources);
//
// 		_script = script;
//
// 		// setup process sub-states
// 		_statePreparing.OnEnter = _State_Preparing_OnEnter;
// 		_stateRunning.OnUpdate = _State_Running_OnUpdate;
// 		_stateWaiting.OnUpdate = _State_Waiting_OnUpdate;
//
// 		_processState.AddState(_statePreparing);	
// 		_processState.AddState(_stateRunning);	
// 		_processState.AddState(_stateWaiting);	
//
// 		// create state transitions
// 		_processState.AddTransition(_statePreparing, _stateRunning, STATE_RUNNING);
// 		_processState.AddTransition(_stateRunning, _stateWaiting, STATE_WAITING);
// 		_processState.AddTransition(_stateWaiting, _stateRunning, STATE_RUNNING);
// 		_processState.AddTransition(_stateRunning, _stateFinished, STATE_FINISHED);
//
// 		// Start the state machine
// 		_processState.Enter();
// 	}
//
// 	public void _State_Preparing_OnEnter()
// 	{
// 		if (_script.Length == 0)
// 		{
// 			_script += @"echo ""this text should act like a simple print statement""\n";
// 			_script += @"echo ""testing: setting variables content""\n";
// 			_script += @"VARNAME=""some string value""\n";
// 			_script += @"echo ""testing: echoing variables: $VARNAME""\n";
// 			_script += @"echo ""testing: setting variables to content with variables inside""\n";
// 			_script += @"HOME=""where the heart is""\n";
// 			_script += @"VARNAME=""home is $HOME""\n";
// 			_script += @"echo ""did you know? $VARNAME""\n";
// 			_script += @"VARNAME=""$(echo I don't really like soup...)""\n";
// 			_script += @"echo ""but did you know? $VARNAME""\n";
// 			_script += @"logdebug ""logging to debug log""\n";
// 			_script += @"echo ""testing"" ""multiple"" ""echo"" ""params""\n";
// 			_script += @"echo echo without quotes\n";
// 			_script += @"echo 1 2 3\n";
// 			_script += @"echo ""testing: setting variables to number types""\n";
// 			_script += @"VARINT=1\n";
// 			_script += @"VARFLOAT=1.1\n";
// 			_script += @"echo ""testing: enclosed script lines content""\n";
// 			_script += @"echo ""$(echo this should return this string)""\n";
// 			_script += @"echo ""$(echo this is part)"" ""$(echo of multiple)"" ""$(echo nested lines)""\n";
// 			_script += @"echo ""testing: accessing array elements""\n";
// 			_script += @"echo ""array key 0: $VARARRAY[0]""\n";
// 			_script += @"echo ""array key 1: $VARARRAY[1]""\n";
// 			_script += @"echo ""testing: accessing dictionary elements""\n";
// 			_script += @"echo ""array key 'key':$VARARRAY['key']""\n";
//
// 			// async wait function call process mode
// 			_script += @"echo ""testing: async wait""\n";
// 			_script += @"waittest\n";
// 			_script += @"echo ""this shouldn't be shown until processing is resumed""\n";
//
//
// 			// if statements
// 			// _script += @"if [ 1 -gt 100]\n";
// 			// _script += @"then\n";
// 			// _script += @"  echo omg such a large number\n";
// 			// _script += @"fi\n";
//             //
// 			// _script += @"if [ 1 -gt 100] || [ 1 -le 100]\n";
// 			// _script += @"then\n";
// 			// _script += @"  echo uh ok\n";
// 			// _script += @"fi\n";
//
// 			// _script += @"if [ ""2"" == ""2"" ]\n";
// 			// _script += @"then\n";
// 			// _script += @"  echo omg such a large number\n";
// 			// _script += @"fi\n";
//
// 			// _script += @"if [ ""$SOMEVARVAL"" = ""1"" ]\n";
// 			// _script += @"then\n";
// 			// _script += @"  echo It's equal to 1 yay\n";
// 			// _script += @"elif [ ""$SOMEVARVAL"" = ""$(somefunccall random_param_1 another_param)"" ]\n";
// 			// _script += @"then\n";
// 			// _script += @"  echo did you know? $(echo this is nested!)\n";
// 			// _script += @"else\n";
// 			// _script += @"  echo eh it's actually ""$SOMEVARVAL""\n";
// 			// _script += @"fi\n";
//
// 			// while loops
// 			// _script += @"counter=1\n";
// 			// _script += @"while [ $counter -le 10 ]\n";
// 			// _script += @"do\n";
// 			// _script += @"  echo count: $counter\n";
// 			// _script += @"  ((counter++))\n";
// 			// _script += @"done\n";
//
// 			// for loops
// 			// _script += @"names=""name1 name2 name3""\n";
// 			// _script += @"for name in $names\n";
// 			// _script += @"do\n";
// 			// _script += @"  echo name: $name\n";
// 			// _script += @"done\n";
//
// 			// for loops range
// 			// _script += @"for val in {1..5}\n";
// 			// _script += @"do\n";
// 			// _script += @"  echo val: $val\n";
// 			// _script += @"done\n";
//
// 			// multiline with commas
// 			// _script += @"echo one; echo two; echo three\n";
// 			// _script += @"echo one; echo ""$(echo a; echo b)""; echo three\n";
//
// 			// nested if else else
// 			// _script += @"if [ ""2"" = ""2"" ]\n";
// 			// _script += @"then\n";
// 			// _script += @"  if [ ""a"" = ""a"" ]\n";
// 			// _script += @"  then\n";
// 			// _script += @"    echo omg such a large number\n";
// 			// _script += @"  else\n";
// 			// _script += @"    echo not a large number...\n";
// 			// _script += @"  fi\n";
// 			// _script += @"else\n";
// 			// _script += @"  echo it's an else\n";
// 			// _script += @"fi\n";
//
// 			// some var setting tests
// 			_script += @"c=""$(a)$(b)""\n";
// 			_script += @"c=""$( ((a + b)) )""\n";
// 		}
// 		LoggerManager.LogDebug(_script);
//
// 		_currentScriptLinesSplit = _script.Split(new string[] {"\\n"}, StringSplitOptions.None);
//
// 		_processState.Transition(STATE_RUNNING);
// 	}
//
// 	public void _State_Running_OnUpdate()
// 	{
// 		if (_scriptLineCounter >= _currentScriptLinesSplit.Count())
// 		{
// 			_processState.Transition(STATE_FINISHED); // end of the script
// 			return;
// 		}
//
// 		// retrive the current script line
// 		string linestr = _currentScriptLinesSplit[_scriptLineCounter].Trim();
//
// 		// process the line if it's not empty
// 		// TODO: figure out why/how to remove empty lines, or just let them
// 		// happen
// 		if (linestr.Length > 0)
// 		{
// 			_scriptLineResult = InterpretLine(linestr);
// 			_scriptLineResults.Add(_scriptLineResult);
//
// 			// increase script line after processing
// 			_scriptLineCounter++;
//
// 			LoggerManager.LogDebug($"Line {_scriptLineCounter}", "", "line", $"[{_scriptLineResult.ReturnCode}] {_scriptLineResult.Result}");
//
// 			if (_scriptLineResult.ResultProcessMode == ResultProcessMode.ASYNC)
// 			{
// 				// we are waiting for something, so switch processing mode
// 				_processState.Transition(STATE_WAITING);
// 			}
// 			else
// 			{
// 				// trigger another update to process the next line
// 				_processState.Update();
// 			}
// 		}
// 	}
//
// 	public void _State_Waiting_OnUpdate()
// 	{
// 		LoggerManager.LogDebug("Pretend we waited for something long...");
// 		_processState.Transition(STATE_RUNNING);
// 	}
//
// 	public override void _Process(double delta)
// 	{
// 		_processState.Update();
// 		// if (_scriptLineCounter >= _currentScriptLinesSplit.Count())
// 		// {
// 		// 	_processState = -1; // end of the script
// 		// }
//         //
//         //
// 		// // regular process state, let's process line by line!
// 		// if (_processState == 0)
// 		// {
// 		// 	string linestr = _currentScriptLinesSplit[_scriptLineCounter].Trim();
//         //
// 		// 	if (linestr.Length > 0)
// 		// 	{
// 		// 		_scriptLineResult = InterpretLine(linestr);
// 		// 		_scriptLineResults.Add(_scriptLineResult);
//         //
// 		// 		LoggerManager.LogDebug($"Line {_scriptLineCounter}", "", "line", $"[{_scriptLineResult.ReturnCode}] {_scriptLineResult.Result}");
//         //
// 		// 		if (_scriptLineResult.ResultProcessMode == ResultProcessMode.ASYNC)
// 		// 		{
// 		// 			// we are waiting for something, so switch processing mode
// 		// 			_processState = 1;
// 		// 		}
// 		// 	}
//         //
// 		// 	_scriptLineCounter++;
// 		// }
// 	}
//
// 	// public void ProcessInterprettedLines(List<List<ScriptProcessOperation>> interprettedLines)
// 	// {
// 	// 	// list structure:
// 	// 	// list of lines
// 	// 		// list of line processes
// 	// 	for (int i = 0; i < interprettedLines.Count; i++)
// 	// 	{
// 	// 		List<ScriptProcessOperation> lineProcesses = interprettedLines[i];
//     //
// 	// 		ProcessInterprettedLine(interprettedLines[i]);
// 	// 	}
// 	// }
//     //
// 	// public ScriptProcessResult ProcessInterprettedLine(List<ScriptProcessOperation> interprettedLine)
// 	// {
// 	// 	ScriptProcessResult lineResult = null;
//     //
// 	// 	for (int ii = 0; ii < interprettedLine.Count; ii++)
// 	// 	{
// 	// 		ScriptProcessOperation currentProcess = interprettedLine[ii];
//     //
// 	// 		if (lineResult == null)
// 	// 		{
// 	// 			lineResult = new ScriptProcessResult(0, "", "");
// 	// 			lineResult.Stdout = currentProcess.ScriptLine;
// 	// 		}
//     //
// 	// 		if (ii == 0)
// 	// 		{
// 	// 			LoggerManager.LogDebug($"Line {ii}", "Process", "line", interprettedLine[ii].ScriptLine);
// 	// 		}
//     //
// 	// 		LoggerManager.LogDebug($"Line {ii} process {ii} {interprettedLine[ii].GetType().Name}", "Process", "process", interprettedLine[ii]);
//     //
// 	// 		// process function call
// 	// 		if (currentProcess is ScriptProcessFunctionCall functionCall)
// 	// 		{
// 	// 			lineResult = ExecuteFunctionCall(functionCall.Function, functionCall.Params.ToArray());
// 	// 			LoggerManager.LogDebug("Function call result", "Process", "res", lineResult);
// 	// 		}
// 	// 		else if (currentProcess is ScriptProcessVarAssignment varAssignment)
// 	// 		{
// 	// 			lineResult = ExecuteVariableAssignment(varAssignment.Name, varAssignment.Value);
// 	// 			LoggerManager.LogDebug("Var assignment result", "Process", "res", lineResult);
// 	// 		}
// 	// 		else if (currentProcess is ScriptProcessVarSubstitution varSubstitution)
// 	// 		{
// 	// 			lineResult = ExecuteVariableSubstitution(varSubstitution.Name, lineResult);
// 	// 			LoggerManager.LogDebug("Var substitution result", "Process", "res", lineResult);
// 	// 		}
//     //
// 	// 		// last just check if it's basic operation and write script line
// 	// 		// as-is
// 	// 		else if (currentProcess is ScriptProcessOperation operation && operation.ScriptLine.Length > 0)
// 	// 		{
// 	// 			lineResult = new ScriptProcessResult(0, operation.ScriptLine);
// 	// 		}
// 	// 	}
//     //
// 	// 	if (lineResult == null)
// 	// 	{
// 	// 		lineResult = new ScriptProcessResult(127, "", "unprocessed script line");
// 	// 	}
//     //
// 	// 	LoggerManager.LogDebug($"Line final result", "Process", "res", lineResult);
//     //
// 	// 	return lineResult;
// 	// }
//
// 	// main script process execution functions
// 	public ScriptProcessResult ExecuteFunctionCall(string func, params string[] funcParams)
// 	{
// 		if (func == "echo")
// 		{
// 			return new ScriptProcessResult(0, funcParams.Join(" "));
// 		}
// 		if (func == "err")
// 		{
// 			return new ScriptProcessResult(0, func+" "+funcParams.Join(" "));
// 		}
// 		if (func == "waittest")
// 		{
// 			LoggerManager.LogDebug("Waittest called");
// 			return new ScriptProcessResult(0, resultProcessMode: ResultProcessMode.ASYNC);
// 		}
//
// 		return new ScriptProcessResult(127, "", $"command not found: {func}");
// 	}
//
// 	public ScriptProcessResult ExecuteVariableAssignment(string varName, string varValue)
// 	{
// 		// parse variable name and keys
// 		string varnamePattern = @"\[([^\\]]*)\]";
// 		MatchCollection matches = Regex.Matches(varName, varnamePattern, RegexOptions.Multiline);
//
// 		// TODO: implement nested variable access by parsing key names
// 		if (matches.Count > 0)
// 		{
// 			LoggerManager.LogDebug("TODO: implement nested variable access", "", "varname", varName);
//
// 			return new ScriptProcessResult(127, "", "variable key access not implemented");
// 		}
// 		else
// 		{
// 			AssignVariableValue(varName, varValue);
// 			return new ScriptProcessResult(0);
// 		}
// 	}
//
// 	public void AssignVariableValue(string varName, string varValue)
// 	{
// 		LoggerManager.LogDebug("Setting variable value", "", "var", $"{varName} = {varValue}");
//
// 		_scriptVars[varName] = varValue;
// 	}
//
// 	public ScriptProcessResult ExecuteVariableSubstitution(string varName, ScriptProcessResult res)
// 	{
// 		if (!_scriptVars.TryGetValue(varName, out object obj))
// 		{
// 			// set empty string for non-existent vars
// 			obj = (string) "";
// 		}
//
// 		return new ScriptProcessResult(0, res.Result.Replace("$"+varName, obj.ToString()));
// 	}
//
// 	// accepts a pure string containing the script content to process for
// 	// interpretation
// 	public List<ScriptProcessResult> InterpretLines(string scriptLines)
// 	{
// 		List<ScriptProcessResult> processes = new List<ScriptProcessResult>();
//
// 		_currentScriptLinesSplit = scriptLines.Split(new string[] {"\\n"}, StringSplitOptions.None);
//
// 		while (_scriptLineCounter < _currentScriptLinesSplit.Count())
// 		{
// 			string linestr = _currentScriptLinesSplit[_scriptLineCounter].Trim();
//
// 			if (linestr.Length > 0)
// 			{
// 				processes.Add(InterpretLine(linestr));
// 			}
//
// 			_scriptLineCounter++;
// 		}
//
// 		return processes;
// 	}
//
// 	// parse a line starting with if/while/for as a block of script to be
// 	// treated up the stack as a single process object
// 	public ScriptProcessOperation ParseBlockProcessLine(string line, string[] scriptLines)
// 	{
// 		string patternBlockProcess = @"^(if|while|for)\[?(.+)*\]*";
// 		Match isBlockProcess = Regex.Match(line, patternBlockProcess, RegexOptions.Multiline);
//
// 		string fullScriptLine = "";
//
// 		if (isBlockProcess.Groups.Count >= 3)
// 		{
// 			string blockProcessType = isBlockProcess.Groups[1].Value;
// 			string blockProcessCondition = isBlockProcess.Groups[2].Value.Trim();
//
// 			List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)> blockConditions = new List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)>();
//
// 			// look over the next lines and build up the process block
// 			List<List<ScriptProcessOperation>> currentBlockProcesses = new List<List<ScriptProcessOperation>>();
// 			List<(List<ScriptProcessOperation>, string)> currentBlockCondition = null;
// 			List<(List<ScriptProcessOperation>, string)> prevBlockCondition = null;
//
// 			while (true)
// 			{
// 				string forwardScriptLineRaw = scriptLines[_scriptLineCounter];
// 				string forwardScriptLine = forwardScriptLineRaw.Trim();
//
// 				var forwardLineConditions = ParseProcessBlockConditions(forwardScriptLine);
//
// 				_scriptLineCounter++;
//
// 				if (Regex.Match(forwardScriptLine, patternBlockProcess, RegexOptions.Multiline).Groups.Count >= 3 && forwardScriptLine != line)
// 				{
// 					LoggerManager.LogDebug("Nested if found!", "", "line", $"{forwardScriptLine} {line}");
// 					_scriptLineCounter--;
// 					var parsedNestedBlock = ParseBlockProcessLine(forwardScriptLine, scriptLines);
// 					currentBlockProcesses.Add(new List<ScriptProcessOperation> {parsedNestedBlock});
// 					fullScriptLine += parsedNestedBlock.ScriptLine;
// 					_scriptLineCounter++;
//
// 					continue;
// 				}
//
// 				fullScriptLine += forwardScriptLineRaw+"\n";
//
// 				// if we have conditional matches, it's an elif or a nested if
// 				if (forwardLineConditions.Count > 0)
// 				{
// 					LoggerManager.LogDebug("Block conditions found in line", "", "line", forwardScriptLine);
//
// 					// set current condition to the one we just found
// 					if (currentBlockProcesses.Count > 0)
// 					{
// 						blockConditions.Add((currentBlockCondition, currentBlockProcesses));
// 					}
//
// 					currentBlockProcesses = new List<List<ScriptProcessOperation>>();
// 					currentBlockCondition = forwardLineConditions;
// 				}
//
// 				// expected when we have entered a conditional statement block
// 				else if (forwardScriptLine == "else")
// 				{
// 					LoggerManager.LogDebug("else line");
//
//
// 					// reset current processes list to account for the next
// 					// upcoming lines
// 					blockConditions.Add((currentBlockCondition, currentBlockProcesses));
// 					currentBlockProcesses = new List<List<ScriptProcessOperation>>();
// 					currentBlockCondition = null;
//
// 					continue;
// 				}
//
// 				// expected when we have entered a conditional statement block
// 				else if (forwardScriptLine == "then" || forwardScriptLine == "do")
// 				{
// 					LoggerManager.LogDebug("then/do line", "", "conditions", currentBlockCondition);
//
//
// 					// reset current processes list to account for the next
// 					// upcoming lines
// 					currentBlockProcesses = new List<List<ScriptProcessOperation>>();
//
// 					continue;
// 				}
//
// 				// end of the current block, let's exit the loop
// 				else if (forwardScriptLine == "fi" || forwardScriptLine == "done")
// 				{
// 					LoggerManager.LogDebug("fi/done line, reached end of block");
//
// 					// // add previous condition processes if there are any
// 					if (currentBlockProcesses.Count > 0)
// 					{
// 						blockConditions.Add((currentBlockCondition, currentBlockProcesses));
// 					}
//
// 					_scriptLineCounter--;
//
// 					LoggerManager.LogDebug("Block conditions list", "", "blockConditions", blockConditions);
// 					return new ScriptProcessBlockProcess(fullScriptLine, blockProcessType, blockConditions);
// 				}
//
// 				// we should be capturing lines as processes here
// 				else
// 				{
// 					currentBlockProcesses.Add(new List<ScriptProcessOperation> {new ScriptProcessOperation(InterpretLine(forwardScriptLine).Result)});
// 				}
//
//
// 			}
//
// 		}
//
// 		return null;
// 	}
//
// 	// return the processed conditions from an if/while/for block
// 	public List<(List<ScriptProcessOperation>, string)> ParseProcessBlockConditions(string scriptLine)
// 	{
// 		string patternBlockProcessCondition = @"\[(.*?)\] ?(\|?\|?)";
//
// 		MatchCollection blockProcessConditionMatches = Regex.Matches(scriptLine, patternBlockProcessCondition, RegexOptions.Multiline);
//
// 		List<(List<ScriptProcessOperation>, string)> conditionsList = new List<(List<ScriptProcessOperation>, string)>();
//
// 		if (scriptLine.StartsWith("for "))
// 		{
// 			conditionsList.Add((new List<ScriptProcessOperation> {new ScriptProcessOperation(InterpretLine(scriptLine.Replace("for ", "")).Result)}, ""));
// 		}
//
// 		foreach (Match match in blockProcessConditionMatches)
// 		{
// 			string blockConditionInside = match.Groups[1].Value;
// 			string blockConditionCompareType = match.Groups[2].Value;
//
// 			var interpretted = InterpretLine(blockConditionInside.Trim());
//
// 			conditionsList.Add((new List<ScriptProcessOperation> {new ScriptProcessOperation(InterpretLine(blockConditionInside.Trim()).Result)}, blockConditionCompareType.Trim()));
// 		}
//
// 		return conditionsList;
// 	}
//
// 	// accepts a single script line and generates a list of process objects to
// 	// achieve the final rendered result for each line
// 	public ScriptProcessResult InterpretLine(string line)
// 	{
// 		// TODO: split and process lines with ; and pipes
//
// 		// execution and parse order
// 		// 1. parse printed vars to real values in unparsed line
// 		// parse var names in expressions (( )) and replace with actual
// 		// values e.g. number or string
// 		// 2. parse nested lines as a normal line, replacing the executed result
// 		// 3. parse variable assignments
// 		// 4. parse function calls
// 		// 5. parse if/while/for calls
//
// 		ScriptProcessResult lineResult = new ScriptProcessResult(0, line);
//
// 		// list of process operations to do to this script line
// 		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();
//
// 		// first thing, parse and replace variable names with values
// 		foreach (ScriptProcessVarSubstitution lineProcess in ParseVarSubstitutions(lineResult.Result))
// 		{
// 			lineResult = ExecuteVariableSubstitution(lineProcess.Name, lineResult);
// 		}
// 		// processes.AddRange(ParseVarSubstitutions(line));
//
// 		// second thing, parse and replace expressions with values
// 		foreach (ScriptProcessExpression lineProcess in ParseExpressions(lineResult.Result))
// 		{
// 			// TODO: implement expression processing
// 			// lineResult = ExecuteExpression(lineProcess.Expression, lineResult);
// 		}
// 		// processes.AddRange(ParseExpressions(line));
//
// 		// third thing, parse nested script lines and replace values
// 		lineResult = ParseNestedLines(lineResult.Result);
// 		if (lineResult.ReturnCode != 0)
// 		{
// 			return lineResult;
// 		}
// 		// foreach (ScriptProcessNestedProcess lineProcess in ParseNestdLines(lineResult.Stdout))
// 		// {
// 		// 	// TODO: process nested lines
// 		// 	// lineResult = ExecuteVariableSubstitution(lineProcess.Name, lineResult);
// 		// }
// 		// processes.AddRange(ParseNestdLines(line));
//
//
// 		// parse variable assignments
// 		var varAssignmentProcesses = ParseVarAssignments(lineResult.Result);
// 		// processes.AddRange(varAssignmentProcesses);
// 		foreach (ScriptProcessVarAssignment lineProcess in varAssignmentProcesses)
// 		{
// 			lineResult = ExecuteVariableAssignment(lineProcess.Name, lineProcess.Value);
// 		}
// 		if (lineResult.ReturnCode != 0)
// 		{
// 			return lineResult;
// 		}
//
// 		var blockProcess = ParseBlockProcessLine(line, _currentScriptLinesSplit);
// 		if (blockProcess != null)
// 		{
// 			processes.AddRange(new List<ScriptProcessOperation>() {blockProcess});
// 		}
// 		else
// 		{
// 			// if var assignments are 0, then try to match function calls
// 			// NOTE: this is because the regex matches both var assignments in lower
// 			// case AND function calls
// 			if (varAssignmentProcesses.Count == 0)
// 			{
// 				foreach (ScriptProcessFunctionCall lineProcess in ParseFunctionCalls(lineResult.Result))
// 				{
// 					lineResult = ExecuteFunctionCall(lineProcess.Function, lineProcess.Params.ToArray());
// 				}
// 				// processes.AddRange(ParseFunctionCalls(line));
// 			}
// 		}
//
// 		// if there's no processes until now, just return the plain object with
// 		// no processing attached
// 		// if (processes.Count == 0)
// 		// {
// 		// 	processes.Add(new ScriptProcessOperation(line));
// 		// }
// 		
// 		// LoggerManager.LogDebug("Line result", "", "res", lineResult);
//
// 		return lineResult;
// 	}
//
// 	// return script processed lines from nested $(...) lines in a script line
// 	public ScriptProcessResult ParseNestedLines(string line)
// 	{
// 		List<(string, ScriptProcessResult)> processes = new List<(string, ScriptProcessResult)>();
//
// 		// string patternNestedLine = @"((?<=\$\()[^""\n]*(?=\)))";
// 		// string patternNestedLine = @"((?<=\$\()[^""\n](?=\)))|((?<=\$\()[^""\n]*(?=\)))";
// 		string patternNestedLine = @"((?<=\$\()[^""\n](?=\)))|((?<=\$\()[^""\n\)\(]*(?=\)))";
//
// 		ScriptProcessResult lineResult = new ScriptProcessResult(0, line);
//
// 		MatchCollection nl = Regex.Matches(line, patternNestedLine, RegexOptions.Multiline);
// 		foreach (Match match in nl)
// 		{
// 			if (match.Groups.Count >= 1)
// 			{
// 				string nestedLine = match.Groups[0].Value;
//
// 				List<string> nestedLines = new List<string>();
//
// 				// LoggerManager.LogDebug("Nested line matche", "", "nestedLine", $"{nestedLine}");
//
// 				processes.Add((nestedLine, InterpretLine(nestedLine)));
// 				// lineResult = InterpretLine(nestedLine);
// 			}
// 		}
//
// 		if (processes.Count > 0)
// 		{
// 			foreach ((string, ScriptProcessResult) nestedRes in processes)
// 			{
// 				lineResult.Stdout = lineResult.Result.Replace($"$({nestedRes.Item1})", nestedRes.Item2.Result);
// 				lineResult.Stderr = nestedRes.Item2.Stderr;
// 				lineResult.ReturnCode = nestedRes.Item2.ReturnCode;
//
// 				if (lineResult.ReturnCode != 0)
// 				{
// 					break;
// 				}
// 			}
// 			LoggerManager.LogDebug("Nested lines result", "", "res", lineResult);
// 		}
//
// 		return lineResult;
// 	}
//
// 	// parse list of required variable substitutions in a script line
// 	public List<ScriptProcessOperation> ParseVarSubstitutions(string line)
// 	{
// 		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();
//
// 		string patternVarSubstitution = @"\$([a-zA-Z0-9_\[\]']+)";
// 		MatchCollection varSubstitutionMatches = Regex.Matches(line, patternVarSubstitution);
//
// 		foreach (Match match in varSubstitutionMatches)
// 		{
// 			if (match.Groups.Count >= 2)
// 			{
// 				processes.Add(new ScriptProcessVarSubstitution(line, match.Groups[1].Value));
// 			}
// 		}
//
// 		return processes;
// 	}
//
// 	// parse expressions in a script line
// 	public List<ScriptProcessOperation> ParseExpressions(string line)
// 	{
// 		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();
//
// 		string patternExpression = @"\(\((.+)\)\)";
// 		MatchCollection expressionMatches = Regex.Matches(line, patternExpression);
//
// 		foreach (Match match in expressionMatches)
// 		{
// 			processes.Add(new ScriptProcessExpression(line, match.Groups[1].Value));
// 		}
//
// 		return processes;
// 	}
//
// 	// parse variable assignments with names and values in a script line
// 	public List<ScriptProcessOperation> ParseVarAssignments(string line)
// 	{
// 		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();
//
// 		string patternIsVarAssignment = @"^[a-zA-Z0-9_]+=.+";
// 		Match isVarAssignment = Regex.Match(line, patternIsVarAssignment);
//
// 		// check if it's a variable
// 		if (isVarAssignment.Success)
// 		{
// 			// string patternVars = @"(^\b[A-Z]+)=([\w.]+)"; // matches VAR assignments without quotes
// 			// string patternVars = @"(^\b[A-Z]+)=[""](\w.+)[""|\w.^]"; //
// 			// string patternVars = @"(^\b[A-Z_]+)=(([\w.]+)|""(.+)"")"; // matches VAR assignments with and without quotes
// 			string patternVars = @"(^\b[a-zA-Z0-9_]+)=(([\w.]+)|.+)"; // matches VAR assignments with and without quotes, keeping the quotes
//
// 			// matches VAR assignments in strings
// 			MatchCollection m = Regex.Matches(line, patternVars, RegexOptions.Multiline);
// 			foreach (Match match in m)
// 			{
// 				if (match.Groups.Count >= 3)
// 				{
// 					string varName = match.Groups[1].Value;
// 					string varValue = match.Groups[2].Value;
//
// 					if (varValue.StartsWith("\"") && varValue.EndsWith("\""))
// 					{
// 						varValue = varValue.Trim('\"');
// 					}
//
// 					processes.Add(new ScriptProcessVarAssignment(line, varName, varValue));
// 					// LoggerManager.LogDebug("Variable assignment match", "", "assignment", execLine);
// 				}
// 			}
// 		}
//
// 		return processes;
// 	}
//
// 	// parse function calls with params in a script line
// 	public List<ScriptProcessOperation> ParseFunctionCalls(string line)
// 	{
// 		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();
//
// 		// string patternFuncCall = @"(^\b[a-z_]+) (""[^""]+"")$"; // matches function calls with single param in quotes but not without
// 		// string patternFuncCall = @"(^\b[a-z_]+) ((""[^""]+"")$|(\w.+))"; // matches function calls with single param in quotes, and multiple param without quotes as single string (can be split?)
// 		// string patternFuncCall = @"(^\b[a-z_]+) ((""[^""]+"")$|(\w.+)|(([\w.]+)|.+))"; // matches single param
// 		string patternFuncCall = @"(^\b[a-z_]+) *((""[^""]+"")*$|(\w.+)|(([\w.]+)|.+))"; // matches single param
//
// 		MatchCollection fm = Regex.Matches(line, patternFuncCall, RegexOptions.Multiline);
// 		foreach (Match match in fm)
// 		{
// 			if (match.Groups.Count >= 3)
// 			{
// 				string funcName = match.Groups[1].Value;
// 				string funcParamsStr = match.Groups[2].Value;
//
// 				List<string> funcParams = new List<string>();
//
// 				// foreach (Match fmatches in Regex.Matches(funcParamsStr, @"(?<="")[^""\n]*(?="")|[\w]+"))
// 				foreach (Match fmatches in Regex.Matches(funcParamsStr, @"((?<="")[^""\n]*(?=""))|([\w-_'\:]+)"))
// 				{
// 					Match nm = fmatches;
//
// 					if (nm.Groups[0].Value != " ")
// 					{
// 						funcParams.Add(nm.Groups[0].Value);
// 					}
//
// 					nm = nm.NextMatch();
// 				}
//
// 				processes.Add(new ScriptProcessFunctionCall(line, funcName, funcParams));
// 				// LoggerManager.LogDebug("Function call match", "", "call", $"func name: {funcName}, params: [{string.Join("|", funcParams.ToArray())}]");
// 			}
// 		}
//
// 		return processes;
// 	}
// }
//
//
//
// public class ScriptProcessOperation
// {
// 	private string _scriptLine;
// 	public string ScriptLine
// 	{
// 		get { return _scriptLine; }
// 		set { _scriptLine = value; }
// 	}
//
// 	private string _type;
// 	public string ProcessType
// 	{
// 		get { return _type; }
// 		set { _type = value; }
// 	}
//
// 	public ScriptProcessOperation(string scriptLine)
// 	{
// 		_scriptLine = scriptLine;
// 		_type = this.GetType().Name;
// 	}
// }
//
// public class ScriptProcessVarAssignment : ScriptProcessOperation
// {
// 	private string _varName;
// 	public string Name
// 	{
// 		get { return _varName; }
// 		set { _varName = value; }
// 	}
//
// 	private string _varValue;
// 	public string Value
// 	{
// 		get { return _varValue; }
// 		set { _varValue = value; }
// 	}
//
// 	public ScriptProcessVarAssignment(string scriptLine, string varName, string varValue) : base(scriptLine)
// 	{
// 		_varName = varName;
// 		_varValue = varValue;
// 	}
// }
//
// public class ScriptProcessVarSubstitution : ScriptProcessOperation
// {
// 	private string _varName;
// 	public string Name
// 	{
// 		get { return _varName; }
// 		set { _varName = value; }
// 	}
//
// 	public ScriptProcessVarSubstitution(string scriptLine, string varName) : base(scriptLine)
// 	{
// 		_varName = varName;
// 	}
// }
//
// public class ScriptProcessNestedProcess : ScriptProcessOperation
// {
// 	private List<ScriptProcessOperation> _nestedProcesses;
// 	public List<ScriptProcessOperation> Processes
// 	{
// 		get { return _nestedProcesses; }
// 		set { _nestedProcesses = value; }
// 	}
//
// 	public ScriptProcessNestedProcess(string scriptLine, List<ScriptProcessOperation> nestedProcesses) : base(scriptLine)
// 	{
// 		_nestedProcesses = nestedProcesses;
// 	}
// }
//
// public class ScriptProcessExpression : ScriptProcessOperation
// {
// 	private string _expression;
// 	public string Expression
// 	{
// 		get { return _expression; }
// 		set { _expression = value; }
// 	}
//
// 	public ScriptProcessExpression(string scriptLine, string expression) : base(scriptLine)
// 	{
// 		_expression = expression;
// 	}
// }
//
// public class ScriptProcessFunctionCall : ScriptProcessOperation
// {
// 	private string _funcName;
// 	public string Function
// 	{
// 		get { return _funcName; }
// 		set { _funcName = value; }
// 	}
//
// 	private List<string> _funcParams;
// 	public List<string> Params
// 	{
// 		get { return _funcParams; }
// 		set { _funcParams = value; }
// 	}
//
// 	public ScriptProcessFunctionCall(string scriptLine, string funcName, List<string> funcParams) : base(scriptLine)
// 	{
// 		_funcName = funcName;
// 		_funcParams = funcParams;
// 	}
// }
//
// public class ScriptProcessBlockProcess : ScriptProcessOperation
// {
// 	private string _blockType;
// 	public string Type
// 	{
// 		get { return _blockType; }
// 		set { _blockType = value; }
// 	}
//
// 	private List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>> BlockProcesses)> _blockProcesses;
// 	public List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)> Processes
// 	{
// 		get { return _blockProcesses; }
// 		set { _blockProcesses = value; }
// 	}
//
// 	public ScriptProcessBlockProcess(string scriptLine, string blockType, List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)> blockProcesses) : base(scriptLine)
// 	{
// 		_blockType = blockType;
// 		_blockProcesses = blockProcesses;
// 	}
// }
//
// public enum ResultProcessMode
// {
// 	NORMAL,
// 	ASYNC
// }
//
// public class ScriptProcessResult
// {
// 	private int _returnCode;
// 	public int ReturnCode
// 	{
// 		get { return _returnCode; }
// 		set { _returnCode = value; }
// 	}
//
// 	private string _stdout;
// 	public string Stdout
// 	{
// 		get { return _stdout; }
// 		set { _stdout = value; }
// 	}
//
// 	private string _stderr;
// 	public string Stderr
// 	{
// 		get { return _stderr; }
// 		set { _stderr = value; }
// 	}
//
// 	public string Result
// 	{
// 		get { return GetResult(); }
// 	}
//
// 	private ResultProcessMode _resultProcessMode;
// 	public ResultProcessMode ResultProcessMode
// 	{
// 		get { return _resultProcessMode; }
// 		set { _resultProcessMode = value; }
// 	}
//
// 	public ScriptProcessResult(int returnCode, string stdout = "", string stderr = "", ResultProcessMode resultProcessMode = ResultProcessMode.NORMAL)
// 	{
// 		_returnCode = returnCode;
// 		_stdout = stdout;
// 		_stderr = stderr;
// 		_resultProcessMode = resultProcessMode;
// 	}
//
// 	public string GetResult()
// 	{
// 		if (_returnCode == 0)
// 		{
// 			return _stdout;
// 		}
// 		else
// 		{
// 			return $"err {_returnCode}: {_stderr}";
// 		}
// 	}
// }
