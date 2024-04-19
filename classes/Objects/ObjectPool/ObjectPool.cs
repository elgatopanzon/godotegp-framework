namespace GodotEGP.Objects.ObjectPool;

using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

using GodotEGP.Logging;

public partial class ObjectPool<T> where T: class
{
	private ConcurrentStack<T> _objects;
	private int _capacityInitial;
	private int _capacityMax;

	public ObjectPool(int capacityInitial = 0, int capacityMax = 100)
	{
		_capacityMax = capacityMax;
		_capacityInitial = capacityInitial;

		_objects = new ConcurrentStack<T>();

		for (int i = 0; i < capacityInitial; i++)
		{
			Return(Get());
		}
	}

	public T Get(params object[] p)
	{
		if (_objects.TryPop(out T obj))
		{
			return obj;
		}
		else
		{
			return (T) Activator.CreateInstance(typeof(T), p);
		}
	}

	public void Return(T obj)
	{
		if (_objects.Count < _capacityMax)
		{
			// LoggerManager.LogDebug("Returning instance", typeof(T).Name);

			_objects.Push(obj);
		}
	}
}

public interface IObjectPoolHandler 
{

	public object Take(object instance, params object[] p);
	public object Return(object instance);
};

public interface IObjectPoolHandler<T> : IObjectPoolHandler
{
	public T OnTake(T instance, params object[] p);
	public T OnReturn(T instance);
}

public partial class ObjectPoolHandler<T>: IObjectPoolHandler<T> where T : IPoolableObject
{
	public virtual T OnTake(T instance, params object[] p)
	{
		return instance;
	}
	public virtual T OnReturn(T instance)
	{
		return instance;
	}

	public object Take(object instance, params object[] p)
	{
		return (object) OnTake((T) instance, p);
	}
	public object Return(object instance)
	{
		return (object) OnReturn((T) instance);
	}
}
