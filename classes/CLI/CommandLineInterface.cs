/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CommandLineInterface
 * @created     : Thursday Jan 04, 2024 00:26:44 CST
 */

namespace GodotEGP.CLI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class CommandLineInterface
{
	private string[] _args { get; set; }

	public CommandLineInterface(string[] args)
	{
		_args = args;
	}

	public async Task<int> Run()
	{
		return 0;
	}
}

