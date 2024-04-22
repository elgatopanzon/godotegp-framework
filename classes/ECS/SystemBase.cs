/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : System
 * @created     : Sunday Apr 21, 2024 16:20:30 CST
 */

namespace GodotEGP.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Collections.Generic;

public partial class SystemBase
{
	// sorted set of entity IDs this system processes
	private SortedSet<int> _entities;
	public SortedSet<int> Entities
	{
		get {
			return _entities;
		}
	}

	public SystemBase()
	{
		_entities = new();
	}

	public void EraseEntity(int entityId)
	{
		_entities.Remove(entityId);
	}

	public void AddEntity(int entityId)
	{
		_entities.Add(entityId);
	}
}
