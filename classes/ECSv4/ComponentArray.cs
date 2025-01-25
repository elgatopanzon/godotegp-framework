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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public partial class ComponentArray<T> : IComponentArray where T : IComponent
{
	// backing data storage of components
	public T[] Data;

	public ComponentArray()
	{
		Data = new T[0];
	}

	public void InsertComponent(Entity entity, T component)
	{
		// add the component to the data store
		// it will be overwritten if it already exists
		if (Data.Length <= entity.Id + 1)
		{
			System.Array.Resize(ref Data, entity.Id + 1);
		}
		Data[entity.Id] = component;
	}

	public void RemoveComponent(Entity entity)
	{
		// remove the provided key and value
		Data[entity.Id] = default(T);
	}

	public bool HasComponent(Entity entity)
	{
		return Data.Length >= entity.Id + 1;
	}

	public ref T GetComponent(Entity entity)
	{
		return ref Data[entity.Id];
	}

	public void DestroyComponents(Entity entity)
	{
		RemoveComponent(entity);
	}
}
