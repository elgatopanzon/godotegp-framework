/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IScriptFunction
 * @created     : Thursday Nov 16, 2023 14:07:51 CST
 */

namespace GodotEGP.Scripting.Functions;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public interface IScriptFunction
{
	ScriptProcessResult Call(ScriptInterpretter i, params object[] p);
}

public  class ScriptFunction : IScriptFunction
{
	public virtual ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		return new ScriptProcessResult(0);
	}
}
