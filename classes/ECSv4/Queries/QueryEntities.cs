/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryEntities
 * @created     : Sunday Jan 26, 2025 09:59:12 CST
 */

namespace GodotEGP.ECSv4.Queries;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Collections;
using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Components;
using System.Runtime.CompilerServices;

using System.Collections.Generic;

public partial record QueryEntities
{
	public HashSet<Entity> Entities { get; set; }

	public QueryEntities()
	{
		Entities = new();
	}
}
public static class QueryEntitiesExtensions
{
    public static void Add(this QueryEntities queryEntities, Entity entity)
    {
        queryEntities.Entities.Add(entity);
    }

    public static bool Remove(this QueryEntities queryEntities, Entity entity)
    {
        return queryEntities.Entities.Remove(entity);
    }

    public static bool Contains(this QueryEntities queryEntities, Entity entity)
    {
        return queryEntities.Entities.Contains(entity);
    }

    public static void Clear(this QueryEntities queryEntities)
    {
        queryEntities.Entities.Clear();
    }
}
