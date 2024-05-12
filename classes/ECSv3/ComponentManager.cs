/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentManager
 * @created     : Monday Apr 29, 2024 20:51:51 CST
 */

namespace GodotEGP.ECSv3;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv3.Components;
using GodotEGP.Collections;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ComponentTypeId = Entity;

public partial class ComponentManager
{
	// component storage by component id 
	private IndexMap<IComponentArray> _componentArrays;

	// Type to componentId mapping
	// private Dictionary<Type, ComponentTypeId> _componentTypeIds;

	private EntityManager _entityManager;

	public ComponentManager(EntityManager entityManager)
	{
		_componentArrays = new();
		// _componentTypeIds = new();

		_entityManager = entityManager;
	}

	/**********************************
	*  Component type ID management  *
	**********************************/

	// register component type and generate a component ID
	public int CreateTypeId<T>() where T : IComponent
	{
		return typeof(T).GetHashCode();
		// // register the component type
		// if (!_componentTypeIds.TryGetValue(typeof(T), out Entity id))
		// {
		// 	id = _entityManager.Create();
		// 	_componentTypeIds[typeof(T)] = id;
		// }

		// return id;
	}

	public int GetTypeId<T>() where T : IComponent
	{
		return CreateTypeId<T>();
	}


	/********************************
	*  Component array management  *
	********************************/
	
	// create a component array for a type ID as type T
	public IComponentArray CreateComponentArray<T>(int typeId) where T : IComponent
	{
		if (_componentArrays.IndexOfData(typeId) == -1)
		{
			IComponentArray array = new ComponentArray<T>();
			_componentArrays[typeId] = array;
			return array;
		}
		// if (!_componentArrays.TryGetValue(typeId, out IComponentArray array))
		// {
		// 	array = new ComponentArray<T>();
		// 	_componentArrays[typeId] = array;
		// }

		return _componentArrays[typeId];
	}

	// create a component array for type T without an ID
	public IComponentArray CreateComponentArray<T>() where T : IComponent
	{
		int typeId = CreateTypeId<T>();
		return CreateComponentArray<T>(typeId);
	}

	// get component array for type ID
	public ComponentArray<T> GetComponentArray<T>(int typeId) where T : IComponent
	{
		return Unsafe.As<ComponentArray<T>>(_componentArrays[typeId]);
	}

	// get component array for type without ID
	public ComponentArray<T> GetComponentArray<T>() where T : IComponent
	{
		return Unsafe.As<ComponentArray<T>>(CreateComponentArray<T>());
	}


	/**********************************
	*  Component management methods  *
	**********************************/

	// add the component to the component array with the given type id
	public ComponentTypeId AddComponent<T>(Entity entity, ComponentTypeId typeId, T component) where T : IComponent
	{
		GetComponentArray<T>((int) typeId.Id).InsertComponent(entity, component);

		return typeId;
	}
	// add the component to the component array without type ID
	public ComponentTypeId AddComponent<T>(Entity entity, T component) where T : IComponent
	{
		int typeId = GetTypeId<T>();
		GetComponentArray<T>(typeId).InsertComponent(entity, component);

		return Entity.CreateFrom((ulong) typeId);;
	}

	// get the component of type T with the provided type ID
	public ref T GetComponent<T>(Entity entity, ComponentTypeId typeId) where T : IComponent
	{
		return ref GetComponentArray<T>((int) typeId.Id).GetComponent(entity);
	}
	public ref T GetComponent<T>(Entity entity, int typeId) where T : IComponent
	{
		return ref GetComponentArray<T>(typeId).GetComponent(entity);
	}
	// get the component of type T without type ID
	public ref T GetComponent<T>(Entity entity) where T : IComponent
	{
		return ref GetComponentArray<T>(GetTypeId<T>()).GetComponent(entity);
	}

	// remove the component of type T with type ID
	public void RemoveComponent<T>(Entity entity, ComponentTypeId typeId) where T : IComponent
	{
		GetComponentArray<T>((int) typeId.Id).RemoveComponent(entity);
	}
	// remove the component of type T without type ID
	public void RemoveComponent<T>(Entity entity) where T : IComponent
	{
		GetComponentArray<T>(GetTypeId<T>()).RemoveComponent(entity);
	}

	// destroy all components for the given entity
	public void DestroyEntityComponents(Entity entity)
	{
		// loop over all component arrays and remove the components for this
		// entity id if they have it
		foreach (var componentArray in _componentArrays.Values)
		{
			componentArray.DestroyComponents(entity);
		}
	}
}

