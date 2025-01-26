/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IComponentData
 * @created     : Saturday Jan 25, 2025 20:07:32 CST
 */

namespace GodotEGP.ECSv4.Components;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

// base component interface for component that has data
public partial interface IDataComponent : IComponent {}
