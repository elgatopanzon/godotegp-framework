/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentArray
 * @created     : Sunday Apr 28, 2024 16:46:17 CST
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

using System.Collections.Generic;
using System.Runtime.InteropServices;

public partial class ComponentArray<T> : IComponentArray where T : IComponent
{
	// backing data storage of components
	private IndexMap<T> _data;
	public IndexMap<T> Data
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
		_data.Set(entity.Id, component);
	}

	public void RemoveComponent(Entity entity)
	{
		// remove the provided key and value
		_data.Unset(entity.Id);
	}

	public bool HasComponent(Entity entity)
	{
		return _data.IndexOfData(entity.Id) != -1;
	}

	public ref T GetComponent(Entity entity)
	{
		// return ref CollectionsMarshal.GetValueRefOrAddDefault(_data, entity, out bool exists);
		return ref _data.GetRef(entity.Id);
	}

	public void DestroyComponents(Entity entity)
	{
		RemoveComponent(entity);
	}
}

