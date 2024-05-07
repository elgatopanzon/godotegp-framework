/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Query
 * @created     : Friday May 03, 2024 13:54:13 CST
 */

namespace GodotEGP.ECSv3.Queries;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Collections;
using GodotEGP.ECSv3;

public partial class Query
{
	// stores raw filter objects with matched entity and type
	private PackedArray<IQueryFilter> _filters;
	public PackedArray<IQueryFilter> Filters
	{
		get {
			return _filters;
		}
	}

	// stores unwrapped filters as archetype lists and operator type
	private PackedArray<QueryArchetypeFilter> _archetypeFilters;
	public PackedArray<QueryArchetypeFilter> ArchetypeFilters
	{
		get {
			return _archetypeFilters;
		}
		set {
			_archetypeFilters = value;
		}
	}

	// an archetype for read and write access
	private PackedArray<Entity> _readArchetype;
	private PackedArray<Entity> _writeArchetype;

	public PackedArray<Entity> ReadsEntities
	{
		get {
			return _readArchetype;
		}
	}
	public PackedArray<Entity> WritesEntities
	{
		get {
			return _writeArchetype;
		}
	}

	public Query()
	{
		_filters = new();
		_archetypeFilters = new();

		_readArchetype = new();
		_writeArchetype = new();
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
	public PackedArray<Entity> Archetypes;
	public PackedArray<Query> ScopedQueries;
	public IQueryFilter Filter { get; set; }

	public bool HasBuiltFilters
	{
		get {
			return (Archetypes.Count > 0 || ScopedQueries.Count > 0);
		}
	}

	public QueryArchetypeFilter()
	{
		Archetypes = new();
		ScopedQueries = new();
	}
}
