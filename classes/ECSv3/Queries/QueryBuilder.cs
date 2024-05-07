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
using GodotEGP.Collections;

using GodotEGP.ECSv3;
using GodotEGP.ECSv3.Components;
using GodotEGP.ECSv3.Exceptions;

using System;
using System.Text.RegularExpressions;

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
        
        bool isNotOnlyQuery = false;
        int notFilterCount = 0;
        foreach (var filter in query.Filters.Array)
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

		LoggerManager.LogDebug("Query filters", query.GetHashCode().ToString(), "filters", query.Filters.Array);

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

				LoggerManager.LogDebug("Scoped query built", query.GetHashCode().ToString(), "scopedQuery", filter.ScopedQuery);
				archetypeFilter.ScopedQueries.Add(filter.ScopedQuery);
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
		foreach (var entity in inArchetypes.Array)
		{
			action(entity);
			if (isOr)
			{
				Or();
			}
		}
		return this;
	}

	// has pair source entity id
	public QueryBuilder HasPairSource(Entity sourceEntity)
	{
		_query.AddFilter(new HasPairQueryFilter() { SourceEntity = Entity.CreateFrom(sourceEntity.Id, 0) });
		return this;
	}
	// has pair target entity id
	public QueryBuilder HasPairTarget(Entity targetEntity)
	{
		_query.AddFilter(new HasPairQueryFilter() { TargetEntity = Entity.CreateFrom(targetEntity.Id, 0) });
		return this;
	}
	// has pair (basically match the full archetype, it's the same as Has())
	public QueryBuilder HasPair(Entity sourceEntity, Entity targetEntity)
	{
		// create an entity from the 2 pair Ids
		Has(Entity.CreateFrom(sourceEntity.Id, targetEntity.Id));
		return this;
	}

	// not pair source entity id
	public QueryBuilder NotPairSource(Entity sourceEntity)
	{
		_query.AddFilter(new NotHasPairQueryFilter() { SourceEntity = Entity.CreateFrom(sourceEntity.Id, 0) });
		return this;
	}
	// not pair target entity id
	public QueryBuilder NotPairTarget(Entity targetEntity)
	{
		_query.AddFilter(new NotHasPairQueryFilter() { TargetEntity = Entity.CreateFrom(targetEntity.Id, 0) });
		return this;
	}
	// not pair (basically match the full archetype, it's the same as Has())
	public QueryBuilder NotPair(Entity sourceEntity, Entity targetEntity)
	{
		// create an entity from the 2 pair Ids
		Not(Entity.CreateFrom(sourceEntity.Id, targetEntity.Id));
		return this;
	}


	// pair target has entity id
	public QueryBuilder PairTargetHas(Entity sourceEntity, Entity targetEntity, Entity targetHasEntity)
	{
		_query.AddFilter(new PairTargetHasQueryFilter() { 
				SourceEntity = Entity.CreateFrom(sourceEntity.Id, 0),
				TargetEntity = Entity.CreateFrom(targetEntity.Id, 0),
				Entity = Entity.CreateFrom(targetHasEntity.Id, 0),
			});
		return this;
	}
	// pair source has entity id
	public QueryBuilder PairSourceHas(Entity sourceEntity, Entity targetEntity, Entity sourceHasEntity)
	{
		_query.AddFilter(new PairSourceHasQueryFilter() { 
				SourceEntity = Entity.CreateFrom(sourceEntity.Id, 0),
				TargetEntity = Entity.CreateFrom(targetEntity.Id, 0),
				Entity = Entity.CreateFrom(sourceHasEntity.Id, 0),
			});
		return this;
	}

	public QueryBuilder NotPairTargetHas(Entity sourceEntity, Entity targetEntity, Entity targetHasEntity)
	{
		Not(Create().PairTargetHas(sourceEntity, targetEntity, targetHasEntity).Build());
		return this;
	}
	public QueryBuilder NotPairSourceHas(Entity sourceEntity, Entity targetEntity, Entity sourceHasEntity)
	{
		Not(Create().PairSourceHas(sourceEntity, targetEntity, sourceHasEntity).Build());
		return this;
	}

	public QueryBuilder PairOwnerHas(Entity sourceEntity, Entity targetEntity, Entity ownerHasEntity)
	{
		if (sourceEntity.Id == 0)
		{
			Has(Create().HasPairTarget(targetEntity).Build());
		}
		else if (targetEntity.Id == 0)
		{
			Has(Create().HasPairSource(sourceEntity).Build());
		}
		else
		{
			Has(Create().HasPair(sourceEntity, targetEntity).Build());
		}
		Has(Create().Has(ownerHasEntity).Build());
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
}
