/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Functions
 * @created     : Thursday Nov 16, 2023 14:32:05 CST
 */

namespace GodotEGP.Scripting.Functions;

using System;
using System.Linq;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class Echo : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{

		return new ScriptProcessResult(0, (p as string[]).Join(" ").Trim());
	}
}

public partial class Return : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		i.ScriptLinerCounter = i.CurrentScriptLineCount();

		int returnCode = 0;
		if (p.Count() > 0)
		{
			returnCode = Convert.ToInt32(p[0]);
		}

		return new ScriptProcessResult(returnCode);
	}
}

public partial class Continue : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		return i.ExecuteFunctionCall("return");
	}
}
public partial class Break : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		return i.ExecuteFunctionCall("return", new string[] {"-100"});
	}
}

public partial class Seq : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		LoggerManager.LogDebug("seq", "", "p", p);

		string seqVal = "";
		if (p.Count() > 1)
		{
			int fromValue = Convert.ToInt32(p[0]);
			int toValue = Convert.ToInt32(p[1]);

			for (int ii = fromValue; ii <= toValue; ii++)
			{
				seqVal += $"{ii} ";
			}
		}

		return new ScriptProcessResult(0, seqVal.Trim());
	}
}

public partial class Source : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		LoggerManager.LogDebug("Source called");

		// created a function call as if we are calling this script directly
		string func = (string) p[0];
		p = p.Skip(1).ToArray();

		i.ChildKeepEnv = true;

		return i.ExecuteFunctionCall(func, p as string[]);
	}
}

public partial class Goto : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		LoggerManager.LogDebug("Goto called", "", "goto", p[0]);

		i.ScriptLinerCounter = Convert.ToInt32(p[0]) - 1;

		return new ScriptProcessResult(0);
	}
}

public partial class Cat : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		string result = "";

		// stdin hack
		if (p.Length == 0 && i.ScriptVars.ContainsKey("STDIN"))
		{
			result = (string) i.ScriptVars["STDIN"];
		}

		return new ScriptProcessResult(0, result);
	}
}

public partial class EvaluateExpression : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		System.Data.DataTable table = new System.Data.DataTable();
		var r = table.Compute(p[0].ToString(), null);

		LoggerManager.LogDebug("Eval test result type", "", "resType", r.GetType().Name);

		return new ScriptProcessResult(0, r.ToString(), rawResult: r);
	}
}

public partial class PrintReturnCode : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		return new ScriptProcessResult(0, i.GetVariableValue("?").ToString());
	}
}

public partial class True : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		return new ScriptProcessResult(0);
	}
}
public partial class False : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		return new ScriptProcessResult(1);
	}
}

public partial class Declare : ScriptFunction
{
	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		// basic functional implementation of declare -A
		var res = new ScriptProcessResult(0);

		if (p.Count() >= 2)
		{
			string paramType = (string) p[0];
			string varName = (string) p[1];

			if (paramType == "-A")
			{
				LoggerManager.LogDebug("Creating dictionary", "", "name", varName);
				i.ScriptVars[varName] = new Dictionary<string, object>();
			}
			else
			{
				res.ReturnCode = 1;
				res.Stderr = $"{paramType} not implemented";
			}
		}

		return res;
	}
}

public partial class CallbackAsFunction : ScriptFunction
{
	private Func<ScriptInterpretter, object[], ScriptProcessResult> _cb;

	public void SetCallbackFunction(Func<ScriptInterpretter, object[], ScriptProcessResult> cb)
	{
		_cb = cb;
	}

	public override ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		if (_cb != null)
		{
			return _cb(i, p);
		}
		else {
			return new ScriptProcessResult(0);
		}
	}
}
