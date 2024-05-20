/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECS
 * @created     : Monday Apr 29, 2024 20:51:05 CST
 */

namespace GodotEGP.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.ECSv4.Components;
using GodotEGP.ECSv4.Exceptions;
using GodotEGP.ECSv4.Queries;
using GodotEGP.ECSv4.Systems;

using System;
using System.Linq;
using System.Collections.Generic;

public partial class ECS : Service
{
	private ECSConfig _config;
	public ECSConfig Config
	{
		get {
			return _config;
		}
	}

	private EntityManager _entityManager;
	private ComponentManager _componentManager;
	private QueryManager _queryManager;
	private SystemManager _systemManager;
	private SystemScheduler _systemScheduler;
	private ObjectManager _objectManager;

	public EntityManager EntityManager { get { return _entityManager; }}
	public ComponentManager ComponentManager { get { return _componentManager; }}
	public QueryManager QueryManager { get { return _queryManager; }}
	public SystemManager SystemManager { get { return _systemManager; }}
	public SystemScheduler SystemScheduler { get { return _systemScheduler; }}
	public ObjectManager ObjectManager { get { return _objectManager; }}

	private readonly object _registerComponentLock = new object();

	// default component IDs
	public readonly Entity EcsWildcard;
	public readonly Entity EcsTag;
	public readonly Entity EcsComponent;
	public readonly Entity EcsComponentConfig;
	public readonly Entity EcsObject;
	public readonly Entity EcsDisabled;
	public readonly Entity EcsQuery;
	public readonly Entity EcsReadOnlyQuery;
	public readonly Entity EcsReadWriteQuery;
	public readonly Entity EcsWriteQuery;
	public readonly Entity EcsNoAccessQuery;
	public readonly Entity EcsSystem;
	public readonly Entity EcsProcessPhase;

	public ECS()
	{
		_entityManager = new();
		_componentManager = new(_entityManager);
		_queryManager = new(_entityManager);
		_systemManager = new(_entityManager);
		_objectManager = new(_entityManager);

		// register default components
		EcsWildcard = RegisterComponent<EcsWildcard>();
		EcsTag = RegisterComponent<EcsTag>();
		EcsComponent = RegisterComponent<EcsComponent>();
		EcsComponentConfig = RegisterComponent<EcsComponentConfig>();
		EcsObject = RegisterComponent<EcsObject>();
		EcsDisabled = RegisterComponent<EcsDisabled>();
		EcsQuery = RegisterComponent<EcsQuery>();
		EcsReadOnlyQuery = RegisterComponent<EcsReadOnlyQuery>();
		EcsReadWriteQuery = RegisterComponent<EcsReadWriteQuery>();
		EcsWriteQuery = RegisterComponent<EcsWriteQuery>();
		EcsNoAccessQuery = RegisterComponent<EcsNoAccessQuery>();
		EcsSystem = RegisterComponent<EcsSystem>();
		EcsProcessPhase = RegisterComponent<EcsProcessPhase>();

		// set the config
		SetConfig(new ECSConfig());

		// create system scheduler instance
		_systemScheduler = new(this, _systemManager, _queryManager);
	}


	/*********************
	*  Service methods  *
	*********************/

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoggerManager.LogDebug("ECS service node ready!");
	}

	// // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Update(delta);
	}

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
		LoggerManager.LogDebug("ECS service registered!");
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
		LoggerManager.LogDebug("ECS service deregistered!");
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
		LoggerManager.LogDebug("ECS service ready!");
	}
	
	public void SetConfig(ECSConfig config)
	{
		_config = config;

		// set the configured ID ranges
		_entityManager.SetIdRanges(_config.EntityIdRangeMin, _config.EntityIdRangeMax);

		_SetServiceReady(true);
	}

	/***********************
	 *  Entity management  *
	 ***********************/

	public EntityHandle Create(string name = "")
	{
		EntityHandle e = new EntityHandle(_entityManager.Create(name), this);

		// LoggerManager.LogDebug("Creating entity", e.ToString(), "name", name);

		_updateQueryResults(e.Entity);

		return e;
	}

	public string GetEntityName(Entity entity)
	{
		if (_entityManager.TryGetEntityName(entity, out string entityName))
		{
			return entityName;
		}

		return "";
	}

	public void SetEntityName(Entity entity, string name)
	{
		// set component entity name
		_entityManager.SetEntityName(entity, name);

		_updateQueryResults(entity);
	}

	public bool IsAlive(Entity entity, bool throwIfDead = false)
	{
		bool alive = _entityManager.IsAlive(entity);

		// throw an exception if throwIfDead is true
		if (throwIfDead && !alive) { throw new OperationOnDeadEntityException($"Entity is dead: {new EntityHandle(entity, this).ToString()}"); }

		return alive;
	}

	public void Destroy(Entity entity)
	{
		if (IsAlive(entity))
		{
			// LoggerManager.LogDebug("Destroying entity", new EntityHandle(entity, this).ToString());

			_entityManager.Destroy(entity);
			_componentManager.DestroyEntityComponents(entity);
		}
	}

	// set the archetype id state for an entity
	public void SetEntityArchetypeState(Entity entity, Entity id, bool state)
	{
		IsAlive(entity, throwIfDead:true);

		// LoggerManager.LogDebug("Setting archetype state", new EntityHandle(entity, this).ToString(), new EntityHandle(id, this).ToString(), state);

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
			if (_entityManager.HasArchetypeId(entity, id))
			{
				_entityManager.RemoveArchetypeId(entity, id);
			}
		}

		_updateQueryResults(entity);
	}

	// get the entity archetype state
	public bool GetEntityArchetypeState(Entity entity, Entity id)
	{
		return _entityManager.HasArchetypeId(entity, id);
	}

	// disable an archetype id for an entity
	public void DisableArchetype(Entity entity, Entity id)
	{
		_entityManager.DisableArchetype(entity, id);

		_updateQueryResults(entity);
	}

	// enable an archetype id for an entity
	public void EnableArchetype(Entity entity, Entity id)
	{
		_entityManager.EnableArchetype(entity, id);

		_updateQueryResults(entity);
	}

	public PackedArray<Entity> GetEntityArchetype(Entity entity)
	{
		return _entityManager.GetArchetype(entity);
	}

	/***********************
	*  Entity ID methods  *
	***********************/
	
	public EntityHandle EntityHandle<T>() where T : IComponent
	{
		Entity typeId = Id<T>();
		_entityManager.CreateArchetypeStorage(typeId);
		return new EntityHandle(typeId, this);
	}
	public EntityHandle EntityHandle(Entity entity)
	{
		_entityManager.CreateArchetypeStorage(entity);
		return new EntityHandle(entity, this);
	}

	public PackedArray<EntityHandle> EntityHandles(Entity[] entities)
	{
		PackedArray<EntityHandle> handles = new();
		foreach (Entity entity in entities)
		{
			handles.Add(EntityHandle(entity));
		}

		return handles;
	}

	/**********************************
	*  Component management methods  *
	**********************************/
	
	// create the needed type ID and component array
	public EntityHandle RegisterComponent<T>() where T : IComponent
	{
		Entity typeId = Entity.CreateFrom(_entityManager.Create(typeof(T).Name));

		T.Id = typeId.Id;

		_componentManager.CreateComponentArray<T>(typeId.Id);

		// LoggerManager.LogDebug("IsAlive", "", "isAlive", IsAlive(typeId));

		LoggerManager.LogDebug("Registering component", typeof(T).Name, "typeId", typeId);

		// _entityManager.CreateUnmanaged(typeId, typeof(T).Name);

		// if component doesn't implement ITag then it gets the
		// ComponentEntity component, allowing us to check later if the
		// component has this entity rather than the ITag interface
		if (typeof(T).GetInterface(nameof(ITag)) == null)
		{
			Add<EcsComponent>(typeId);
		}
		else
		{
			Add<EcsTag>(typeId);
		}

		// set component entity name
		_entityManager.SetEntityName(typeId, typeof(T).Name);

		_updateQueryResults(typeId);

		// return an entity handle for this component
		return EntityHandle(typeId);
	}

	// set the component config object on a component ID
	public void SetComponentConfig(Entity typeId, EcsComponentConfig componentConfig)
	{
		EntityHandle(typeId).Set<EcsComponentConfig>(componentConfig);
	}
	public ref EcsComponentConfig GetComponentConfig(Entity typeId)
	{
		// create a default one
		if (!Has<EcsComponentConfig>(typeId))
		{
			SetComponentConfig(typeId, new EcsComponentConfig());
		}

		return ref Get<EcsComponentConfig>(typeId);
	}

	// disable a component globally
	public void DisableComponent<T>()
	{
		throw new NotImplementedException("Disabling components is not implemented");
	}

	// enable a component globally
	public void EnableComponent<T>()
	{
		throw new NotImplementedException("Enabling components is not implemented");
	}

	// get a component array as an IComponentArray
	public IComponentArray GetComponentArray(Entity typeId)
	{
		return _componentManager.GetComponentArray(typeId);
	}

	/***************************
	 *  Add component methods  *
	 ***************************/

	// add an entity ID to an entity
	public void Add(Entity entity, Entity id)
	{
		// LoggerManager.LogDebug("Adding entity to entity", new EntityHandle(entity, this).ToString(), "entity", new EntityHandle(id, this).ToString());

		// set the archetype state
		SetEntityArchetypeState(entity, id, true);
	}


	// add a single ITag component to an entity
	public void Add<T>(Entity entity) where T : ITag, new()
	{
		// add entity ID for T to entity archetype
		Add(entity, Id<T>());
	}


	/***************************
	*  Set component methods  *
	***************************/

	// set component data on an entity
	public void Set<T>(Entity entity, Entity id, T component, bool isEntityId = true) where T : IComponentData
	{
		// LoggerManager.LogDebug("Setting component data", new EntityHandle(entity, this).ToString(), new EntityHandle(id, this).ToString(), component);

		// add entity ID to entity
		Add(entity, id);

		// set component data
		_componentManager.AddComponent<T>(entity, id, component);
	}


	// add a single component to an entity
	public void Set<T>(Entity entity, T component) where T : IComponentData
	{
		Set<T>(entity, Id<T>(), component, isEntityId:true);
	}


	/******************************
	 *  Remove component methods  *
	 ******************************/

	// remove an entity ID from an entity
	public void Remove(Entity entity, Entity id)
	{
		// LoggerManager.LogDebug("Removing entity from entity", new EntityHandle(entity, this).ToString(), "pair", new EntityHandle(id, this).ToString());

		// make sure entity is alive first
		IsAlive(entity, throwIfDead:true);

		// set the archetype on the entity
		SetEntityArchetypeState(entity, id, false);
	}
	public void Remove<T>(Entity entity, Entity id, bool isEntityId = true) where T : IComponent
	{
		// remove the component entity Id
		Remove(entity, id);

		// remove the component data if it has any
		if (Has<EcsComponent>(Id<T>()))
		{
			_componentManager.RemoveComponent<T>(entity, id);
		}
	}


	// remove component T from entity
	public void Remove<T>(Entity entity) where T : IComponent
	{
		Remove<T>(entity, Id<T>(), isEntityId:true);
	}


	/**************************************
	*  Enable/disable component methods  *
	**************************************/
	
	public void Disable<T>(Entity entity) where T : IComponent
	{
		DisableArchetype(entity, Id<T>());
	}

	public void Enable<T>(Entity entity) where T : IComponent
	{
		EnableArchetype(entity, Id<T>());
	}

	/***************************
	 *  Has component methods  *
	 ***************************/

	// check if an entity has component T
	public bool Has<T>(Entity entity) where T : IComponent
	{
		return Has(entity, Id<T>());
	}

	// check if entity has entity ID
	public bool Has(Entity entity, Entity id)
	{
		// check first if entity is alive
		IsAlive(entity, throwIfDead:true);

		return GetEntityArchetypeState(entity, id);
	}

	/***************************
	 *  Get component methods  *
	 ***************************/

	// get component T for entity
	public ref T Get<T>(Entity entity) where T : IComponentData
	{
		return ref _componentManager.GetComponent<T>(entity);
	}

	public ref T Get<T>(int typeId, Entity entity) where T : IComponentData
	{
		return ref _componentManager.GetComponent<T>(entity, typeId);
	}


	/****************
	 *  Id methods  *
	 ****************/

	// get entity ID for T
	public Entity Id<T>() where T : IComponent
	{
		// if it's got a 0 Id it hasn't been registered
		if (T.Id == 0)
		{
			Entity e = RegisterComponent<T>().Entity;
			LoggerManager.LogWarning("Registering dynamic component", "", typeof(T).Name, e);
			return e;
		}
		else
		{
			return Entity.CreateFrom(T.Id);
		}
	}


	/*******************
	*  Query methods  *
	*******************/

	// register a query object with an optional name
	public EntityHandle RegisterQuery(Query query)
	{
		return RegisterQuery(query, "");
	}
	public EntityHandle RegisterQuery(Query query, string name)
	{
		// create/register the query
		EntityHandle e = EntityHandle(_queryManager.RegisterQuery(query, name));

		// add query component
		e.Add<EcsQuery>();

		// run the query once to update the results
		Query(e.Entity);

		return e;
	}

	// create a QueryBuilder object
	public QueryBuilder CreateQuery()
	{
		return QueryBuilder.Create(this);
	}

	// run an on-demand query
	public QueryResult Query(Query query)
	{
		return _queryManager.RunQuery(query);
	}
	// run registered query by name
	public QueryResult Query(string name)
	{
		return _queryManager.RunQuery(name);
	}
	// run registered query by id
	public QueryResult Query(Entity entity)
	{
		return _queryManager.RunQuery(entity);
	}

	// get query results by name
	public QueryResult QueryResults(string name)
	{
		return _queryManager.QueryResults(name);
	}
	// get query results by id
	public QueryResult QueryResults(Entity entity)
	{
		return _queryManager.QueryResults(entity);
	}

	// update query results for an entity
	public void _updateQueryResults(Entity entity)
	{
		if ((_config != null && _config.KeepQueryResultsUpdated) || _config == null)
		{
			_queryManager.UpdateQueryResults(entity);
		}
	}

	/********************
	*  System methods  *
	********************/

	// register a system with a provided name and query entity ID
	public EntityHandle RegisterSystem<TSystem, TPhase>(string name, Entity queryId)
		where TSystem : ISystem, new()
		where TPhase : IEcsProcessPhase
	{
		EntityHandle e = EntityHandle(_systemManager.RegisterSystem<TSystem, TPhase>(name, queryId));

		// add components to entity
		e.Add<EcsSystem>();
		Add(e.Entity, Id<TPhase>());

		return e;
	}
	
	// register a system with a default name and no attached query
	public EntityHandle RegisterSystem<TSystem, TPhase>()
		where TSystem : ISystem, new()
		where TPhase : IEcsProcessPhase
	{
		return RegisterSystem<TSystem, TPhase>(String.Empty, default(Entity));
	}

	// register a system with a provided name, and no attached query
	public EntityHandle RegisterSystem<TSystem, TPhase>(string name)
		where TSystem : ISystem, new()
		where TPhase : IEcsProcessPhase
	{
		return RegisterSystem<TSystem, TPhase>(name, default(Entity));
	}

	// register a system with a default name, and an attached query entity ID
	public EntityHandle RegisterSystem<TSystem, TPhase>(Entity queryId)
		where TSystem : ISystem, new()
		where TPhase : IEcsProcessPhase
	{
		return RegisterSystem<TSystem, TPhase>(String.Empty, queryId);
	}

	// register a system with a provided name and query name
	public EntityHandle RegisterSystem<TSystem, TPhase>(string name, string queryName)
		where TSystem : ISystem, new()
		where TPhase : IEcsProcessPhase
	{
		return RegisterSystem<TSystem, TPhase>(name, _queryManager.GetQueryEntity(queryName));
	}

	// register a system with a provided name and query object
	public EntityHandle RegisterSystem<TSystem, TPhase>(string name, Query query)
		where TSystem : ISystem, new()
		where TPhase : IEcsProcessPhase
	{
		EntityHandle queryId = RegisterSystemQuery(query, $"system_{name}");
		return RegisterSystem<TSystem, TPhase>(name, queryId);
	}

	// register a system with query object
	public EntityHandle RegisterSystem<TSystem, TPhase>(Query query)
		where TSystem : ISystem, new()
		where TPhase : IEcsProcessPhase
	{
		string name = typeof(TSystem).Name;
		return RegisterSystem<TSystem, TPhase>(name, query);
	}

	// register a query which automatically ignores disabled entities
	public EntityHandle RegisterSystemQuery(Query query, string name)
	{
		EntityHandle e = RegisterQuery(CreateQuery()
				.Has(query)
				.Not<EcsDisabled>() // ignore disabled entities
				.Build(), name);

		return e;
	}

	// trigger an update of all the systems
	public void Update(double deltaTime)
	{
		_systemScheduler.Update(deltaTime);
	}


	/****************************
	*  Object manager methods  *
	****************************/

	public Entity RegisterObject<T>(T obj) where T : class
	{
		return _objectManager.RegisterObject<T>(obj);
	}

	public T GetObject<T>(Entity id) where T : class
	{
		return _objectManager.Get<T>(id);
	}

	public void DestroyObject(Entity id)
	{
		_objectManager.DeregisterObject(id.Id);
		Destroy(id);
	}
	
}
