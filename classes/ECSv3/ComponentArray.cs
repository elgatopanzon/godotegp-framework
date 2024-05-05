/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentArray
 * @created     : Sunday Apr 28, 2024 16:46:17 CST
 */

namespace GodotEGP.ECSv3;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Collections;
using GodotEGP.ECSv3.Components;

public partial class ComponentArray<T> : IComponentArray where T : IComponent
{
	// backing data storage of components
	private PackedDictionary<Entity, T> _data;
	public PackedDictionary<Entity, T> Data
	{
		get { return _data; }
	}

	public ComponentArray()
	{
		_data = new();
	}

	public void InsertComponent(Entity entity, T component)
	{
		// add the component to the data store
		// it will be overwritten if it already exists
		_data.Insert(entity, component);
	}

	public void RemoveComponent(Entity entity)
	{
		// remove the provided key and value
		_data.Remove(entity);
	}

	public bool HasComponent(Entity entity)
	{
		return _data.ContainsKey(entity);
	}

	public ref T GetComponent(Entity entity)
	{
		return ref _data.GetRef(entity);
	}

	public void DestroyComponents(Entity entity)
	{
		RemoveComponent(entity);
	}
}

