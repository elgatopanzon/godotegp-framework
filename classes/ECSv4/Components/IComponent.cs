/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IComponent
 * @created     : Saturday Jan 25, 2025 20:06:56 CST
 */

namespace GodotEGP.ECSv4.Components;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

// base component interface for a Component
public partial interface IComponent 
{
	public static abstract int Id { get; set; }
}

