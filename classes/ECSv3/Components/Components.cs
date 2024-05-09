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
public partial interface IComponent {}

// base component interface for component that has data
public partial interface IComponentData : IComponent {}

// base component interface for component that is used as a tag
public partial interface ITag : IComponent {}


/****************
*  Components  *
****************/
// component which means wildcard
public partial struct EcsWildcard : ITag {}

// component given to entities that represent components without data
public partial struct EcsTag : ITag {}

// component given to entities that represent components with data
public partial struct EcsComponent : ITag {}

// attached to component entities to give them configuration
public partial struct EcsComponentConfig : IComponentData
{
}

// attached to components or entities to mark them as disabled
public partial struct EcsDisabled : ITag {}

// query components
// component attached to all query entities
public partial struct EcsQuery : ITag {}

// indicates a query is read only for all components
public partial struct EcsReadOnlyQuery : ITag {}

// indicates a query is read and write for one or more components
public partial struct EcsReadWriteQuery : ITag {}

// indicates a query is write for one or more components
public partial struct EcsWriteQuery : ITag {}

// indicates a query does not access any components
public partial struct EcsNoAccessQuery : ITag {}

// system components
// main component attached to system entities
public partial interface IEcsSystem : ITag {}
public partial struct EcsSystem : IEcsSystem {}

// system processing phase tag interface
public partial interface IEcsProcessPhase : ITag {}

// system processing phases
public partial struct EcsProcessPhase : IEcsProcessPhase {}
public partial struct OnStartupPhase : IEcsProcessPhase {}
public partial struct PreLoadPhase : IEcsProcessPhase {}
public partial struct PreUpdatePhase : IEcsProcessPhase {}
public partial struct OnUpdatePhase : IEcsProcessPhase {}
public partial struct PostUpdatePhase : IEcsProcessPhase {}
public partial struct FinalPhase : IEcsProcessPhase {}
