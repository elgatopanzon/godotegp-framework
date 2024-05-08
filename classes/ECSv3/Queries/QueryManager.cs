/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryManager
 * @created     : Tuesday May 07, 2024 12:54:52 CST
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

using System;
using System.Collections;
using System.Collections.Generic;

public partial class QueryManager
{
	private EntityManager _entityManager;

	// reference to entity names and entity archetypes
	PackedDictionary<Entity, PackedArray<Entity>> _entityArchetypes;
	PackedDictionary<string, Entity> _entityNames;

	// store query objects with result objects by their entity ID
	PackedDictionary<Entity, (Query Query, QueryResult Results)> _queries;

	// store query names mapping to query entities
	PackedDictionary<string, Entity> _queryNameMap;

	public QueryManager(EntityManager entityManager)
	{
		_entityManager = entityManager;

		_entityArchetypes = _entityManager.GetArchetypes();
		_entityNames = _entityManager.GetEntityNames();

		_queries = new();
		_queryNameMap = new();
	}

	/************************
	*  Query registration  *
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
		if (!_queries.TryGetValue(e, out (Query Query, QueryResult Results) queryTuple))
		{
			queryTuple = (query, new QueryResult());
			_queries[e] = queryTuple;

			// if we have a name, map it to the entity
			if (name.Length > 0)
			{
				_queryNameMap.Add(name, e);
				query.Name = name;
			}
		}

		return e;
	}


	/***************************
	*  Query results methods  *
	***************************/
	
	// get saved query results for query by entity id
	public QueryResult QueryResults(Entity entity)
	{
		if (_queries.TryGetValue(entity, out (Query Query, QueryResult Results) queryTuple))
		{
			return queryTuple.Results;
		}

		// if results don't exist, run the query
		return RunQuery(entity);
	}

	// get saved query results for query by name
	public QueryResult QueryResults(string name)
	{
		// try and get the entity beloning to the name, 
		if (_queryNameMap.TryGetValue(name, out Entity queryEntity))
		{
			return QueryResults(queryEntity);
		}

		throw new ArgumentException($"No query matches '{name}'.");
	}

	/*************************************
	*  Query execution & match methods  *
	*************************************/

	// run a registered query on-demand by name
	public QueryResult RunQuery(string name)
	{
		// try and get the entity beloning to the name, 
		if (_queryNameMap.TryGetValue(name, out Entity queryEntity))
		{
			return RunRegisteredQuery(_queries[queryEntity].Query, queryEntity);
		}

		throw new ArgumentException($"No query matches '{name}'.");
	}

	// run a registered query on-demand by entity id
	public QueryResult RunQuery(Entity entity)
	{
		// try and get the query object from the entity id 
		if (_queries.TryGetValue(entity, out (Query Query, QueryResult Results) queryTuple))
		{
			return RunRegisteredQuery(queryTuple.Query, entity);
		}

		throw new ArgumentException($"No query matches entity {entity}.");
	}
	
	// execute an on-demand query and return the results
	public QueryResult RunQuery(Query query)
	{
		// create a new result object
		QueryResult result = new();

		LoggerManager.LogDebug("ArchetypeFilters", query.GetHashCode().ToString(), "archetypeFilters", query.ArchetypeFilters.ArraySegment);

		// match all entities against any of the valid filter archetypes
		// loop over all entities and build a results list
		foreach (Entity entity in _entityArchetypes.Keys)
		{
			// match the entity against the query, adding to results on match
			if (_matchEntity(entity, query))
			{
				result.AddEntity(entity);
			}
		}

		return result;
	}

	// execute a registered query and store the results object
	public QueryResult RunRegisteredQuery(Query query, Entity queryEntity)
	{
		QueryResult result = RunQuery(query);
		_queries[queryEntity] = (query, result);

		return result;
	}

	// update registered query results for an entity by matching just the entity
	// through all queries and adding/removing from results
	public void UpdateQueryResults(Entity entity)
	{
		LoggerManager.LogDebug("Updating query results for entity", "", "entity", entity);

		// loop over all queries and match with the provided entity
		Span<Entity> queriesKeys = _queries.Keys;
		Span<(Query Query, QueryResult Results)> queriesValues = _queries.Values;
		int length = queriesKeys.Length;
		for (int i = 0; i < length; i++)
		{
			var queryTuple = queriesValues[i];
			bool existsInResults = queryTuple.Results.Entities.Contains(entity);

			bool match = _matchEntity(entity, queryTuple.Query);

			// if it's not a match, attempt to remove from results
			if (existsInResults && !match)
			{
				LoggerManager.LogDebug("Removing entity from query results", queryTuple.Query.Name, "entity", entity);
				queryTuple.Results.Entities.Remove(entity);
			}

			// otherwise, attempt to add if it doesn't exist
			else if (!existsInResults && match)
			{
				LoggerManager.LogDebug("Adding entity to query results", queryTuple.Query.Name, "entity", entity);
				queryTuple.Results.Entities.Add(entity);
			}
		}
	}

	public bool _matchEntity(Entity entity, Query query)
	{
		if (_entityArchetypes.TryGetValue(entity, out PackedArray<Entity> entitiesArchetypes))
		{
			// for the sake of this on-demand query it's better performance
			// to stop processing empty entities
			if (entitiesArchetypes.Count == 0)
			{
				return false;
			}

			LoggerManager.LogDebug($"Matching {entity.ToString()}", query.GetHashCode().ToString(), "entitiesArchetypes", entitiesArchetypes.ArraySegment);

			int matchCount = 0;
			foreach (var filter in query.ArchetypeFilters.Span)
			{
				LoggerManager.LogDebug("Matching against filter", query.GetHashCode().ToString(), "filter", filter);

				bool matched = _matchFilter(entity, query, filter, entitiesArchetypes, out bool nonMatchingEntity);

				if (matched)
				{
					LoggerManager.LogDebug($"Filter matched type {entity.ToString()}", query.GetHashCode().ToString(), "matcher", filter.Filter.Matcher.GetType().Name);

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

	public bool _matchFilter(Entity entity, Query query, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, out bool nonMatchingEntity)
	{
		// match based on the operator type
		bool matched = false;
		nonMatchingEntity = false;

		// do an archetype comparison if there's no scoped query
		if (filter.ScopedQueries.Count == 0)
		{
			LoggerManager.LogDebug($"Matching against archetype filter {entity.ToString()}", query.GetHashCode().ToString(), "filter", filter);

			matched = filter.Filter.Matcher.PreMatch(entity, filter, entitiesArchetypes, _entityArchetypes, _entityNames, out bool nonMatchingEntityPre);
		}
		// recursively check matches with scoped queries
		else
		{
			matched = _matchScopedQueries(entity, filter.ScopedQueries, query);
		}
		
		matched = filter.Filter.Matcher.PostMatch(entity, filter, entitiesArchetypes, _entityArchetypes, _entityNames, matched, out bool nonMatchingEntityPost);

		nonMatchingEntity = nonMatchingEntityPost;

		if (matched)
		{
			LoggerManager.LogDebug($"Matched {filter.Filter.Matcher.GetType().Name} filter {entity.ToString()}", query.GetHashCode().ToString(), "filterArchetypes", filter.Archetypes.ArraySegment);
			LoggerManager.LogDebug($"Matched {filter.Filter.Matcher.GetType().Name} filter {entity.ToString()}", query.GetHashCode().ToString(), "entityArchetypes", entitiesArchetypes.ArraySegment);
		}

		LoggerManager.LogDebug("Filter match result", "", "matched", matched);

		return matched;
	}

	public bool _matchScopedQueries(Entity entity, PackedArray<Query> scopedQueries, Query query)
	{
		LoggerManager.LogDebug($"Matching against scoped query {entity.ToString()}", query.GetHashCode().ToString(), "scopedQuery", scopedQueries);

		int matchCount = 0;
		bool match = false;
		foreach (var q in scopedQueries.Span)
		{
			match = _matchEntity(entity, q);

			if (match)
			{
				matchCount++;
			}
		}

		LoggerManager.LogDebug("Scoped query match result", query.GetHashCode().ToString(), matchCount.ToString(), matchCount);

		// ensure all the scoped queries match
		return (matchCount == scopedQueries.Count);
	}
}

