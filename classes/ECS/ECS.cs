/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECS
 * @created     : Sunday Apr 21, 2024 18:06:49 CST
 */

namespace GodotEGP.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Collections;

public partial class ECS : Service
{
	private int _maxEntities;
	private int _maxComponents;
	private int _maxSystems;

	private EntityManager _entityManager;
	private ComponentManager _componentManager;
	private SystemManager _systemManager;

	public ECS(int maxEntities = 100, int maxComponents = 32, int maxSystems = 32)
	{
		_maxEntities = maxEntities;
		_maxComponents = maxComponents;
		_maxSystems = maxSystems;

		// init the managers
		_entityManager = new(maxEntities:maxEntities, maxComponents:maxComponents);
		_componentManager = new(componentArraySize:maxEntities, maxComponentTypes:maxComponents);
		_systemManager = new(maxSystems:maxSystems, maxComponents:maxComponents);
	}


	/*******************************
	*  Entity management methods  *
	*******************************/
	
	public int CreateEntity()
	{
		return _entityManager.Create();
	}

	public void DestroyEntity(int entityId)
	{
		_entityManager.Destroy(entityId);
		_componentManager.DestroyEntityComponents(entityId);
		_systemManager.DestroyEntity(entityId);
	}


	/**********************************
	*  Component management methods  *
	**********************************/
	
	public void RegisterComponent<T>() where T : notnull
	{
		_componentManager.RegisterComponent<T>();
	}

	public void AddComponent<T>(int entityId, T component) where T : notnull
	{
		// add the component to the component array
		_componentManager.AddComponent<T>(entityId, component);

		SetEntityComponentArchetypeState<T>(entityId, true);
	}

	public void RemoveComponent<T>(int entityId)
	{
		// remove the component from the component array
		_componentManager.RemoveComponent<T>(entityId);

		SetEntityComponentArchetypeState<T>(entityId, false);
	}

	public void SetEntityComponentArchetypeState<T>(int entityId, bool state) where T : notnull
	{
		// update the state for the entities archetype
		BitArray entityArchetype = _entityManager.GetArchetype(entityId);
		entityArchetype.Set(_componentManager.GetComponentType<T>(), state);
		_entityManager.SetArchetype(entityId, entityArchetype);

		// update systems entity lists to reflect the new archetype change
		_systemManager.UpdateEntityArchetype(entityId, entityArchetype);
	}

	public ref T GetComponent<T>(int entityId) where T : notnull
	{
		return ref _componentManager.GetComponent<T>(entityId);
	}

	public int GetComponentType<T>() where T : notnull
	{
		return _componentManager.GetComponentType<T>();
	}


	/*******************************
	*  System management methods  *
	*******************************/
	
	public SystemBase RegisterSystem<T>() where T : SystemBase, new()
	{
		return _systemManager.RegisterSystem<T>(this);
	}

	public void SetSystemArchetype<T>(BitArray archetype) where T : SystemBase
	{
		_systemManager.SetSystemArchetype<T>(archetype);
	}

	public void SetSystemComponentArchetypeState<T, TT>(bool state)
		where T : SystemBase
		where TT : notnull
	{
		int componentTypeId = _componentManager.GetComponentType<TT>();
		BitArray systemArchetype = _systemManager.GetSystemArchetype<T>();

		// update the state for the entities archetype
		systemArchetype.Set(componentTypeId, state);
		_systemManager.SetSystemArchetype<T>(systemArchetype);
	}
}
