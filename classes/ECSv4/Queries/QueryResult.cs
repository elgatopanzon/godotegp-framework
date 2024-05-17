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
using GodotEGP.ECSv4.Components;
using System.Runtime.CompilerServices;

public partial class QueryResult
{
	// holds entity objects representing the entities in the results
	private PackedArray<QueryResultEntity> _entities;
	public PackedArray<QueryResultEntity> Entities
	{
		get {
			return _entities;
		}
	}

	// a map of the components belonging to the results of the query
	private IndexMap<IComponentArray> _componentArrayCache;
	public IndexMap<IComponentArray> ComponentArrayCache
	{
		get { return _componentArrayCache; }
		set { _componentArrayCache = value; }
	}


	public QueryResult()
	{
		_entities = new();
		_componentArrayCache = new();
	}

	/****************************
	*  Cached component methods  *
	****************************/

	public void CacheComponentArray(Entity typeId, IComponentArray componentArray)
	{
		_componentArrayCache[typeId] = componentArray;
	}

	public ref T GetComponent<T>(Entity entity) where T : IComponentData
	{
		return ref Unsafe.As<ComponentArray<T>>(_componentArrayCache[T.Id]).GetComponent(entity);
	}

	/***********************
	*  Entity management  *
	***********************/

	public void AddEntity(Entity entity)
	{
		_entities.Add(new QueryResultEntity () {
			Entity = entity,
		});
	}

	public void RemoveEntity(Entity entity)
	{
		for (int i = 0; i < _entities.Count; i++)
		{
			if (_entities[i].Entity == entity)
			{
				_entities.RemoveAt(i);
				break;
			}
		}
	}

	public bool ContainsEntity(Entity entity)
	{
		for (int i = 0; i < _entities.Count; i++)
		{
			if (_entities[i].Entity == entity)
			{
				return true;
			}
		}

		return false;
	}

	public void ClearEntities()
	{
		_entities.Clear();
	}
}

public partial class QueryResultEntity
{
	public Entity Entity { get; set; }
}
