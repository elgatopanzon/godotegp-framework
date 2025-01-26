/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentManager
 * @created     : Monday Apr 29, 2024 20:51:51 CST
 */

namespace GodotEGP.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4.Components;
using GodotEGP.Collections;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ComponentTypeId = Entity;

public partial record struct ComponentDatabase
{
	public EntityMap<IComponentStore> Components;

	public ComponentDatabase()
	{
		Components = new();
	}

}
public static class ComponentDatabaseExtensions
{
	// create a component array for a type ID as type T
	public static IComponentStore CreateStore<T>(this ComponentDatabase components, int typeId) where T : IComponent
	{
		if (!components.Components.Has(Entity.CreateFrom(typeId)))
		{
			IComponentStore componentStore = new ComponentStore<T>();
			components.Components.Add(Entity.CreateFrom(typeId), componentStore);
			return componentStore;
		}

		return components.Components.Get(Entity.CreateFrom(typeId));
	}
	public static IComponentStore CreateStore<T>(this ComponentDatabase components) where T : IComponent
	{
		return components.CreateStore<T>(T.Id);
	}

	// get component array for type ID
	public static ComponentStore<T> GetStore<T>(this ComponentDatabase components, int typeId) where T : IComponent
	{
		return Unsafe.As<ComponentStore<T>>(components.Components.Get(Entity.CreateFrom(typeId)));
	}

	// get component array for type without ID
	public static ComponentStore<T> GetStore<T>(this ComponentDatabase components) where T : IComponent
	{
		return components.GetStore<T>(T.Id);
	}

	// get component array for type ID as IComponentStore
	public static IComponentStore GetStore(this ComponentDatabase components, int typeId)
	{
		return components.Components.Get(Entity.CreateFrom(typeId));
	}

	// destroy all of the components for a given entity
    public static void DestroyComponents(this ComponentDatabase components, Entity entity)
    {
    	foreach (var componentStore in components.Components.Data)
    	{
    		if (componentStore != null)
    		{
				componentStore.Destroy(entity);    		
    		}
    	}
    }
}
