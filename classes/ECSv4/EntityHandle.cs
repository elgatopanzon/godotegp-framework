/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EntityHandle
 * @created     : Monday Apr 29, 2024 22:53:17 CST
 */

namespace GodotEGP.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4.Components;

public partial class EntityHandle
{
	private Entity _entity;
	public Entity Entity
	{
		get {
			return _entity;
		}
	}

	public string Name
	{
		get {
			return ToString();
		}
	}

	private ECS _core;

	public EntityHandle(Entity entity, ECS core)
	{
		_entity = entity;
		_core = core;
	}

	// allow implicit conversion from EntityHandle to Entity
	public static implicit operator Entity(EntityHandle entityHandle) => entityHandle.Entity.RawId;
	public static implicit operator int(EntityHandle entityHandle) => entityHandle.Entity.Id;

	public override string ToString()
	{
		string name = _core.GetEntityName(_entity);

		if (name.Length == 0)
		{
			name = $"e{_entity.RawId}";
		}
		name = $"{name} ({_entity.Id})";

		return name;
	}

	/*******************************
	*  Core ECS shortcut methods  *
	*******************************/

	public bool IsAlive()
	{
		return _core.IsAlive(_entity);
	}

	public void Destroy()
	{
		_core.Destroy(_entity);
	}
	
	/***********
	*  Add()  *
	***********/
	
	public EntityHandle Set<T>(T component) where T : IComponentData
	{
		_core.Set<T>(_entity, component);

		return this;
	}
	public EntityHandle Add<T>() where T : ITag, new()
	{
		_core.Add<T>(_entity);

		return this;
	}

	
	/**************
	*  Remove()  *
	**************/
	
	public EntityHandle Remove<T>() where T : IComponent
	{
		_core.Remove<T>(_entity);

		return this;
	}

	/***********
	*  Has()  *
	***********/
	
	public bool Has<T>() where T : IComponent
	{
		return _core.Has<T>(_entity);
	}


	/***********
	*  Get()  *
	***********/
	
	public ref T Get<T>() where T : IComponentData
	{
		return ref _core.Get<T>(_entity);
	}
}
