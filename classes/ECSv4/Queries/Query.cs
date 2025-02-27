/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Query
 * @created     : Friday May 03, 2024 13:54:13 CST
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

public partial class Query
{
	// stores raw filter objects with matched entity and type
	private List<IQueryFilter> _filters;
	public List<IQueryFilter> Filters
	{
		get {
			return _filters;
		}
	}

	// stores unwrapped filters as archetype lists and operator type
	private List<QueryArchetypeFilter> _archetypeFilters;
	public List<QueryArchetypeFilter> ArchetypeFilters
	{
		get {
			return _archetypeFilters;
		}
		set {
			_archetypeFilters = value;
		}
	}

	// an archetype for read and write access
	private Archetype _readArchetype;
	private Archetype _writeArchetype;

	public Archetype ReadsEntities
	{
		get {
			return _readArchetype;
		}
	}
	public Archetype WritesEntities
	{
		get {
			return _writeArchetype;
		}
	}

	private QueryEntities _entities;
	public QueryEntities Entities
	{
		get { return _entities; }
		set { _entities = value; }
	}

	public string Name { get; set; }

	// a live query is included in the auto update results on entity changes
	public bool IsLiveQuery { get; set; } = true;

	public Query()
	{
		_filters = new();
		_archetypeFilters = new();

		_readArchetype = new();
		_writeArchetype = new();

		_entities = new();
	}

	public void AddFilter(IQueryFilter filter)
	{
		_filters.Add(filter);
	}

	public void AddReadAccess(Entity entity)
	{
		_readArchetype.Add(entity);
	}
	public void AddWriteAccess(Entity entity)
	{
		_writeArchetype.Add(entity);
	}
	public void AddReadWriteAccess(Entity entity)
	{
		AddReadAccess(entity);
		AddWriteAccess(entity);
	}
}

public partial class QueryArchetypeFilter
{
	public Archetype Archetype;
	public List<Query> ScopedQueries;
	public IQueryFilter Filter { get; set; }

	public bool HasBuiltFilters
	{
		get {
			return (Archetype.Entities.Count > 0 || ScopedQueries.Count > 0);
		}
	}

	public QueryArchetypeFilter()
	{
		Archetype = new();
		ScopedQueries = new();
	}
}
