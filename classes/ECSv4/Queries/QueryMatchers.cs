/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryMatchers
 * @created     : Sunday May 05, 2024 23:44:40 CST
 */

namespace GodotEGP.ECSv4.Queries;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using System;
using System.Collections.Generic;
using System.Linq;

public partial class QueryMatchPassthrough : IQueryMatcher
{
	public virtual bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, Archetype entitiesArchetype, Dictionary<Entity, Archetype> entityArchetypes, Dictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		return true;
	}

	// default post-match is to pass through the pre match result
	public virtual bool PostMatch(Entity matchEntity, QueryArchetypeFilter filter, Archetype entitiesArchetype, Dictionary<Entity, Archetype> entityArchetypes, Dictionary<string, Entity> entityNames, bool preMatched, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		return preMatched;
	}
}

public partial class QueryMatchArchetype : QueryMatchPassthrough
{
	// match the filter's achetypes with the provided entity archetype using an
	// intersect
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, Archetype entitiesArchetype, Dictionary<Entity, Archetype> entityArchetypes, Dictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;

		int matchCount = filter.Archetype.IntersectCount(entitiesArchetype);
		bool matched = (matchCount == filter.Archetype.Entities.Count);

		// match a wildcard by making sure the entity has >= archetype count,
		// and the archetype match was just 1 less than the filter archetype
		// count
		if (filter.Filter.Entity == 0 && matchCount == (filter.Archetype.Entities.Count - 1) && entitiesArchetype.Entities.Count >= filter.Archetype.Entities.Count)
		{
			matched = true;
		}

		// LoggerManager.LogDebug("PreMatch", "", matchEntity.ToString(), matched);

		return matched;
	}
}

public partial class QueryMatchEntity : QueryMatchArchetype
{
	// match the provided entity ID with the filter's entity ID
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, Archetype entitiesArchetype, Dictionary<Entity, Archetype> entityArchetypes, Dictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		return filter.Filter.Entity == matchEntity;
	}
}

public partial class QueryMatchNotEntity : QueryMatchEntity
{
	public override bool PostMatch(Entity matchEntity, QueryArchetypeFilter filter, Archetype entitiesArchetype, Dictionary<Entity, Archetype> entityArchetypes, Dictionary<string, Entity> entityNames, bool preMatched, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		if (preMatched)
		{
			nonMatchingEntity = true;
			return true;
		}

		return false;
	}
}

public partial class QueryMatchNotArchetype : QueryMatchArchetype
{
	public override bool PostMatch(Entity matchEntity, QueryArchetypeFilter filter, Archetype entitiesArchetype, Dictionary<Entity, Archetype> entityArchetypes, Dictionary<string, Entity> entityNames, bool preMatched, out bool nonMatchingEntity)
	{

		nonMatchingEntity = false;
		if (filter.Filter.MatchMethod == FilterMatchMethod.MatchReverse && !preMatched)
		{
			preMatched = !preMatched;
			return preMatched;
		}
		if (preMatched)
		{
			nonMatchingEntity = true;
			return true;
		}

		return false;
	}
}

public partial class QueryMatchEntityName : QueryMatchArchetype
{
	// match the provided entity name with the filter's entity name
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, Archetype entitiesArchetype, Dictionary<Entity, Archetype> entityArchetypes, Dictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		if (filter.Filter is NameIsQueryFilter nf)
		{
			if (entityNames.TryGetValue(nf.Name, out Entity namedEntity))
			{
				return namedEntity == matchEntity;
			}
		}

		return false;
	}
}

public partial class QueryMatchEntityNameRegex : QueryMatchEntityName
{
	// match the provided entity name with the filter's entity name
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, Archetype entitiesArchetype, Dictionary<Entity, Archetype> entityArchetypes, Dictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		if (filter.Filter is NameMatchesQueryFilter nf)
		{
			foreach (var name in entityNames)
			{
				if (name.Value == matchEntity)
				{
					return nf.Regex.IsMatch(name.Key);
				}
			}
		}

		return false;
	}
}
