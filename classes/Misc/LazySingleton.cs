/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : LazySingleton
 * @created     : Sunday Nov 19, 2023 22:34:07 CST
 */

namespace GodotEGP.Misc;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class LazySingleton<T> where T : class, new()
{
	// Lazy singleton instance
	private static readonly Lazy<T> _instance = 
		new Lazy<T>(
			() => new T(), isThreadSafe: true
		);

	public static T Instance {
		get { return _instance.Value; }
	}
	public LazySingleton()
	{
		
	}
}
