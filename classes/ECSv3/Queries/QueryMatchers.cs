/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryMatchers
 * @created     : Sunday May 05, 2024 23:44:40 CST
 */

namespace GodotEGP.ECSv3.Queries;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;
using System.Linq;

public partial class QueryMatchPassthrough : IQueryMatcher
{
	public virtual bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		return true;
	}

	// default post-match is to pass through the pre match result
	public virtual bool PostMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, bool preMatched, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		return preMatched;
	}
}

public partial class QueryMatchArchetype : QueryMatchPassthrough
{
	// match the filter's achetypes with the provided entity archetype using an
	// intersect
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;

		int matchCount = filter.Archetypes.Array.Intersect(entitiesArchetypes.Array).Count();
		bool matched = (matchCount == filter.Archetypes.Count);

		// match a wildcard by making sure the entity has >= archetype count,
		// and the archetype match was just 1 less than the filter archetype
		// count
		if (filter.Filter.Entity == 0 && matchCount == (filter.Archetypes.Count - 1) && entitiesArchetypes.Count >= filter.Archetypes.Count)
		{
			matched = true;
		}

		LoggerManager.LogDebug("PreMatch", "", matchEntity.ToString(), matched);

		return matched;
	}
}

public partial class QueryMatchEntity : QueryMatchArchetype
{
	// match the provided entity ID with the filter's entity ID
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		return filter.Filter.Entity == matchEntity;
	}
}

public partial class QueryMatchNotEntity : QueryMatchEntity
{
	public override bool PostMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, bool preMatched, out bool nonMatchingEntity)
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
	public override bool PostMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, bool preMatched, out bool nonMatchingEntity)
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
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, out bool nonMatchingEntity)
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
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		if (filter.Filter is NameMatchesQueryFilter nf)
		{
			for (int i = 0; i < entityNames.Values.Count; i++)
			{
				if (entityNames.Values[i] == matchEntity)
				{
					return nf.Regex.IsMatch(entityNames.Keys[i]);
				}
			}
		}

		return false;
	}
}

public partial class QueryMatchPairArchetype : QueryMatchPassthrough
{
	// match the filter's id and pair id, since we're matching on a pair level
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		bool matched = false;

		if (filter.Filter is HasPairQueryFilter pf)
		{
			LoggerManager.LogDebug("Matching pair archetype", "", pf.Entity.ToString(), pf.Entity);

			// match entity archetypes against pair
			int matchCount = 0;
			foreach (var archetype in entitiesArchetypes.Array)
			{
				// skip non-pairs
				if (archetype.PairId == 0)
				{
					continue;
				}

				LoggerManager.LogDebug("Matching pair", "", "entityArchetype", archetype);
				LoggerManager.LogDebug("Against pair", "", "filterArchetype", pf.Entity);

				if ((archetype.Id == pf.SourceEntity.Id || pf.SourceEntity.Id == 0) && (archetype.PairId == pf.TargetEntity.Id || pf.TargetEntity.Id == 0))
				{
					matchCount++;
				}
			}

			matched = (matchCount >= 1);
		}

		LoggerManager.LogDebug("PreMatch", "", matchEntity.ToString(), matched);

		return matched;
	}
}

public partial class QueryMatchNotPairArchetype : QueryMatchPairArchetype
{
	public override bool PostMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, bool preMatched, out bool nonMatchingEntity)
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

public partial class QueryMatchPairTargetArchetype : QueryMatchArchetype
{
	// match the filter's id and pair id, since we're matching on a pair level
	public override bool PreMatch(Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entityArchetype, PackedDictionary<Entity, PackedArray<Entity>> entityArchetypes, PackedDictionary<string, Entity> entityNames, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		bool matched = false;

		if (filter.Filter is PairTargetHasQueryFilter pth)
		{
			// check if matching entity has the target entity
			bool hasTarget = entityArchetype.Contains(filter.Filter.Entity);

			Entity targetPair;
			if (pth.MatchPairTarget)
			{
				targetPair = Entity.CreateFrom(pth.SourceEntity.Id, matchEntity.Id);
			}
			else
			{
				targetPair = Entity.CreateFrom(matchEntity.Id, pth.TargetEntity.Id);

			}

			if (hasTarget)
			{
				LoggerManager.LogDebug("Potential pair target match", "", "entity", matchEntity);

				// search for potential pairs in other entity archetypes
				for (int i = 0; i < entityArchetypes.Values.Count; i++)
				{
					// attempt to match pair with source/target id
					if (entityArchetypes.Values[i].Contains(targetPair))
					{
						return true;
					}
				}
			}
		}

		LoggerManager.LogDebug("PreMatch", "", matchEntity.ToString(), matched);

		return matched;
	}
}
