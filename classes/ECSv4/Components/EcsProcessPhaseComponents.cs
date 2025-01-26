/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EcsProcessPhaseComponents
 * @created     : Saturday Jan 25, 2025 20:12:27 CST
 */

namespace GodotEGP.ECSv4.Components;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

// system processing phases
public partial struct EcsProcessPhase : IEcsProcessPhaseComponent { public static int Id { get; set; } }
public partial struct OnStartupPhase : IEcsProcessPhaseComponent { public static int Id { get; set; } }
public partial struct PreLoadPhase : IEcsProcessPhaseComponent { public static int Id { get; set; } }
public partial struct PreUpdatePhase : IEcsProcessPhaseComponent { public static int Id { get; set; } }
public partial struct OnUpdatePhase : IEcsProcessPhaseComponent { public static int Id { get; set; } }
public partial struct PostUpdatePhase : IEcsProcessPhaseComponent { public static int Id { get; set; } }
public partial struct FinalPhase : IEcsProcessPhaseComponent { public static int Id { get; set; } }
