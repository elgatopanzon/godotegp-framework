/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EntityManager
 * @created     : Saturday Apr 27, 2024 20:51:37 CST
 */

namespace GodotEGP.ECSv2;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Collections.Generic;
using GodotEGP.Collections;

public partial class EntityManager
{
	// current count of used entities
	private ulong _entityCounter;

	// current count of alive entities
	private ulong _entityAliveCounter;

	// stack of recycled and re-usable entity IDs
	private Stack<ulong> _recycledEntities;
	private ulong _recycledEntityCount;

	// initial entity/component ID limit (grows based on requirements)
	private ulong _maxEntities;

	// archetypes for entity IDs, storing other entities
	private PackedDictionary<Entity, PackedArray<Entity>> _entityArchetypes;

	public EntityManager(ulong maxEntities = 100)
	{
		// create the instance of our stack
		_recycledEntities = new();

		// create the archetype storage
		_entityArchetypes = new();

		_maxEntities = maxEntities;
	}


	/**********************************
	*  Entity ID management methods  *
	**********************************/

	public Entity Create()
	{
		// increase alive entity counter
		_entityAliveCounter++;

		Entity entity;

		// use a recycled entity ID if we have any
		if (_recycledEntityCount > 0)
		{
			// pop a recycled entity
			entity = new Entity(_recycledEntities.Pop());
		}
		else
		{
			// increase global entity counter
			_entityCounter++;
			entity = new Entity(_entityCounter);
		}

		// init the archetype storage for this entity
		_resetArchetypeStorage(entity);

		return entity;
	}

	public void Destroy(Entity entity)
	{
		// remove archetype data for this ID
		_destoryArchetypeStorage(entity);

		// recycle the entity
		_recycle(entity);

		// decrease the alive entity count
		_entityAliveCounter--;
	}

	private void _recycle(Entity entity)
	{
		// increase the entity version count
		entity++;

		_recycledEntities.Push(entity);
		_recycledEntityCount++;
	}


	/**********************************
	*  Archetype management methods  *
	**********************************/
	
	private void _resetArchetypeStorage(Entity entity)
	{
		_entityArchetypes.Add(entity, new());
	}
	private void _destoryArchetypeStorage(Entity entity)
	{
		_entityArchetypes.Remove(entity);
	}

	public void AddArchetypeId(Entity entity, Entity id)
	{
		LoggerManager.LogDebug("Adding archetype id", "", entity.ToString(), id);

		_entityArchetypes[entity].Add(id);
	}
	public void RemoveArchetypeId(Entity entity, Entity id)
	{
		LoggerManager.LogDebug("Removing archetype id", "", entity.ToString(), id);
		
		_entityArchetypes[entity].Remove(id);
	}
	public bool HasArchetypeId(Entity entity, Entity id)
	{
		return _entityArchetypes[entity].Contains(id);
	}

	public PackedArray<Entity> GetArchetype(Entity entity)
	{
		LoggerManager.LogDebug("Getting archetype for entity", "", "entity", entity.ToString());

		return _entityArchetypes[entity];
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
}

