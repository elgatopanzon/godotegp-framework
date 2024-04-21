/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentArray
 * @created     : Sunday Apr 21, 2024 11:08:01 CST
 */

namespace GodotEGP.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECS.Exceptions;

public partial class ComponentArray<T>
{
	private PackedArray<T> _array;
	public PackedArray<T> Array
	{
		get { return _array; }
		set { _array = value; }
	}

	public ComponentArray(int maxSize = 32)
	{
		_array = new PackedArray<T>(maxSize);
	}

	public void InsertComponent(int entityId, T component)
	{
		if (_array.ContainsIndex(entityId))
		{
			throw new ComponentExistsException($"Component already exists for entity.");
		}

		_array.Insert(entityId, component);
	}

	public void RemoveComponent(int entityId)
	{
		if (!_array.ContainsIndex(entityId))
		{
			throw new ComponentNotFoundException($"Entity does not contain component.");
		}

		_array.RemoveAt(entityId);
	}

	public T GetComponent(int entityId)
	{
		if (!_array.ContainsIndex(entityId))
		{
			throw new ComponentNotFoundException($"Entity does not contain component.");
		}

		return _array.Get(entityId);
	}
}

