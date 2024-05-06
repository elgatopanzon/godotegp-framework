/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IQueryMatcher
 * @created     : Sunday May 05, 2024 23:44:05 CST
 */

namespace GodotEGP.ECSv3.Queries;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

public partial interface IQueryMatcher
{
	// the pre-match method which does the required matching
	public bool PreMatch(ECS core, Entity entity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, out bool nonMatchingEntity);

	// post-match method to optionally do post-match actions
	public bool PostMatch(ECS core, Entity entity, QueryArchetypeFilter filter, PackedArray<Entity> entitiesArchetypes, bool preMatched, out bool nonMatchingEntity);
}
