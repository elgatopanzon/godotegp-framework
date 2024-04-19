namespace GodotEGP.Service;

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

using GodotEGP.Logging;
using GodotEGP.Objects.ObjectPool;

public partial class ObjectPoolService : Service
{
	private Dictionary<Type, object> _pools = new Dictionary<Type, object>();

	private Dictionary<Type, Dictionary<string, int>> _poolsConfig = new Dictionary<Type, Dictionary<string, int>>();

	private Dictionary<Type, IObjectPoolHandler> _poolHandlers = new Dictionary<Type, IObjectPoolHandler>();

	public ObjectPoolService()
	{
		var type = typeof(IObjectPoolHandler);
		var objectPoolHandlers = AppDomain.CurrentDomain.GetAssemblies()
    		.SelectMany(s => s.GetTypes())
    		.Where(p => type.IsAssignableFrom(p) && p.IsClass && p != typeof(ObjectPoolHandler<>));

    	foreach (var handler in objectPoolHandlers)
    	{
    		Type handlerType = handler.GetInterfaces()[0].GetGenericArguments()[0];
    		LoggerManager.LogDebug("Creating pool handler instance", "", handlerType.Name, handler);

    		_poolHandlers.Add(handlerType, (IObjectPoolHandler) Activator.CreateInstance(handler));
    	}
	}

	public override void _Ready()
	{
		_SetServiceReady(true);
	}

	/// <summary>
	/// Get an object of type <c>T</c> from an instance of
	/// <c>ObjectPool<T></c>.
	/// If no pool for <c>T</c> exists, creates one.
	/// </summary>
	public T Get<T>(params object[] p) where T: class
    {
        T instance = GetPoolInstance<T>().Get(p);
        TryOnTakeObjectFromPool(instance, p);

        return instance;
    }

    /// <summary>
    /// Return an object to the ObjectPool if the pool exists
    /// </summary>
    public void Return<T>(T obj) where T: class
    {
    	TryOnReturnObjectToPool(obj);
		GetPoolInstance<T>().Return(obj);
    }

	/// <summary>
	/// Get or create an <c>ObjectPool</c> for <c>T</c>.
	/// </summary>
    public ObjectPool<T> GetPoolInstance<T>() where T: class
    {
    	lock(_pools)
    	{
        	if (!_pools.TryGetValue(typeof(T), out var obj) || obj is not ObjectPool<T> pool)
        	{
            	var poolConfig = GetPoolConfig<T>();

            	pool = new ObjectPool<T>(poolConfig["capacityInitial"], poolConfig["capacityMax"]);
            	_pools.Add(typeof(T), pool);

				LoggerManager.LogDebug($"Creating pool", "", "pool", typeof(T));
        	}

        	return pool;
        }
    }

    public Dictionary<string, int> GetPoolConfig<T>()
    {
    	lock(_poolsConfig)
    	{
        	if (!_poolsConfig.TryGetValue(typeof(T), out var obj) || obj is not Dictionary<string, int> config)
        	{
        		SetPoolConfig<T>();
        		config = GetPoolConfig<T>();
        	}

        	return config;
        }
    }

    public void SetPoolConfig<T>(int capacityInitial = 0, int capacityMax = 100)
    {
    	_poolsConfig.Add(typeof(T), new Dictionary<string, int>
		{
			{ "capacityInitial", capacityInitial },
			{ "capacityMax", capacityMax }
		});
    }

    public bool TryOnReturnObjectToPool<T>(T obj)
    {
    	if (TryGetPoolHandler<T>(out IObjectPoolHandler handler))
    	{
    		handler.Return(obj);

    		return true;
    	}

    	return false;
    }

    public bool TryOnTakeObjectFromPool<T>(T obj, params object[] p)
    {
    	if (TryGetPoolHandler<T>(out IObjectPoolHandler handler))
    	{
    		handler.Take(obj, p);

    		return true;
    	}

    	return false;
    }

    public bool TryGetPoolHandler<T>(out IObjectPoolHandler handler)
    {
    	handler = null;

		foreach (var poolHandler in _poolHandlers)
		{
			if (typeof(T).IsSubclassOf(poolHandler.Key) || typeof(T) == poolHandler.Key)
			{
				handler = poolHandler.Value;

				return true;
			}			
		}

    	return false;
    }
}
