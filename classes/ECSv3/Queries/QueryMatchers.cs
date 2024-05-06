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
	public virtual bool PreMatch(ECS core, Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		return true;
	}

	// default post-match is to pass through the pre match result
	public virtual bool PostMatch(ECS core, Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, bool preMatched, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		return preMatched;
	}
}

public partial class QueryMatchArchetype : QueryMatchPassthrough
{
	// match the filter's achetypes with the provided entity archetype using an
	// intersect
	public override bool PreMatch(ECS core, Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		bool matched = (filter.Archetypes.Array.Intersect(entitiesArchetypes.Array).Count() == filter.Archetypes.Count);

		LoggerManager.LogDebug("PreMatch", "", core.EntityHandle(matchEntity).ToString(), matched);

		return matched;
	}
}

public partial class QueryMatchEntity : QueryMatchArchetype
{
	// match the provided entity ID with the filter's entity ID
	public override bool PreMatch(ECS core, Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		return filter.Filter.Entity == matchEntity;
	}
}

public partial class QueryMatchNotEntity : QueryMatchEntity
{
	public override bool PostMatch(ECS core, Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, bool preMatched, out bool nonMatchingEntity)
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
	public override bool PostMatch(ECS core, Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, bool preMatched, out bool nonMatchingEntity)
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
	public override bool PreMatch(ECS core, Entity matchEntity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, out bool nonMatchingEntity)
	{
		nonMatchingEntity = false;
		if (filter.Filter is NameIsQueryFilter nf)
		{
			return nf.Name == core.GetEntityName(matchEntity);
		}

		return false;
	}
}
