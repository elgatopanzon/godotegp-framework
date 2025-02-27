/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryManager
 * @created     : Tuesday May 07, 2024 12:54:52 CST
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

using System;
using System.Collections;
using System.Collections.Generic;

public partial class QueryManager
{
	private EntityManager _entityManager;

	// reference to entity names and entity archetypes
	Dictionary<Entity, Archetype> _entityArchetypes;
	Dictionary<string, Entity> _entityNames;

	// store query objects with result objects by their entity ID
	Dictionary<int, Query> _queries;

	// store query names mapping to query entities
	Dictionary<string, Entity> _queryNameMap;

	public QueryManager(EntityManager entityManager)
	{
		_entityManager = entityManager;

		_entityArchetypes = _entityManager.GetArchetypes();
		_entityNames = _entityManager.GetEntityNames();

		_queries = new();
		_queryNameMap = new();
	}

	/************************
	*  Query management    *
	************************/
	
	public Entity RegisterQuery(Query query)
	{
		return RegisterQuery(query, "");
	}
	public Entity RegisterQuery(Query query, string name)
	{
		// create entity or obtain existing one by name
		Entity e = _entityManager.Create(name);

		// store the query object if it doesn't exist
		if (!_queries.TryGetValue(e, out Query queryExisting))
		{
			_queries[e] = query;

			// if we have a name, map it to the entity
			if (name.Length > 0)
			{
				_queryNameMap.Add(name, e);
				query.Name = name;
			}
		}

		return e;
	}

	public Query GetQuery(Entity entity)
	{
		if (_queries.TryGetValue(entity, out Query query))
		{
			return query;
		}

		throw new ArgumentException($"No query matches '{entity}'.");
	}
	public Query GetQuery(string name)
	{
		return GetQuery(GetQueryEntity(name));
	}

	public string GetQueryName(Entity entity)
	{
		return GetQuery(entity).Name;
	}
	public Entity GetQueryEntity(string name)
	{
		if (_queryNameMap.TryGetValue(name, out Entity queryEntity))
		{
			return queryEntity;
		}

		throw new ArgumentException($"No query matches '{name}'.");
	}

	/***************************
	*  Query results methods  *
	***************************/
	
	// get saved query results for query by entity id
	public QueryEntities QueryEntities(Entity entity)
	{
		if (_queries.TryGetValue(entity, out Query query))
		{
			return query.Entities;
		}

		// if results don't exist, run the query
		return RunQuery(entity);
	}

	// get saved query results for query by name
	public QueryEntities QueryEntities(string name)
	{
		return QueryEntities(GetQueryEntity(name));
	}

	/*************************************
	*  Query execution & match methods  *
	*************************************/

	// run a registered query on-demand by name
	public QueryEntities RunQuery(string name)
	{
		return RunRegisteredQuery(GetQuery(name), GetQueryEntity(name));
	}

	// run a registered query on-demand by entity id
	public QueryEntities RunQuery(Entity entity)
	{
		return RunRegisteredQuery(GetQuery(entity), entity);
	}
	
	// execute an on-demand query and return the results
	public QueryEntities RunQuery(Query query)
	{
		// create a new result object
		query.Entities.Clear();

		// LoggerManager.LogDebug("ArchetypeFilters", query.GetHashCode().ToString(), "archetypeFilters", query.ArchetypeFilters.ArraySegment);

		// match all entities against any of the valid filter archetypes
		// loop over all entities and build a results list
		foreach (Entity entity in _entityArchetypes.Keys)
		{
			// match the entity against the query, adding to results on match
			if (_matchEntity(entity, query))
			{
				query.Entities.Add(entity);
			}
		}

		return query.Entities;
	}

	// execute a registered query and store the results object
	public QueryEntities RunRegisteredQuery(Query query, Entity queryEntity)
	{
		return RunQuery(query);
	}

	// update registered query results for an entity by matching just the entity
	// through all queries and adding/removing from results
	public void UpdateQueryResults(Entity entity)
	{
		// LoggerManager.LogDebug("Updating query results for entity", "", "entity", entity);

		// loop over all queries and match with the provided entity
		foreach (var query in _queries.Values)
		{
			// skip non-live queries
			if (!query.IsLiveQuery)
			{
				continue;
			}

			bool existsInResults = query.Entities.Contains(entity);

			bool match = _matchEntity(entity, query);

			// if it's not a match, attempt to remove from results
			if (existsInResults && !match)
			{
				// LoggerManager.LogDebug("Removing entity from query results", query.Name, "entity", entity);
				query.Entities.Remove(entity);
			}

			// otherwise, attempt to add if it doesn't exist
			else if (!existsInResults && match)
			{
				// LoggerManager.LogDebug("Adding entity to query results", query.Name, "entity", entity);
				query.Entities.Add(entity);
			}
		}
	}

	public bool _matchEntity(Entity entity, Query query)
	{
		if (_entityArchetypes.TryGetValue(entity, out Archetype entitiesArchetype))
		{
			// for the sake of this on-demand query it's better performance
			// to stop processing empty entities
			if (entitiesArchetype.Entities.Count == 0)
			{
				return false;
			}

			// LoggerManager.LogDebug($"Matching {entity.ToString()}", query.GetHashCode().ToString(), "entitiesArchetypes", entitiesArchetypes.ArraySegment);

			int matchCount = 0;
			foreach (var filter in query.ArchetypeFilters)
			{
				// LoggerManager.LogDebug("Matching against filter", query.GetHashCode().ToString(), "filter", filter);

				bool matched = _matchFilter(entity, query, filter, entitiesArchetype, out bool nonMatchingEntity);

				if (matched)
				{
					// LoggerManager.LogDebug($"Filter matched type {entity.ToString()}", query.GetHashCode().ToString(), "matcher", filter.Filter.Matcher.GetType().Name);

					matchCount++;
				}

				// if the entity is a non-matching entity, stop the query
				if (nonMatchingEntity)
				{
					return false;
				}
			}

			// it's considered a match if we match at least 1 filter
			return (matchCount > 0);
		}

		return false;
	}

	public bool _matchFilter(Entity entity, Query query, QueryArchetypeFilter filter, Archetype entitiesArchetype, out bool nonMatchingEntity)
	{
		// match based on the operator type
		bool matched = false;
		nonMatchingEntity = false;

		// do an archetype comparison if there's no scoped query
		if (filter.ScopedQueries.Count == 0)
		{
			// LoggerManager.LogDebug($"Matching against archetype filter {entity.ToString()}", query.GetHashCode().ToString(), "filter", filter);

			matched = filter.Filter.Matcher.PreMatch(entity, filter, entitiesArchetype, _entityArchetypes, _entityNames, out bool nonMatchingEntityPre);
		}
		// recursively check matches with scoped queries
		else
		{
			matched = _matchScopedQueries(entity, filter.ScopedQueries, query);
		}
		
		matched = filter.Filter.Matcher.PostMatch(entity, filter, entitiesArchetype, _entityArchetypes, _entityNames, matched, out bool nonMatchingEntityPost);

		nonMatchingEntity = nonMatchingEntityPost;

		if (matched)
		{
			// LoggerManager.LogDebug($"Matched {filter.Filter.Matcher.GetType().Name} filter {entity.ToString()}", query.GetHashCode().ToString(), "filterArchetypes", filter.Archetypes.ArraySegment);
			// LoggerManager.LogDebug($"Matched {filter.Filter.Matcher.GetType().Name} filter {entity.ToString()}", query.GetHashCode().ToString(), "entityArchetypes", entitiesArchetypes.ArraySegment);
		}

		// LoggerManager.LogDebug("Filter match result", "", "matched", matched);

		return matched;
	}

	public bool _matchScopedQueries(Entity entity, List<Query> scopedQueries, Query query)
	{
		// LoggerManager.LogDebug($"Matching against scoped query {entity.ToString()}", query.GetHashCode().ToString(), "scopedQuery", scopedQueries);

		int matchCount = 0;
		bool match = false;
		foreach (var q in scopedQueries)
		{
			match = _matchEntity(entity, q);

			if (match)
			{
				matchCount++;
			}
		}

		// LoggerManager.LogDebug("Scoped query match result", query.GetHashCode().ToString(), matchCount.ToString(), matchCount);

		// ensure all the scoped queries match
		return (matchCount == scopedQueries.Count);
	}
}

