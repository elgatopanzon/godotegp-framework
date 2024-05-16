/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryResult
 * @created     : Friday May 03, 2024 14:33:49 CST
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

public partial class QueryResult
{
	private PackedArray<Entity> _entities;
	public PackedArray<Entity> Entities
	{
		get {
			return _entities;
		}
	}

	private Query _query;
	public Query Query
	{
		get { return _query; }
		set { _query = value; }
	}

	public QueryResult()
	{
		_entities = new();
	}

	public void AddEntity(Entity entity)
	{
		_entities.Add(entity);
	}
}

