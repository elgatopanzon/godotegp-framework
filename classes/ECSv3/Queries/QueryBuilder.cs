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

using GodotEGP.ECSv3;
using GodotEGP.ECSv3.Components;
using GodotEGP.ECSv3.Exceptions;

public partial class QueryBuilder
{
	private Query _query;

	public QueryBuilder()
	{
		Reset();
	}

	// reset the Query object
	public QueryBuilder Reset()
	{
		_query = new();
		return this;
	}

	// build the query object
	public Query Build()
	{
		// TODO: build something?
		return _query;
	}

	public static QueryBuilder Create()
	{
		return new QueryBuilder();
	}

	/*********************
	*  Builder methods  *
	*********************/
	
	// has component or entity ID
	public QueryBuilder Has(Entity entity)
	{
		_query.AddFilter(new HasQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder Has(Query scopedQuery)
	{
		_query.AddFilter(new HasQueryFilter() { Query = scopedQuery });
		return this;
	}

	// and has component or entity ID
	public QueryBuilder And(Entity entity)
	{
		_query.AddFilter(new AndQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder And(Query scopedQuery)
	{
		_query.AddFilter(new AndQueryFilter() { Query = scopedQuery });
		return this;
	}

	// or has component or entity ID
	public QueryBuilder Or(Entity entity)
	{
		_query.AddFilter(new OrQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder Or(Query scopedQuery)
	{
		_query.AddFilter(new OrQueryFilter() { Query = scopedQuery });
		return this;
	}

	// not have the component or entity ID
	public QueryBuilder Not(Entity entity)
	{
		_query.AddFilter(new NotQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder Not(Query scopedQuery)
	{
		_query.AddFilter(new NotQueryFilter() { Query = scopedQuery });
		return this;
	}

	// and not have the component or entity ID
	public QueryBuilder AndNot(Entity entity)
	{
		_query.AddFilter(new AndNotQueryFilter() { Entity = entity });
		return this;
	}
	public QueryBuilder AndNot(Query scopedQuery)
	{
		_query.AddFilter(new AndNotQueryFilter() { Query = scopedQuery });
		return this;
	}

	// is to match the entity to the entity in the query
	public QueryBuilder Is(Entity entity)
	{
		_query.AddFilter(new IsQueryFilter() { Entity = entity });
		return this;
	}

	public QueryBuilder IsNot(Entity entity)
	{
		_query.AddFilter(new IsNotQueryFilter() { Entity = entity });
		return this;
	}
}
