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

using System.Collections.Generic;

public partial class QueryResult
{
	// holds entity objects representing the entities in the results
	private Dictionary<int, Entity> _entities;
	public Dictionary<int, Entity> Entities
	{
		get {
			return _entities;
		}
	}

	// a map of the components belonging to the results of the query
	private IComponentArray[] _componentArrays;
	public IComponentArray[] ComponentArrays
	{
		get { return _componentArrays; }
	}

	private int _componentArraySize;

	public QueryResult()
	{
		_entities = new();
	}

	/****************************
	*  Cached component methods  *
	****************************/

	public void CacheComponentArray(Entity typeId, IComponentArray componentArray)
	{
		if (_componentArraySize <= typeId.Id + 1)
		{
			_componentArraySize = typeId.Id + 1;
			System.Array.Resize(ref _componentArrays, _componentArraySize);
		}
		_componentArrays[typeId] = componentArray;
	}

	public ref T GetComponent<T>(Entity entity) where T : IComponentData
	{
		return ref Unsafe.As<ComponentArray<T>>(_componentArrays[T.Id]).GetComponent(entity);
	}

	/***********************
	*  Entity management  *
	***********************/

	public void AddEntity(Entity entity)
	{
		_entities[entity.Id] = entity;
	}

	public void RemoveEntity(Entity entity)
	{
		_entities.Remove(entity.Id);
	}

	public bool ContainsEntity(Entity entity)
	{
		return _entities.ContainsKey(entity.Id);
	}

	public void ClearEntities()
	{
		_entities = new();
	}
}
