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

public partial record ComponentStore<T> : IComponentStore where T : IComponent
{
	public EntityMap<T> Data;

	public ComponentStore()
	{
		Data = new();
	}

	public void Destroy(Entity entity)
	{
		Data[entity.Id] = default(T);
	}
}
public static class ComponentsExtensions
{
    public static void Add<T>(this ComponentStore<T> components, Entity entity, T component) where T : IComponent
    {
		components.Data.Add(entity, component);
    }

    public static bool Has<T>(this ComponentStore<T> components, Entity entity) where T : IComponent
    {
		return components.Data.Has(entity);
    }

    public static void Remove<T>(this ComponentStore<T> components, Entity entity) where T : IComponent
    {
		components.Data.Remove(entity);
    }

    public static T Get<T>(this ComponentStore<T> components, Entity entity) where T : IComponent
    {
		return components.Data.Get(entity);
    }

    public static ref T GetMutable<T>(this ComponentStore<T> components, Entity entity) where T : IComponent
    {
		return ref components.Data.GetRef(entity);
    }
}
