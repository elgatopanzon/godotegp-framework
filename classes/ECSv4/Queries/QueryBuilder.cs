/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryBuilder
 * @created     : Friday May 03, 2024 13:38:43 CST
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
using GodotEGP.ECSv4.Exceptions;

using System;
using System.Text.RegularExpressions;

public partial class QueryBuilder
{
	private ECS _ecs;
	private Query _query;

	public QueryBuilder(ECS ecs = null)
	{
		_ecs = ecs;
		Reset();
	}

	// reset the Query object
	public QueryBuilder Reset()
	{
		_query = new();
		return this;
	}

	// build the query object
	public Query BuildQuery(Query query)
	{
		// create the initial archetype filter
		query.ArchetypeFilters = new();
		QueryArchetypeFilter archetypeFilter = new();
        
        bool isNotOnlyQuery = false;
        int notFilterCount = 0;
        foreach (var filter in query.Filters.Span)
        {
        	if (filter.MatchType == FilterMatchType.Not)
        	{
        		notFilterCount++;
        	}
        	// increase not count for non-matcher queries
        	if (filter.Matcher == null)
        	{
        		notFilterCount++;
        	}
        }
        isNotOnlyQuery = (notFilterCount == query.Filters.Count);

		if (query.Filters.Count > 0)
		{
			archetypeFilter = SetQueryArchetypeFilterProperties(archetypeFilter, query.Filters[0], isNotOnlyQuery);
		}

		LoggerManager.LogDebug("Query filters", query.GetHashCode().ToString(), "filters", query.Filters.ArraySegment);

		for (int i = 0; i < query.Filters.Count; i++)
		{
			IQueryFilter filter = query.Filters[i];

			LoggerManager.LogDebug("Query filter", query.GetHashCode().ToString(), "filter", filter);

			// if trigger end is set, we end this archetype and create a new one
			if (filter.TriggerFilterEnd)
			{
				LoggerManager.LogDebug("Query filter move next", query.GetHashCode().ToString());

				if (archetypeFilter.HasBuiltFilters && archetypeFilter.Filter.Matcher != null)
				{
					query.ArchetypeFilters.Add(archetypeFilter);
				}

				archetypeFilter = new();
				continue;
			}
			archetypeFilter = SetQueryArchetypeFilterProperties(archetypeFilter, filter, isNotOnlyQuery);

			// insert built scoped queries here
			if (filter.ScopedQuery != null)
			{
				LoggerManager.LogDebug("Scoped query found", query.GetHashCode().ToString(), "scopedQuery", filter.ScopedQuery);

				// build this filter's queries
				BuildQuery(filter.ScopedQuery);

				// merge cached component arrays
				foreach (var typeId in filter.ScopedQuery.Results.ComponentArrayCache.Keys)
				{
					CacheComponentArray(Entity.CreateFrom(typeId));
				}

				LoggerManager.LogDebug("Scoped query built", query.GetHashCode().ToString(), "scopedQuery", filter.ScopedQuery);
				archetypeFilter.ScopedQueries.Add(filter.ScopedQuery);
			}

			if (filter.Entity != 0)
			{
				CacheComponentArray(filter.Entity);
			}

			// only add the entity to the archetypes if it's not part of a
			// scoped query
			if (filter.ScopedQuery == null && filter.Matcher != null)
			{
				archetypeFilter.Archetypes.Add(filter.Entity);
			}
		}

		if (archetypeFilter.HasBuiltFilters)
		{
			archetypeFilter = SetQueryArchetypeFilterProperties(archetypeFilter, query.Filters[query.Filters.Count - 1], isNotOnlyQuery);

			query.ArchetypeFilters.Add(archetypeFilter);
		}

		LoggerManager.LogDebug("Archetype filters built", query.GetHashCode().ToString(), "archetypeFilters", query.ArchetypeFilters.ArraySegment);

		return query;
	}

	public QueryArchetypeFilter SetQueryArchetypeFilterProperties(QueryArchetypeFilter archetypeFilter, IQueryFilter filter, bool isNotOnlyQuery)
	{
		archetypeFilter.Filter = filter;

		if (isNotOnlyQuery)
		{
			LoggerManager.LogDebug("Setting as not-only query");
			archetypeFilter.Filter.MatchMethod = FilterMatchMethod.MatchReverse;
		}

		return archetypeFilter;
	}

	public Query Build()
	{
		BuildQuery(_query);
		return _query;
	}

	public static QueryBuilder Create()
	{
		return new QueryBuilder();
	}
	public static QueryBuilder Create(ECS ecs)
	{
		return new QueryBuilder(ecs);
	}


	/**************************************
	*  ECS core-backed builder methods   *
	**************************************/
	
	// Has() methods
	public QueryBuilder Has<T>() where T : IComponent
	{
		return Has(_ecs.Id<T>());
	}


	// Not() methods
	public QueryBuilder Not<T>() where T : IComponent
	{
		return Not(_ecs.Id<T>());
	}


	// Is() methods
	public QueryBuilder Is<T>() where T : IComponent
	{
		return Is(_ecs.Id<T>());
	}


	// IsNot() methods
	public QueryBuilder IsNot<T>() where T : IComponent
	{
		return IsNot(_ecs.Id<T>());
	}


	// InAnd() methods
	public QueryBuilder InAnd<T>() where T : IComponent
	{
		PackedArray<Entity> archetype = _ecs.GetEntityArchetype(_ecs.Id<T>());
		return InAnd(archetype);
	}


	// InOr() methods
	public QueryBuilder InOr<T>() where T : IComponent
	{
		PackedArray<Entity> archetype = _ecs.GetEntityArchetype(_ecs.Id<T>());
		return InOr(archetype);
	}


	// InNot() methods
	public QueryBuilder InNot<T>() where T : IComponent
	{
		PackedArray<Entity> archetype = _ecs.GetEntityArchetype(_ecs.Id<T>());
		return InNot(archetype);
	}

	/*********************
	*  Builder methods  *
	*********************/

	// and/or query filter scope seperators
	public QueryBuilder Or()
	{
		// don't allow Or insertion on an empty query
		if (_query.Filters.Count > 0)
		{
			_query.AddFilter(new QueryFilterOrTrigger() {});
		}
		return this;
	}

	
	// has component or entity ID
	public QueryBuilder Has(Entity entity)
	{
		_query.AddFilter(new HasQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder Has(Query scopedQuery)
	{
		_query.AddFilter(new HasQueryFilter() { ScopedQuery = scopedQuery });
		return this;
	}

	// does not have the component or entity ID
	public QueryBuilder Not(Entity entity)
	{
		Or();
		_query.AddFilter(new NotQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder Not(Query scopedQuery)
	{
		Or();
		_query.AddFilter(new NotQueryFilter() { ScopedQuery = scopedQuery });
		return this;
	}

	// is a specific component or entity ID
	public QueryBuilder Is(Entity entity)
	{
		_query.AddFilter(new IsQueryFilter() { Entity = entity });
		return this;
	}
	// is not a specific component or entity ID
	public QueryBuilder IsNot(Entity entity)
	{
		Or();
		_query.AddFilter(new IsNotQueryFilter() { Entity = entity });
		return this;
	}

	// component or entity name matches given name
	public QueryBuilder NameIs(string name)
	{
		_query.AddFilter(new NameIsQueryFilter() { Name = name });
		return this;
	}
	// component or entity name matches given regex
	public QueryBuilder NameMatches(Regex regex)
	{
		_query.AddFilter(new NameMatchesQueryFilter() { Regex = regex });
		return this;
	}

	// add the archetypes as an and operation
	public QueryBuilder InAnd(PackedArray<Entity> inArchetypes)
	{
		_in(inArchetypes, Has);
		return this;
	}
	public QueryBuilder InOr(PackedArray<Entity> inArchetypes)
	{
		_in(inArchetypes, Has, true);
		return this;
	}
	public QueryBuilder InNot(PackedArray<Entity> inArchetypes)
	{
		_in(inArchetypes, Not);
		return this;
	}
	public QueryBuilder _in(PackedArray<Entity> inArchetypes, Func<Entity, QueryBuilder> action, bool isOr = false)
	{
		foreach (var entity in inArchetypes.Span)
		{
			action(entity);
			if (isOr)
			{
				Or();
			}
		}
		return this;
	}



	// access type methods to define access
	public QueryBuilder Reads(Entity entity)
	{
		_query.AddReadAccess(entity);
		return this;
	}
	public QueryBuilder Writes(Entity entity)
	{
		_query.AddWriteAccess(entity);
		return this;
	}
	public QueryBuilder ReadWrites(Entity entity)
	{
		_query.AddReadWriteAccess(entity);
		return this;
	}


	// cache an IComponentArray in the query
	public void CacheComponentArray(Entity typeId)
	{
		if (_ecs == null)
		{
			return;
		}
		_query.Results.CacheComponentArray(typeId, _ecs.GetComponentArray(typeId));
	}
}
