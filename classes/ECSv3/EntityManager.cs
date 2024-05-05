/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EntityManager
 * @created     : Saturday Apr 27, 2024 20:51:37 CST
 */

namespace GodotEGP.ECSv3;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using GodotEGP.Collections;

using GodotEGP.ECSv3.Components;

public partial class EntityManager
{
	// current count of used entities
	private ulong _entityCounter;

	// current count of alive entities
	private ulong _entityAliveCounter;

	// stack of recycled and re-usable entity IDs
	private Stack<ulong> _recycledEntities;
	private ulong _recycledEntityCount;

	// ID generation ranges
	private uint _idRangeMin;
	private uint _idRangeMax;

	// archetypes for entity IDs, storing other entities
	private PackedDictionary<Entity, PackedArray<Entity>> _entityArchetypes;
	private PackedDictionary<Entity, PackedArray<Entity>> _entityDisabledArchetypes;

	// entity name storage
	private PackedDictionary<Entity, string> _entityToNameMap;
	private PackedDictionary<string, Entity> _nameToEntityMap;

	public EntityManager()
	{
		// create the instance of our stack
		_recycledEntities = new();

		// create the archetype storage
		_entityArchetypes = new();
		_entityDisabledArchetypes = new();

		// create the entity name maps
		_entityToNameMap = new();
		_nameToEntityMap = new();
	}

	public Span<Entity> GetEntities()
	{
		return _entityArchetypes.Keys.AsSpan();
	}

	/**********************************
	*  Entity ID management methods  *
	**********************************/

	public void SetIdRanges(uint rangeMin, uint rangeMax = 0)
	{
		_idRangeMin = rangeMin;
		_idRangeMax = rangeMax;
	}

	public Entity Create(string name = "")
	{
		Entity entity;

		// try to get an existing named entity and return it without creating a
		// copy
		if (name.Length > 0 && TryGetNameEntity(name, out Entity e))
		{
			entity = e;
			return e;
		}

		// use a recycled entity ID if we have any
		if (_recycledEntityCount > 0)
		{
			// pop a recycled entity
			entity = CreateUnmanaged(_recycledEntities.Pop(), name);
		}
		else
		{
			// check if we'd generate an ID outside of max range
			if (_idRangeMax > 0 && _entityCounter >= _idRangeMax)
			{
				throw new ArgumentException("Max entities exist!");
			}

			entity = CreateUnmanaged(_entityCounter + _idRangeMin, name);

			// increase global entity counter
			Interlocked.Increment(ref _entityCounter);
		}

		return entity;
	}

	public Entity CreateUnmanaged(Entity entity, string name = "")
	{
		// init the archetype storage for this entity
		_resetArchetypeStorage(entity);

		// increase alive entity counter
		Interlocked.Increment(ref _entityAliveCounter);

		// set entity name if set
		if (name.Length > 0)
		{
			SetEntityName(entity, name);
		}

		return entity;
	}

	public bool IsAlive(Entity entity)
	{
		return _hasArchetypeStorage(entity);
	}

	public void Destroy(Entity entity)
	{
		// remove archetype data for this ID
		_destoryArchetypeStorage(entity);

		// remove entity name if it has one
		if (EntityHasName(entity))
		{
			RemoveEntityName(entity);
		}

		// recycle the entity
		_recycle(entity);

		// decrease the alive entity count
		_entityAliveCounter--;
		Interlocked.Decrement(ref _entityAliveCounter);
	}

	private void _recycle(Entity entity)
	{
		// increase the entity version count
		entity++;

		_recycledEntities.Push(entity);
		Interlocked.Increment(ref _recycledEntityCount);
	}


	/**********************************
	*  Archetype management methods  *
	**********************************/
	
	private void _resetArchetypeStorage(Entity entity)
	{
		_entityArchetypes.Add(entity, new());
		_entityDisabledArchetypes.Add(entity, new());
	}
	private void _destoryArchetypeStorage(Entity entity)
	{
		_entityArchetypes.Remove(entity);
		_entityDisabledArchetypes.Remove(entity);
	}
	public bool _hasArchetypeStorage(Entity entity)
	{
		return _entityArchetypes.ContainsKey(entity);
	}
	public void CreateArchetypeStorage(Entity entity)
	{
		if (!_hasArchetypeStorage(entity))
		{
			_resetArchetypeStorage(entity);
		}
	}

	public void AddArchetypeId(Entity entity, Entity id)
	{
		LoggerManager.LogDebug("Adding archetype id", "", entity.ToString(), id.RawId);
		LoggerManager.LogDebug("Archetype", "", entity.ToString(), _entityArchetypes[entity].Array);

		_entityArchetypes[entity].Add(id);

		LoggerManager.LogDebug("Archetype", "", entity.ToString(), _entityArchetypes[entity].Array);
	}
	public void RemoveArchetypeId(Entity entity, Entity id)
	{
		LoggerManager.LogDebug("Removing archetype id", "", entity.ToString(), id.RawId);
		LoggerManager.LogDebug("Archetype", "", entity.ToString(), _entityArchetypes[entity].Array);
		
		_entityArchetypes[entity].Remove(id);

		LoggerManager.LogDebug("Archetype", "", entity.ToString(), _entityArchetypes[entity].Array);
	}
	public bool HasArchetypeId(Entity entity, Entity id)
	{
		return (_entityArchetypes[entity].Contains(id));
	}

	public PackedArray<Entity> GetArchetype(Entity entity)
	{
		return _entityArchetypes[entity];
	}

	public void DisableArchetype(Entity entity, Entity id)
	{
		if (!_entityDisabledArchetypes[entity].Contains(id))
		{
			_entityArchetypes[entity].Remove(id);
			_entityDisabledArchetypes[entity].Add(id);
		}
	}

	public void EnableArchetype(Entity entity, Entity id)
	{
		_entityDisabledArchetypes[entity].Remove(id);
		_entityArchetypes[entity].Add(id);
	}

	public PackedDictionary<Entity, PackedArray<Entity>> GetArchetypes()
	{
		return _entityArchetypes;
	}

	/********************************
	*  Entity ID encoding methods  *
	********************************/

	// get the left-side ID from a ulong ID
	public uint GetEncodedId(ulong id)
	{
		return (uint) id;
	}
	
	// get the entity version from a ulong ID
	public ushort GetEncodedVersion(ulong id)
	{
		return (ushort) (id >> 32);
	}

	// set the entity version on a ulong ID
	public ulong SetEncodedVersion(ulong id, ushort version)
	{
		return (ulong) ((ulong) version << 32) | GetEncodedId(id);
	}

	// increment the entity version on a ulong ID
	public ulong IncrementVersion(ulong id)
	{
		return SetEncodedVersion(id, (ushort) (GetEncodedVersion(id) + (ushort) 1));
	}

	// set the right side pair ID on a ulong ID
	public ulong SetPairId(ulong id, ulong idLeft)
	{
		return (ulong) ((ulong) GetEncodedId(idLeft) << 32) | GetEncodedId(id);
	}

	// get the right side pair ID of a ulong ID
	public ulong GetPairId(ulong id)
	{
		return (uint) (id >> 32);
	}


	/************************************
	*  Entity name management methods  *
	************************************/
	
	public void SetEntityName(Entity entity, string name)
	{
		_nameToEntityMap[name] = entity.Id;
		_entityToNameMap[entity.Id] = name;
	}

	public void RemoveEntityName(Entity entity)
	{
		_nameToEntityMap.Remove(_entityToNameMap[entity.Id]);
		_entityToNameMap.Remove(entity.Id);
	}

	public bool EntityHasName(Entity entity)
	{
		return _entityToNameMap.ContainsKey(entity.Id);
	}

	public bool EntityNameExists(string name)
	{
		return _nameToEntityMap.ContainsKey(name);
	}

	public bool TryGetEntityName(Entity entity, out string entityName)
	{
		if (_entityToNameMap.TryGetValue(entity.Id, out string name))
		{
			entityName = name;
			return true;
		}

		entityName = default(string);
		return false;
	}

	public bool TryGetNameEntity(string name, out Entity entityId)
	{
		if (_nameToEntityMap.TryGetValue(name, out Entity entity))
		{
			entityId = entity;
			return true;
		}

		entityId = default(Entity);
		return false;
	}
}
