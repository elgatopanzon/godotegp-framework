/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryFilters
 * @created     : Friday May 03, 2024 13:55:13 CST
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
using GodotEGP.ECSv4.Components;
using GodotEGP.ECSv4.Exceptions;

using System.Text.RegularExpressions;

public enum FilterMatchType
{
	And = 0, // must include
	Or = 1, // include either the previous or this one
	Not = 2, // do not include
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

public partial class QueryFilterOrTrigger : QueryFilterBase 
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
}

public partial class HasQueryFilter : QueryFilterBase
{
	public HasQueryFilter()
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

	public NotQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchNotArchetype();
	}
}

public partial class IsQueryFilter : QueryFilterBase
{
	public IsQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchEntity();
	}
}
public partial class IsNotQueryFilter : QueryFilterBase
{
	public IsNotQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchNotEntity();
	}
}

public partial class NameIsQueryFilter : QueryFilterBase
{
	public string Name { get; set; }

	public NameIsQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchEntityName();
	}
}

public partial class NameMatchesQueryFilter : QueryFilterBase
{
	public Regex Regex { get; set; }

	public NameMatchesQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchEntityNameRegex();
	}
}

public partial class HasPairQueryFilter : QueryFilterBase
{
	// pair source entity (defaults to 0 (wildcard)
	public Entity SourceEntity { get; set; }
	// pair target entity (defaults to 0 (wildcard)
	public Entity TargetEntity { get; set; }

	public HasPairQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchPairArchetype();
	}
}

public partial class NotHasPairQueryFilter : HasPairQueryFilter
{
	public override FilterMatchType MatchType
	{
		get {
			return FilterMatchType.Not;
		}
	}

	public NotHasPairQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchNotPairArchetype();
	}
}

public partial class PairTargetHasQueryFilter : HasPairQueryFilter
{
	public bool MatchPairTarget { get; set; }

	public PairTargetHasQueryFilter()
	{
		_matcher = (IQueryMatcher) new QueryMatchPairEntityArchetype();
		MatchPairTarget = true;
	}
}

public partial class PairSourceHasQueryFilter : PairTargetHasQueryFilter
{
	public PairSourceHasQueryFilter()
	{
		MatchPairTarget = false;
	}
}
