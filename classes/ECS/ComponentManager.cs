/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentManager
 * @created     : Sunday Apr 21, 2024 13:28:36 CST
 */

namespace GodotEGP.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using GodotEGP.ECS.Exceptions;

public partial class ComponentManager
{
	// max amount of different component types
	private int _maxComponentTypes = 32;

	// next component type ID
	private int _nextComponentTypeId;

	// dictionary of active component type IDs
	private Dictionary<Type, int> _componentTypes;

	// dictionary of active component array instances
	private Dictionary<int, IComponentArray> _componentArrays;

	// size of component arrays
	private int _componentArraySize;

	public ComponentManager(int componentArraySize = 32, int maxComponentTypes = 32)
	{
		_maxComponentTypes = maxComponentTypes;
		_componentArraySize = componentArraySize;

		_componentTypes = new(maxComponentTypes);
		_componentArrays = new(maxComponentTypes);
	}

	public int RegisterComponent<T>() where T : notnull
	{
		Type componentType = typeof(T);

		if (!_componentTypes.TryGetValue(componentType, out int typeId))
		{
			int componentTypeId = _nextComponentTypeId;

			// set component id for type
			_componentTypes[componentType] = componentTypeId;

			// create a component array for this type
			_componentArrays[componentTypeId] = new ComponentArray<T>(_componentArraySize);

			// increase next component type id
			_nextComponentTypeId++;
			
			return componentTypeId;
		}

		throw new ComponentAlreadyRegisteredException($"The component has already been registered.");
	}

	public int GetComponentType<T>() where T : notnull
	{
		if (_componentTypes.TryGetValue(typeof(T), out int typeId))
		{
			return typeId;
		}

		throw new ComponentNotRegisteredException($"Component is not registered.");
	}

	public ComponentArray<T> GetComponentArray<T>() where T : notnull
	{
		return Unsafe.As<ComponentArray<T>>(_componentArrays[GetComponentType<T>()]);
	}

	public void AddComponent<T>(int entityId, T component) where T : notnull
	{
		GetComponentArray<T>().InsertComponent(entityId, component);
	}

	public void RemoveComponent<T>(int entityId) where T : notnull
	{
		GetComponentArray<T>().RemoveComponent(entityId);
	}

	public T GetComponent<T>(int entityId) where T : notnull
	{
		return GetComponentArray<T>().GetComponent(entityId);
	}

	public void DestroyEntityComponents(int entityId)
	{
		// loop over all component arrays and remove the components for this
		// entity id if they have it
		foreach (var componentArray in _componentArrays.Values)
		{
			componentArray.DestroyComponents(entityId);
		}
	}
}

