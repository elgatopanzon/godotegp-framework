/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SystemManager
 * @created     : Sunday Apr 21, 2024 16:27:18 CST
 */

namespace GodotEGP.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Collections;
using System.Collections.Generic;

using GodotEGP.ECS.Exceptions;

public partial class SystemManager
{
	// max number of systems
	private int _maxSystems;

	// dictionary of registered systems by type
	private Dictionary<Type, SystemBase> _systems;

	// dictionary of system archetypes as bitarray
	private Dictionary<Type, BitArray> _systemArchetypes;

	public SystemManager(int maxSystems = 32)
	{
		_maxSystems = maxSystems;

		_systems = new(maxSystems);
		_systemArchetypes = new(maxSystems);
	}

	public SystemBase RegisterSystem<T>() where T : SystemBase, new()
	{
		Type systemType = typeof(T);

		if (!_systems.TryGetValue(systemType, out SystemBase system))
		{
			// create new instance of system
			_systems[systemType] = new T();

			return _systems[systemType];
		}

		throw new SystemAlreadyRegisteredException($"The system has already been registered.");
	}

	public void SetSystemArchetype<T>(BitArray archetype) where T : SystemBase
	{
		Type systemType = typeof(T);

		if (!_systems.ContainsKey(systemType))
		{
			throw new SystemNotRegisteredException($"The system is not registered.");
		}

		_systemArchetypes[systemType] = archetype;
	}

	public void DestroyEntity(int entityId)
	{
		foreach (SystemBase system in _systems.Values)
		{
			system.EraseEntity(entityId);
		}
	}

	public void UpdateEntityArchetype(int entityId, BitArray archetype)
	{
		// update all system's entity lists
		foreach (SystemBase system in _systems.Values)
		{
			BitArray systemArchetype = _systemArchetypes[system.GetType()];

			// if bitarray AND operation matches system array then this entity
			// is processed by this system
			if (ArchetypeMatches(archetype, systemArchetype))
			{
				system.AddEntity(entityId);
			}
			else
			{
				system.EraseEntity(entityId);
			}
		}
	}

	public bool ArchetypeMatches(BitArray archetype1, BitArray archetype2)
	{
		return !(((BitArray)archetype1.Clone()).And(archetype2).HasAnySet()) == archetype2.HasAllSet();
	}
}