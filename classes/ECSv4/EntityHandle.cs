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
	public static implicit operator uint(EntityHandle entityHandle) => entityHandle.Entity.Id;

	public override string ToString()
	{
		string name = _core.GetEntityName(_entity);

		if (name.Length == 0)
		{
			name = $"e{_entity.RawId}";
		}
		name = $"{name} ({_entity.Id})";


		// add entity ID info
		if (_entity.PairId > 0)
		{
			string pairName = _core.GetEntityName(_entity.PairId);
			if (pairName.Length == 0)
			{
				pairName = $"e{_entity.PairId}";
			}
			else {
				pairName = $"{pairName} ({_entity.PairId})";
			}
			name += $", {pairName}";
		}

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

	public EntityHandle Set<TSource, TTarget>(TSource sourceComponent) 
		where TSource : IComponentData
		where TTarget : ITag
	{
		_core.Set<TSource, TTarget>(_entity, sourceComponent);

		return this;
	}
	public EntityHandle Set<TSource, TTarget>(TTarget targetComponent) 
		where TSource : ITag
		where TTarget : IComponentData
	{
		_core.Set<TSource, TTarget>(_entity, targetComponent);

		return this;
	}
	public EntityHandle Add<TSource, TTarget>() 
		where TSource : ITag
		where TTarget : ITag
	{
		_core.Add<TSource, TTarget>(_entity);

		return this;
	}

	public EntityHandle Set<T>(Entity targetEntity, T component) where T : IComponentData
	{
		_core.Set<T>(_entity, targetEntity, component);

		return this;
	}

	public EntityHandle Add<T>(Entity targetEntity) where T : ITag
	{
		_core.Add<T>(_entity, targetEntity);

		return this;
	}

	public EntityHandle Add(Entity entitySource, Entity entityTarget)
	{
		_core.Add(_entity, entitySource, entityTarget);

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

	public EntityHandle Remove<TSource, TTarget>() 
		where TSource : IComponent
		where TTarget : IComponent
	{
		_core.Remove<TSource, TTarget>(_entity);

		return this;
	}

	public EntityHandle Remove<T>(Entity entitySource) where T : IComponent
	{
		_core.Remove<T>(_entity, entitySource);

		return this;
	}

	public EntityHandle Remove(Entity entitySource, Entity entityTarget)
	{
		_core.Remove(_entity, entitySource, entityTarget);

		return this;
	}

	/***********
	*  Has()  *
	***********/
	
	public bool Has<T>() where T : IComponent
	{
		return _core.Has<T>(_entity);
	}

	public bool Has<TSource, TTarget>() 
		where TSource : IComponent
		where TTarget : IComponent
	{
		return _core.Has<TSource, TTarget>(_entity);
	}
	
	public bool Has<T>(Entity entitySource) where T : IComponent
	{
		return _core.Has<T>(_entity, entitySource);
	}

	public bool Has(Entity entitySource, Entity entityTarget)
	{
		return _core.Has(_entity, entitySource, entityTarget);
	}


	/***********
	*  Get()  *
	***********/
	
	public ref T Get<T>() where T : IComponentData
	{
		return ref _core.Get<T>(_entity);
	}

	public ref TData Get<TSource, TData>() 
		where TSource : ITag
		where TData : IComponentData
	{
		return ref _core.Get<TSource, TData>(_entity);
	}

	public ref T Get<T>(Entity entity) where T : IComponentData
	{
		return ref _core.Get<T>(_entity, entity);
	}
}
