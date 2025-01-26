/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EntityMap
 * @created     : Saturday Jan 25, 2025 18:10:23 CST
 */

namespace GodotEGP.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class EntityMap<T>
{
	public T[] Data;

	public T this[int index]
	{
		get { return Data[index]; }
	}
	
	public EntityMap(int length = 0)
	{
		Data = new T[length];
	}
}
public static class EntityMapExtensions
{
    public static void Add<T>(this EntityMap<T> map, Entity entity, T item)
    {
		if (map.Data.Length <= entity.Id + 1)
		{
			System.Array.Resize(ref map.Data, entity.Id + 1);
		}
		map.Data[entity.Id] = item;
    }

    public static bool Has<T>(this EntityMap<T> map, Entity entity)
    {
		return map.Data.Length >= entity.Id + 1;
    }

    public static void Remove<T>(this EntityMap<T> map, Entity entity)
    {
    	if (map.Has(entity))
    	{
			map.Data[entity.Id] = default(T);
    	}
    }

    public static T Get<T>(this EntityMap<T> components, Entity entity)
    {
		return components.Data[entity.Id];
    }

    public static ref T GetRef<T>(this EntityMap<T> map, Entity entity)
    {
		return ref map.Data[entity.Id];
    }
}
