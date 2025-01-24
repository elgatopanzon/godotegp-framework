/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SystemManager
 * @created     : Wednesday May 08, 2024 15:11:57 CST
 */

namespace GodotEGP.ECSv4.Systems;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Components;
using GodotEGP.ECSv4.Systems;

using System;
using System.Collections.Generic;

public partial class SystemManager
{
	private EntityManager _entityManager;

	// stores System objects by their entity ID
	private SystemInstance[] _systems;
	private int _dataSize;

	// stores a map of system names to entity IDs
	private Dictionary<string, Entity> _nameToSystemMap;

	public SystemManager(EntityManager entityManager)
	{
		_entityManager = entityManager;
		_systems = new SystemInstance[0];
		_nameToSystemMap = new();
	}

	/*****************************
	*  System registration  *
	*****************************/
	
	public Entity RegisterSystem<TSystem, TPhase>(string name, Entity queryEntity) 
		where TSystem : ISystem, new()
		where TPhase : IEcsProcessPhase
	{
		// assign default name if it's empty
		name = (name == String.Empty) ? $"s{typeof(TSystem).Name}" : name;

		// register an entity for the system
		Entity e = _entityManager.Create(name);

		// create a SystemInstance object for this system
		SystemInstance system = new SystemInstance() {
			System = new TSystem(),
			SystemEntity = e,
			QueryEntity = queryEntity,
		};

		LoggerManager.LogDebug("Registering system instance", "", "system", system);

		// add the registered system
		if (_dataSize <= e.Id + 1)
		{
			_dataSize = e.Id + 1;
			System.Array.Resize(ref _systems, _dataSize);
		}
		_systems[e.Id] = system;

		// add name to name map
		_nameToSystemMap.Add(name, e);

		return e;
	}

	/***********************
	*  System management  *
	***********************/
	
	// get a system instance by entity ID
	public SystemInstance GetSystemInstance(Entity entity)
	{
		return _systems[entity.Id];
		// if (_systems.TryGetValue(entity, out SystemInstance system))
		// {
		// 	return system;
		// }
        //
		// throw new ArgumentException($"No system matches '{entity}'");
	}
	// get a system instance by name
	public SystemInstance GetSystemInstance(string name)
	{
		if (_nameToSystemMap.TryGetValue(name, out Entity entity))
		{
			return GetSystemInstance(entity);
		}

		throw new ArgumentException($"No system matches '{entity}'");
	}

	// get all system instances
	public SystemInstance[] GetSystems()
	{
		return _systems;
	}
}
