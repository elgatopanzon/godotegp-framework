/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryFilters
 * @created     : Friday May 03, 2024 13:55:13 CST
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

using System.Linq;

public enum FilterMatchType
{
	And = 0, // must include
	Or = 1, // include either the previous or this one
	Not = 2, // do not include
	Optional = 3, // match if it does or doesn't
	Is = 4, // trigger matching entity directly, not archetype
	IsNot = 5, // trigger matching entity directly, not archetype
}

public enum FilterMatchMethod
{
	Match = 0,
	MatchReverse = 1,
}

public partial class QueryFilterBase : IQueryFilter
{
	public virtual FilterMatchType MatchType
	{
		get {
			return FilterMatchType.And;
		}
	}
	public Entity Entity { get; set; }
	public Query ScopedQuery { get; set; }
	public virtual bool TriggerFilterEnd { get; set; }
	public virtual FilterMatchMethod MatchMethod { get; set; }

	protected IQueryMatcher _matcher;
	public virtual IQueryMatcher Matcher 
	{ 
		get {
			return _matcher;
		}
	}
}

public partial class HasQueryFilter : QueryFilterBase
{
	public override FilterMatchType MatchType
	{
		get {
			return FilterMatchType.And;
		}
	}

	public HasQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchArchetype();
	}
}

public partial class AndQueryFilter : HasQueryFilter {}

public partial class OrQueryFilter : QueryFilterBase
{
	public override FilterMatchType MatchType
	{
		get {
			return FilterMatchType.Or;
		}
	}
	public override bool TriggerFilterEnd
	{
		get {
			return true;
		}
	}

	public OrQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchArchetype();
	}
}

public partial class NotQueryFilter : QueryFilterBase
{
	public override FilterMatchType MatchType
	{
		get {
			return FilterMatchType.Not;
		}
	}
	public override bool TriggerFilterEnd
	{
		get {
			return true;
		}
	}

	public NotQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchNotArchetype();
	}
}
public partial class AndNotQueryFilter : NotQueryFilter
{
	public override FilterMatchType MatchType
	{
		get {
			return FilterMatchType.Not;
		}
	}
	public override bool TriggerFilterEnd
	{
		get {
			return true;
		}
	}
}

// public partial class OptionallyQueryFilter : QueryFilterBase
// {
// 	public override FilterMatchType MatchType
// 	{
// 		get {
// 			return FilterMatchType.Optional;
// 		}
// 	}
// 	public override bool TriggerFilterEnd
// 	{
// 		get {
// 			return true;
// 		}
// 	}
// }

public partial class IsQueryFilter : QueryFilterBase
{
	public override FilterMatchType MatchType
	{
		get {
			return FilterMatchType.Is;
		}
	}
	public override FilterMatchMethod MatchMethod
	{
		get {
			return FilterMatchMethod.Match;
		}
	}
	public override bool TriggerFilterEnd
	{
		get {
			return true;
		}
	}

	public IsQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchEntity();
	}
}
public partial class IsNotQueryFilter : QueryFilterBase
{
	public override FilterMatchType MatchType
	{
		get {
			return FilterMatchType.IsNot;
		}
	}
	public override FilterMatchMethod MatchMethod
	{
		get {
			return FilterMatchMethod.Match;
		}
	}
	public override bool TriggerFilterEnd
	{
		get {
			return true;
		}
	}

	public IsNotQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchNotEntity();
	}
}
