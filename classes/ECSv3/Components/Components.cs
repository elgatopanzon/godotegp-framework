/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Components
 * @created     : Tuesday Apr 30, 2024 13:35:20 CST
 */

namespace GodotEGP.ECSv3.Components;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

// base component interface for a Component
public partial interface IComponent 
{
	public static abstract int Id { get; set; }
}

// base component interface for component that has data
public partial interface IComponentData : IComponent {}

// base component interface for component that is used as a tag
public partial interface ITag : IComponent {}


/****************
*  Components  *
****************/
// component which means wildcard
public partial struct EcsWildcard : ITag { public static int Id { get; set; } }

// component given to entities that represent components without data
public partial struct EcsTag : ITag { public static int Id { get; set; } }

// component given to entities that represent components with data
public partial struct EcsComponent : ITag { public static int Id { get; set; } }

// attached to component entities to give them configuration
public partial struct EcsComponentConfig : IComponentData
{
	public static int Id { get; set; } 
}

// attached to components or entities to mark them as disabled
public partial struct EcsDisabled : ITag { public static int Id { get; set; } }

// query components
// component attached to all query entities
public partial struct EcsQuery : ITag { public static int Id { get; set; } }

// indicates a query is read only for all components
public partial struct EcsReadOnlyQuery : ITag { public static int Id { get; set; } }

// indicates a query is read and write for one or more components
public partial struct EcsReadWriteQuery : ITag { public static int Id { get; set; } }

// indicates a query is write for one or more components
public partial struct EcsWriteQuery : ITag { public static int Id { get; set; } }

// indicates a query does not access any components
public partial struct EcsNoAccessQuery : ITag { public static int Id { get; set; } }

// system components
// main component attached to system entities
public partial interface IEcsSystem : ITag {}
public partial struct EcsSystem : IEcsSystem { public static int Id { get; set; } }

// system processing phase tag interface
public partial interface IEcsProcessPhase : ITag {}

// system processing phases
public partial struct EcsProcessPhase : IEcsProcessPhase { public static int Id { get; set; } }
public partial struct OnStartupPhase : IEcsProcessPhase { public static int Id { get; set; } }
public partial struct PreLoadPhase : IEcsProcessPhase { public static int Id { get; set; } }
public partial struct PreUpdatePhase : IEcsProcessPhase { public static int Id { get; set; } }
public partial struct OnUpdatePhase : IEcsProcessPhase { public static int Id { get; set; } }
public partial struct PostUpdatePhase : IEcsProcessPhase { public static int Id { get; set; } }
public partial struct FinalPhase : IEcsProcessPhase { public static int Id { get; set; } }
