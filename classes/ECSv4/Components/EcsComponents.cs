/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Components
 * @created     : Tuesday Apr 30, 2024 13:35:20 CST
 */

namespace GodotEGP.ECSv4.Components;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;


/****************
*  Components  *
****************/
// component which means wildcard
public partial struct EcsWildcard : ITagComponent { public static int Id { get; set; } }

// component given to entities that represent components without data
public partial struct EcsTag : ITagComponent { public static int Id { get; set; } }

// component given to entities that represent components with data
public partial struct EcsComponent : ITagComponent { public static int Id { get; set; } }

// attached to component entities to give them configuration
public partial struct EcsComponentConfig : IDataComponent
{
	public static int Id { get; set; } 
}

// component given to entities that represent object instances
public partial struct EcsObject : ITagComponent { public static int Id { get; set; } }

// attached to components or entities to mark them as disabled
public partial struct EcsDisabled : ITagComponent { public static int Id { get; set; } }

// query components
// component attached to all query entities
public partial struct EcsQuery : ITagComponent { public static int Id { get; set; } }

// indicates a query is read only for all components
public partial struct EcsReadOnlyQuery : ITagComponent { public static int Id { get; set; } }

// indicates a query is read and write for one or more components
public partial struct EcsReadWriteQuery : ITagComponent { public static int Id { get; set; } }

// indicates a query is write for one or more components
public partial struct EcsWriteQuery : ITagComponent { public static int Id { get; set; } }

// indicates a query does not access any components
public partial struct EcsNoAccessQuery : ITagComponent { public static int Id { get; set; } }

// main component attached to system entities
public partial struct EcsSystem : IEcsSystemComponent { public static int Id { get; set; } }
