/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ScriptResource
 * @created     : Wednesday Nov 15, 2023 16:17:40 CST
 */

namespace GodotEGP.Resource.Resources;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class GameScript : Godot.Resource
{
	private string _scriptContent;
	public string ScriptContent
	{
		get { return _scriptContent; }
		set { _scriptContent = value; }
	}
	public GameScript()
	{
		
	}
}

