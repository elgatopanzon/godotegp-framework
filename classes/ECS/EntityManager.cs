/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EntityManager
 * @created     : Saturday Apr 20, 2024 18:46:03 CST
 */

namespace GodotEGP.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECS.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections;

public partial class EntityManager
{
	// max amount of active entities
	private int _maxEntities;

	// max amount of components per entity
	private int _maxComponents;

	// current count of active entities
	private int _activeEntityCount;

	// queue of the available entity IDs we can use
	private Stack<int> _availableEntities;

	// array of entities Archetypes
	private BitArray[] _entityArchetypes;

	public EntityManager(int maxEntities = 5000, int maxComponents = 32)
	{
		_maxEntities = maxEntities;
		_maxComponents = maxComponents;

		// init the available entities queue with the max entities
		_availableEntities = new(maxEntities);

		// init the Archetypes bit array
		_entityArchetypes = new BitArray[maxEntities];
		for (int entityId = maxEntities - 1; entityId >= 0; entityId--)
		{
    		_entityArchetypes[entityId] = new BitArray(maxComponents);
		}

		// add all available entities to the queue
		for (int entityId = maxEntities - 1; entityId >= 0; entityId--)
		{
			_availableEntities.Push(entityId);
		}
	}

	public int Create()
	{
		// check we still have capacity to create the entity
		if (_activeEntityCount == _maxEntities)
		{
			throw new MaxEntitiesReachedException($"Max number of entities already active ({_activeEntityCount}).");
		}

		// pop an available entity ID from the stack
		int entityId = _availableEntities.Peek();
		_availableEntities.Pop();

		// increase current active entity counter
		_activeEntityCount++;

		return entityId;
	}

	public void Destroy(int entityId)
	{
		// check we provided a valid entity ID
		if (entityId >= _maxEntities || entityId < 0)
		{
			throw new ArgumentOutOfRangeException($"Entity ID out of range.");
		}

		// reset the archetype for this entity
		_entityArchetypes[entityId] = new BitArray(_maxComponents);

		// return the entity to the available entities list
		_availableEntities.Push(entityId);

		// decrease the active entity counter
		_activeEntityCount--;
	}

	public void SetArchetype(int entityId, BitArray archetype)
	{
		// check we provided a valid entity ID
		if (entityId >= _maxEntities || entityId < 0)
		{
			throw new ArgumentOutOfRangeException($"Entity ID out of range.");
		}

		// set the archetype for this entity
		_entityArchetypes[entityId] = archetype;
	}

	public BitArray GetArchetype(int entityId)
	{
		// check we provided a valid entity ID
		if (entityId >= _maxEntities || entityId < 0)
		{
			throw new ArgumentOutOfRangeException($"Entity ID out of range.");
		}

		return _entityArchetypes[entityId];
	}
}

