/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IQueryFilter
 * @created     : Friday May 03, 2024 13:54:56 CST
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

public partial interface IQueryFilter
{
	// used to specify the match type and the match method
	public FilterMatchType MatchType { get; }
	public FilterMatchMethod MatchMethod { get; set; }

	// holds the entity to match against
	public Entity Entity { get; set; }

	// holds another query to run instead of a simple archetype match
	public Query ScopedQuery { get; set; }

	// determines if the built filter triggers the end of the filter collection
	public bool TriggerFilterEnd { get; set; }

	// instance of the query matcher to use with this filter
	public IQueryMatcher Matcher { get; }
}
