/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentManager
 * @created     : Sunday Apr 28, 2024 16:24:49 CST
 */

namespace GodotEGP.ECSv2;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Runtime.CompilerServices;

using GodotEGP.Collections;
using GodotEGP.ECSv2.Components;

using ComponentTypeId = ulong;

public partial class ComponentManager
{
	// reference to EntityManager instance
	private EntityManager _entityManager;

	// component storage by component id 
	private PackedDictionary<ComponentTypeId, IComponentArray> _componentArrays;

	// Type to componentId mapping
	private PackedDictionary<Type, ComponentTypeId> _componentTypeIds;

	public ComponentManager(EntityManager entityManager)
	{
		_entityManager = entityManager;

		_componentArrays = new();
		_componentTypeIds = new();
	}


	/****************************************
	*  Component array management methods  *
	****************************************/

	public ComponentTypeId CreateComponentArray<T>(Entity componentTypeId) where T : IComponent
	{
		if (!_componentArrays.ContainsKey(componentTypeId))
		{
			// create a new component array for this component type
			_componentArrays[componentTypeId] = new ComponentArray<T>();
		}

		return componentTypeId;
	}

	public ComponentTypeId CreateComponentArray<T>() where T : IComponent
	{
		Type componentType = typeof(T);
		ulong componentTypeId;

		if (!_componentTypeIds.ContainsKey(componentType))
		{
			// create an entity ID for this component type
			// and take the first half uint as the id
			Entity componentEntity = _entityManager.Create();
			componentTypeId = componentEntity.Id;

			// set the component type => type id mapping so that we can later
			// find the created type id with just the Type
			_componentTypeIds[componentType] = componentTypeId;

			// create a new component array for this component type
			_componentArrays[componentTypeId] = new ComponentArray<T>();
		}
		else
		{
			componentTypeId = _componentTypeIds[componentType];
		}

		return componentTypeId;
	}

	public ComponentTypeId CreateComponentArray<T, TT>() 
		where T : IComponent
		where TT : IComponent
	{
		Type componentType = typeof(EcsRelation<T, TT>);
		ulong componentTypeId;

		LoggerManager.LogDebug("Creating component pair array", "", "pair", $"{typeof(T).Name}, {typeof(TT).Name}");

		if (!_componentTypeIds.ContainsKey(componentType))
		{
			ComponentTypeId typeLeftId = CreateComponentArray<T>();
			ComponentTypeId typeRightId = CreateComponentArray<TT>();

			Entity pairEntityId = Entity.CreateFrom((uint) typeLeftId, (uint) typeRightId);
			_componentTypeIds[componentType] = pairEntityId;
			_componentArrays[pairEntityId] = new ComponentArray<EcsRelation<T, TT>>();

			LoggerManager.LogDebug("Component type ids", "", "typeIds", _componentTypeIds);
			LoggerManager.LogDebug("Component arrays", "", "arrays", _componentArrays);

			LoggerManager.LogDebug("Pair component type id", "", "pairTypeId", GetComponentType<EcsRelation<T, TT>>());

			componentTypeId = pairEntityId;
		}
		else
		{
			componentTypeId = _componentTypeIds[componentType];
		}

		return componentTypeId;
	}

	public ComponentArray<T> GetComponentArray<T>() where T : IComponent
	{
		LoggerManager.LogDebug("Getting component array", "", "type", typeof(T));
		LoggerManager.LogDebug("Component array type id", "", "type", GetComponentType<T>());

		return Unsafe.As<ComponentArray<T>>(_componentArrays[GetComponentType<T>()]);
	}

	public ComponentArray<T> GetComponentArray<T>(Entity entity) where T : IComponent
	{
		LoggerManager.LogDebug("Getting pair component array", "", "type", typeof(T));
		LoggerManager.LogDebug("Component pair array type id", "", "type", GetComponentType<T>(entity));

		return Unsafe.As<ComponentArray<T>>(_componentArrays[GetComponentType<T>(entity)]);
	}

	public ComponentTypeId GetComponentType<T>() where T : IComponent
	{
		// create the component type id and component array if it doesn't exist
		if (!_componentTypeIds.ContainsKey(typeof(T)))
		{
			CreateComponentArray<T>();
		}

		return _componentTypeIds[typeof(T)];
	}

	// get a component type id corresponding to the pair
	public ComponentTypeId GetComponentType<T, TT>()
		where T : IComponent
		where TT : IComponent
	{
		Type relationType = typeof(EcsRelation<T, TT>);

		// create the component type id and component array if it doesn't exist
		if (!_componentTypeIds.ContainsKey(relationType))
		{
			CreateComponentArray<T, TT>();
		}

		return _componentTypeIds[relationType];
	}

	// get a component type id corresponding to a component entity pair
	public ComponentTypeId GetComponentType<T>(Entity entity) where T : IComponent
	{
		// trigger creation of relation type ID
		Entity relationTypeId = GetComponentType<T>();

		LoggerManager.LogDebug("Pair relation type id", "", "id", relationTypeId);

		// prepare the component entity id
		Entity relationPairId = Entity.CreateFrom((uint) relationTypeId, entity.Id);

		LoggerManager.LogDebug("Pair relation pair id", "", "id", relationPairId);

		// if the array for this pair doesn't exist, create it
		if (!_componentArrays.ContainsKey(relationPairId))
		{
			CreateComponentArray<T>(relationPairId);
		}

		return relationPairId;;
	}

	/**********************************
	*  Component management methods  *
	**********************************/

	public void Add<T>(Entity entity, T component) where T : IComponent
	{
		// add the component to the component array
		GetComponentArray<T>().InsertComponent(entity, component);
	}

	public void Add<T>(Entity entity, T component, Entity entity2) where T : IComponent
	{
		// add the component to the component array
		GetComponentArray<T>(entity2).InsertComponent(entity, component);
	}

	public void Remove<T>(Entity entity) where T : IComponent
	{
		GetComponentArray<T>().RemoveComponent(entity);
	}

	public void Remove<T>(Entity entity, Entity entity2) where T : IComponent
	{
		GetComponentArray<T>(entity2).RemoveComponent(entity);
	}

	public ref T GetComponent<T>(Entity entity) where T : IComponent
	{
		return ref GetComponentArray<T>().GetComponent(entity);
	}

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

