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
	public QueryBuilder Has<T, TT>() 
		where T : IComponent
		where TT : IComponent
	{
		return Has(_ecs.Id<T, TT>());
	}
	public QueryBuilder Has<T>(Entity entity) where T : IComponent
	{
		return Has(_ecs.Id<T>(entity));
	}
	public QueryBuilder Has(Entity sourceEntity, Entity targetEntity)
	{
		return Has(_ecs.Id(sourceEntity, targetEntity));
	}


	// Not() methods
	public QueryBuilder Not<T>() where T : IComponent
	{
		return Not(_ecs.Id<T>());
	}
	public QueryBuilder Not<T, TT>() 
		where T : IComponent
		where TT : IComponent
	{
		return Not(_ecs.Id<T, TT>());
	}
	public QueryBuilder Not<T>(Entity entity) where T : IComponent
	{
		return Not(_ecs.Id<T>(entity));
	}
	public QueryBuilder Not(Entity sourceEntity, Entity targetEntity)
	{
		return Not(_ecs.Id(sourceEntity, targetEntity));
	}


	// Is() methods
	public QueryBuilder Is<T>() where T : IComponent
	{
		return Is(_ecs.Id<T>());
	}
	public QueryBuilder Is<T, TT>() 
		where T : IComponent
		where TT : IComponent
	{
		return Is(_ecs.Id<T, TT>());
	}
	public QueryBuilder Is<T>(Entity entity) where T : IComponent
	{
		return Is(_ecs.Id<T>(entity));
	}
	public QueryBuilder Is(Entity sourceEntity, Entity targetEntity)
	{
		return Is(_ecs.Id(sourceEntity, targetEntity));
	}


	// IsNot() methods
	public QueryBuilder IsNot<T>() where T : IComponent
	{
		return IsNot(_ecs.Id<T>());
	}
	public QueryBuilder IsNot<T, TT>() 
		where T : IComponent
		where TT : IComponent
	{
		return IsNot(_ecs.Id<T, TT>());
	}
	public QueryBuilder IsNot<T>(Entity entity) where T : IComponent
	{
		return IsNot(_ecs.Id<T>(entity));
	}
	public QueryBuilder IsNot(Entity sourceEntity, Entity targetEntity)
	{
		return IsNot(_ecs.Id(sourceEntity, targetEntity));
	}


	// InAnd() methods
	public QueryBuilder InAnd<T>() where T : IComponent
	{
		return InAnd(_ecs.GetEntityArchetype(_ecs.Id<T>()));
	}
	public QueryBuilder InAnd<T, TT>() 
		where T : IComponent
		where TT : IComponent
	{
		return InAnd(_ecs.GetEntityArchetype(_ecs.Id<T, TT>()));
	}
	public QueryBuilder InAnd<T>(Entity entity) where T : IComponent
	{
		return InAnd(_ecs.GetEntityArchetype(_ecs.Id<T>(entity)));
	}
	public QueryBuilder InAnd(Entity sourceEntity, Entity targetEntity)
	{
		return InAnd(_ecs.GetEntityArchetype(_ecs.Id(sourceEntity, targetEntity)));
	}


	// InOr() methods
	public QueryBuilder InOr<T>() where T : IComponent
	{
		return InOr(_ecs.GetEntityArchetype(_ecs.Id<T>()));
	}
	public QueryBuilder InOr<T, TT>() 
		where T : IComponent
		where TT : IComponent
	{
		return InOr(_ecs.GetEntityArchetype(_ecs.Id<T, TT>()));
	}
	public QueryBuilder InOr<T>(Entity entity) where T : IComponent
	{
		return InOr(_ecs.GetEntityArchetype(_ecs.Id<T>(entity)));
	}
	public QueryBuilder InOr(Entity sourceEntity, Entity targetEntity)
	{
		return InOr(_ecs.GetEntityArchetype(_ecs.Id(sourceEntity, targetEntity)));
	}


	// InNot() methods
	public QueryBuilder InNot<T>() where T : IComponent
	{
		return InNot(_ecs.GetEntityArchetype(_ecs.Id<T>()));
	}
	public QueryBuilder InNot<T, TT>() 
		where T : IComponent
		where TT : IComponent
	{
		return InNot(_ecs.GetEntityArchetype(_ecs.Id<T, TT>()));
	}
	public QueryBuilder InNot<T>(Entity entity) where T : IComponent
	{
		return InNot(_ecs.GetEntityArchetype(_ecs.Id<T>(entity)));
	}
	public QueryBuilder InNot(Entity sourceEntity, Entity targetEntity)
	{
		return InNot(_ecs.GetEntityArchetype(_ecs.Id(sourceEntity, targetEntity)));
	}


	// HasPairSource() methods
	public QueryBuilder HasPairSource<TSource>() 
		where TSource : IComponent
	{
		return HasPairSource(_ecs.Id<TSource>().Id);
	}
	// HasPairTarget() methods
	public QueryBuilder HasPairTarget<TTarget>() 
		where TTarget : IComponent
	{
		return HasPairTarget(_ecs.Id<TTarget>().Id);
	}
	// HasPair() methods
	public QueryBuilder HasPair<TSource, TTarget>() 
		where TSource : IComponent
		where TTarget : IComponent
	{
		return HasPair(_ecs.Id<TSource>().Id, _ecs.Id<TTarget>().Id);
	}


	// NotPairSource() methods
	public QueryBuilder NotPairSource<TSource>() 
		where TSource : IComponent
	{
		return NotPairSource(_ecs.Id<TSource>().Id);
	}
	// HasPairTarget() methods
	public QueryBuilder NotPairTarget<TTarget>() 
		where TTarget : IComponent
	{
		return NotPairTarget(_ecs.Id<TTarget>().Id);
	}
	// HasPair() methods
	public QueryBuilder NotPair<TSource, TTarget>() 
		where TSource : IComponent
		where TTarget : IComponent
	{
		return NotPair(_ecs.Id<TSource>().Id, _ecs.Id<TTarget>().Id);
	}


	// PairTargetHas() methods
	public QueryBuilder PairTargetHas<TSource, TTarget, THas>()
		where TSource : IComponent
		where TTarget : IComponent
		where THas : IComponent
	{
		return PairTargetHas(_ecs.Id<TSource>(), _ecs.Id<TTarget>(), _ecs.Id<THas>());
	}
	// PairSourceHas() methods
	public QueryBuilder PairSourceHas<TSource, TTarget, THas>()
		where TSource : IComponent
		where TTarget : IComponent
		where THas : IComponent
	{
		return PairSourceHas(_ecs.Id<TSource>(), _ecs.Id<TTarget>(), _ecs.Id<THas>());
	}

	// NotPairTargetHas() methods
	public QueryBuilder NotPairTargetHas<TSource, TTarget, THas>()
		where TSource : IComponent
		where TTarget : IComponent
		where THas : IComponent
	{
		return NotPairTargetHas(_ecs.Id<TSource>(), _ecs.Id<TTarget>(), _ecs.Id<THas>());
	}
	// NotPairSourceHas() methods
	public QueryBuilder NotPairSourceHas<TSource, TTarget, THas>()
		where TSource : IComponent
		where TTarget : IComponent
		where THas : IComponent
	{
		return NotPairSourceHas(_ecs.Id<TSource>(), _ecs.Id<TTarget>(), _ecs.Id<THas>());
	}

	// PairOwnerHas() methods
	public QueryBuilder PairOwnerHas<TSource, TTarget, THas>()
		where TSource : IComponent
		where TTarget : IComponent
		where THas : IComponent
	{
		return PairOwnerHas(_ecs.Id<TSource>(), _ecs.Id<TTarget>(), _ecs.Id<THas>());
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
				Entity = targetHasEntity,
			});
		return this;
	}
	// pair source has entity id
	public QueryBuilder PairSourceHas(Entity sourceEntity, Entity targetEntity, Entity sourceHasEntity)
	{
		_query.AddFilter(new PairSourceHasQueryFilter() { 
				SourceEntity = Entity.CreateFrom(sourceEntity.Id, 0),
				TargetEntity = Entity.CreateFrom(targetEntity.Id, 0),
				Entity = sourceHasEntity,
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
