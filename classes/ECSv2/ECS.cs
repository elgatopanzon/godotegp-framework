/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECS
 * @created     : Friday Apr 26, 2024 20:14:16 CST
 */

namespace GodotEGP.ECSv2;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Collections;
using GodotEGP.ECSv2.Components;

// ECS core manager class is also a service
public partial class ECS : Service
{
	private EntityManager _entityManager;
	private ComponentManager _componentManager;

	public ECS()
	{
		_entityManager = new();
		_componentManager = new(_entityManager);
	}


	/*******************************
	*  Entity management methods  *
	*******************************/

	public Entity CreateEntity()
	{
		Entity e = _entityManager.Create();

		LoggerManager.LogDebug("Creating new entity", "", "entity", e);

		return e;
	}

	public void DestroyEntity(Entity entity)
	{
		LoggerManager.LogDebug("Destroying entity", "", "entity", entity);

		_entityManager.Destroy(entity);
	}


	/**********************************
	*  Component management methods  *
	**********************************/
	
	// add a component of type IComponent to an entity
	public void AddComponent<T>(Entity entity, T component) where T : IComponent
	{
		LoggerManager.LogDebug("Adding component to entity", "", entity.ToString(), component.GetType());

		// add the component to the component array
		_componentManager.Add<T>(entity, component);

		SetEntityComponentArchetypeState<T>(entity, true);
	}

	// remove the component of type IComponent from an entity
	public void RemoveComponent<T>(Entity entity) where T : IComponent
	{
		LoggerManager.LogDebug("Removing component from entity", "", entity.ToString(), typeof(T));

		// remove the component from the component array
		_componentManager.Remove<T>(entity);

		SetEntityComponentArchetypeState<T>(entity, false);
	}
	// get the component of type T for an entity
	public ref T GetComponent<T>(Entity entity) where T : IComponent
	{
		return ref _componentManager.GetComponent<T>(entity);
	}

	// get the component type id for type T
	public ulong GetComponentType<T>() where T : IComponent
	{
		return _componentManager.GetComponentType<T>();
	}

	// check if an entity has an instance of a component
	public bool HasComponent<T>(Entity entity) where T : IComponent
	{
		return _entityManager.HasArchetypeId(entity, GetComponentType<T>());
	}


	/***************************************
	*  Component pair management methods  *
	***************************************/

	// add a component pair to an entity
	public void AddComponent<T, TT>(Entity entity, T component, TT component2) 
		where T : IComponent
		where TT : IComponent
	{
		LoggerManager.LogDebug("Adding component pair to entity", "", entity.ToString(), $"{typeof(T).Name}, {typeof(TT).Name}");

		// trigger creation of underlying type ID
		GetComponentType<T, TT>();

		// add the component to the component array
		_componentManager.Add<EcsRelation<T, TT>>(entity, new EcsRelation<T, TT>() {
			Relation = component,
			Target = component2,
			});

		SetEntityComponentArchetypeState<EcsRelation<T, TT>>(entity, true);
	}

	// remove a component pair from an entity
	public void RemoveComponent<T, TT>(Entity entity)
		where T : IComponent
		where TT : IComponent
	{
		LoggerManager.LogDebug("Removing component pair from entity", "", entity.ToString(), $"{typeof(T).Name}, {typeof(TT).Name}");

		// remove the component from the component array
		_componentManager.Remove<EcsRelation<T, TT>>(entity);

		SetEntityComponentArchetypeState<EcsRelation<T, TT>>(entity, false);
	}

	// get a component pair from an entity
	public EcsRelation<T, TT> GetComponent<T, TT>(Entity entity) 
		where T : IComponent
		where TT : IComponent
	{
		return _componentManager.GetComponent<EcsRelation<T, TT>>(entity);
	}

	// check if an entity has a component pair
	public bool HasComponent<T, TT>(Entity entity)
		where T : IComponent
		where TT : IComponent
	{
		Entity componentId = Entity.CreateFrom(GetComponentType<T, TT>());

		return _entityManager.HasArchetypeId(entity, componentId);
	}

	// get the component type id for a component pair
	public ulong GetComponentType<T, TT>() 
		where T : IComponent
		where TT : IComponent
	{
		return _componentManager.GetComponentType<T, TT>();
	}


	/**************************************************
	*  Component pair (entities) management methods  *
	**************************************************/
	
	// add a component pair with entity to an entity
	public void AddComponent<T>(Entity entity, T component, Entity entity2) 
		where T : IComponent
	{
		LoggerManager.LogDebug("Adding component entity pair to entity", "", entity.ToString(), $"{typeof(T).Name}, {entity2}");

		// create a component type id to represent the pair
		ulong leftComponentId = GetComponentType<T>();
		Entity componentEntityPairId = Entity.CreateFrom((uint) leftComponentId, entity2.Id);

		LoggerManager.LogDebug("Component entity pair id", "", "pairId", componentEntityPairId);

		// create a component array using the type id
		// TODO: refactor everything from T and TT to ComponentArray<IComponent>
		// so we can create component ids using standard ulong ID
		// a. this allows adding a component array for the components of a
		// (component, entityId) pair
		// b. we can still use T and TT, but it will instead be stored directly
		// as (componentA, componentB) id pair
		//		do we have to use EcsRelation<T, TT> for it?
		//		perhaps we can take the generated pair ID as an entity ID
		//		and instead set/access the components directly for that ID
		//		and store them in their respective stores

		// // trigger creation of underlying type ID
		// GetComponentType<T, TT>();
        //
		// // add the component to the component array
		// _componentManager.Add<EcsRelation<T, TT>>(entity, new EcsRelation<T, TT>() {
		// 	Relation = component,
		// 	Target = component2,
		// 	});
        //
		// SetEntityComponentArchetypeState<EcsRelation<T, TT>>(entity, true);

		// trigger creation of underlying type ID
		GetComponentType<T>(entity2);

		// add the component to the component array
		_componentManager.Add<T>(entity, component, entity2);

		SetEntityArchetypeIdState(entity, componentEntityPairId, true);
	}

	// get the component type id for component entity pair
	public ulong GetComponentType<T>(Entity entity) 
		where T : IComponent
	{
		LoggerManager.LogDebug("Getting component entity pair type id", "", typeof(T).Name, entity);

		return _componentManager.GetComponentType<T>(entity);
	}

	public bool HasComponent<T>(Entity entity, Entity entity2) where T : IComponent
	{
		Entity typeId = GetComponentType<T>(entity2);

		LoggerManager.LogDebug("Checking if entity has type id", "", entity.ToString(), entity2);

		return _entityManager.HasArchetypeId(entity, typeId);
	}
	// remove the component entity pair of type IComponent from an entity
	public void RemoveComponent<T>(Entity entity, Entity entity2) where T : IComponent
	{
		LoggerManager.LogDebug("Removing component entity pair from entity", "", entity.ToString(), typeof(T));

		// remove the component from the component array
		_componentManager.Remove<T>(entity, entity2);

		SetEntityArchetypeIdState(entity, GetComponentType<T>(entity2), false);
	}

	/**********************************
	*  Archetype management methods  *
	**********************************/
	
	// get the archetype of an entity
	public PackedArray<Entity> GetArchetype(Entity entity)
	{
		return _entityManager.GetArchetype(entity);
	}

	// set the archetype id state for an entity
	public void SetEntityArchetypeIdState(Entity entity, Entity id, bool state)
	{
		LoggerManager.LogDebug("Setting entity archetype id state", "", entity.ToString(), $"{id.ToString()} = {state}");

		// update the state for the entities archetype
		if (state == true)
		{
			// add the ID to the entities Archetype id list
			if (!_entityManager.HasArchetypeId(entity, id))
			{
				_entityManager.AddArchetypeId(entity, id);
			}
		}
		else
		{
			// remove the ID to the entities Archetype id list
			_entityManager.RemoveArchetypeId(entity, id);
		}

		// TODO: update systems entity lists to reflect the new archetype change
		// _systemManager.UpdateEntityArchetype(entity, entityArchetype);
	}

	// set an entities archetype state for an IComponent
	public void SetEntityComponentArchetypeState<T>(Entity entity, bool state) where T : IComponent
	{
		SetEntityArchetypeIdState(entity, GetComponentType<T>(), state);
	}
}
