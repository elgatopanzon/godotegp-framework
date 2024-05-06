/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryBuilder
 * @created     : Friday May 03, 2024 13:38:43 CST
 */

namespace GodotEGP.ECSv3.Queries;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv3;
using GodotEGP.ECSv3.Components;
using GodotEGP.ECSv3.Exceptions;

public partial class QueryBuilder
{
	private Query _query;

	public QueryBuilder()
	{
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
		if (query.Filters.Count > 0)
		{
			archetypeFilter.Filter = query.Filters[0];
		}
        
        bool isNotOnlyQuery = true;
        foreach (var filter in query.Filters.Array)
        {
        	if (filter.MatchType != FilterMatchType.Not)
        	{
        		isNotOnlyQuery = false;
        	}
        }

		LoggerManager.LogDebug("Query filters", query.GetHashCode().ToString(), "filters", query.Filters.Array);

		for (int i = 0; i < query.Filters.Count; i++)
		{
			IQueryFilter filter = query.Filters[i];

			LoggerManager.LogDebug("Query filter", query.GetHashCode().ToString(), "filter", filter);

			// if trigger end is set, we end this archetype and create a new one
			if (filter.TriggerFilterEnd)
			{
				LoggerManager.LogDebug("Query filter move next", query.GetHashCode().ToString(), "filter", filter);

				if (archetypeFilter.HasBuiltFilters)
				{
					query.ArchetypeFilters.Add(archetypeFilter);
				}

				archetypeFilter = new();
				archetypeFilter = SetQueryArchetypeFilterProperties(archetypeFilter, filter, isNotOnlyQuery);
			}

			// insert built scoped queries here
			if (filter.ScopedQuery != null)
			{
				LoggerManager.LogDebug("Scoped query found", query.GetHashCode().ToString(), "scopedQuery", filter.ScopedQuery);

				// build this filter's queries
				BuildQuery(filter.ScopedQuery);

				LoggerManager.LogDebug("Scoped query built", query.GetHashCode().ToString(), "scopedQuery", filter.ScopedQuery);
				archetypeFilter.ScopedQueries.Add(filter.ScopedQuery);
			}


			// only add the entity to the archetypes if it's not part of a
			// scoped query
			if (filter.ScopedQuery == null)
			{
				archetypeFilter.Archetypes.Add(filter.Entity);
			}
		}

		if (archetypeFilter.HasBuiltFilters)
		{
			archetypeFilter = SetQueryArchetypeFilterProperties(archetypeFilter, query.Filters[query.Filters.Count - 1], isNotOnlyQuery);

			query.ArchetypeFilters.Add(archetypeFilter);
		}

		LoggerManager.LogDebug("Archetype filters built", query.GetHashCode().ToString(), "archetypeFilters", query.ArchetypeFilters.Array);

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

	/*********************
	*  Builder methods  *
	*********************/
	
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

	// and has component or entity ID
	public QueryBuilder And(Entity entity)
	{
		_query.AddFilter(new AndQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder And(Query scopedQuery)
	{
		_query.AddFilter(new AndQueryFilter() { ScopedQuery = scopedQuery });
		return this;
	}

	// or has component or entity ID
	public QueryBuilder Or(Entity entity)
	{
		_query.AddFilter(new OrQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder Or(Query scopedQuery)
	{
		_query.AddFilter(new OrQueryFilter() { ScopedQuery = scopedQuery });
		return this;
	}

	// not have the component or entity ID
	public QueryBuilder Not(Entity entity)
	{
		_query.AddFilter(new NotQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder Not(Query scopedQuery)
	{
		_query.AddFilter(new NotQueryFilter() { ScopedQuery = scopedQuery });
		return this;
	}

	// and not have the component or entity ID
	public QueryBuilder AndNot(Entity entity)
	{
		_query.AddFilter(new AndNotQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder AndNot(Query scopedQuery)
	{
		_query.AddFilter(new AndNotQueryFilter() { ScopedQuery = scopedQuery });
		return this;
	}

	// is to match the entity to the entity in the query
	public QueryBuilder Is(Entity entity)
	{
		_query.AddFilter(new IsQueryFilter() { Entity = entity });
		return this;
	}

	public QueryBuilder IsNot(Entity entity)
	{
		_query.AddFilter(new IsNotQueryFilter() { Entity = entity });
		return this;
	}
}
