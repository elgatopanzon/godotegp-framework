/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Archetype
 * @created     : Saturday Apr 27, 2024 20:54:06 CST
 */

namespace GodotEGP.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Collections.Generic;
using System.Linq;

using GodotEGP.ECSv4.Components;

public partial record Archetype
{
	public HashSet<Entity> Entities { get; set; }

	public Archetype()
	{
		Entities = new();
	}
}
public static class ArchetypeExtensions
{
    public static void Add(this Archetype archetype, Entity entity)
    {
        archetype.Entities.Add(entity);
    }

    public static void Remove(this Archetype archetype, Entity entity)
    {
        archetype.Entities.Remove(entity);
    }

    public static bool Contains(this Archetype archetype, Entity entity)
    {
        return archetype.Entities.Contains(entity);
    }

    public static IEnumerable<Entity> Intersect(this Archetype archetype, Archetype archetypeB)
    {
        return archetype.Entities.Intersect(archetypeB.Entities);
    }

    public static int IntersectCount(this Archetype archetype, Archetype archetypeB)
    {
        return archetype.Intersect(archetypeB).Count();
    }
}
