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

	public Query()
	{
		_filters = new();
		_archetypeFilters = new();
	}

	public void AddFilter(IQueryFilter filter)
	{
		_filters.Add(filter);
	}
}

public partial class QueryArchetypeFilter
{
	public FilterMatchType OperatorType;
	public PackedArray<Entity> Archetypes;
	public PackedArray<Query> ScopedQueries;
	public FilterMatchMethod MatchMethod { get; set; }

	public QueryArchetypeFilter()
	{
		Archetypes = new();
		ScopedQueries = new();
	}
}
