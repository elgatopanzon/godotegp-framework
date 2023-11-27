namespace GodotEGP.Service;

using Godot;
using System;
using System.Collections.Generic;

using GodotEGP.Logging;
using GodotEGP.Objects;

public partial class ObjectPoolService : Service
{
	private Dictionary<Type, object> _pools = new Dictionary<Type, object>();

	private Dictionary<Type, Dictionary<string, int>> _poolsConfig = new Dictionary<Type, Dictionary<string, int>>();

	public override void _Ready()
	{
		_SetServiceReady(true);
	}

	/// <summary>
	/// Get an object of type <c>T</c> from an instance of
	/// <c>ObjectPool<T></c>.
	/// If no pool for <c>T</c> exists, creates one.
	/// </summary>
	public T Get<T>() where T: class
    {
        return GetPoolInstance<T>().Get();
    }

    /// <summary>
    /// Return an object to the ObjectPool if the pool exists
    /// </summary>
    public void Return<T>(T obj) where T: class
    {
		GetPoolInstance<T>().Return(obj);
    }

	/// <summary>
	/// Get or create an <c>ObjectPool</c> for <c>T</c>.
	/// </summary>
    public ObjectPool<T> GetPoolInstance<T>() where T: class
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

    public Dictionary<string, int> GetPoolConfig<T>()
    {
        if (!_poolsConfig.TryGetValue(typeof(T), out var obj) || obj is not Dictionary<string, int> config)
        {
        	SetPoolConfig<T>();
        	config = GetPoolConfig<T>();
        }

        return config;
    }

    public void SetPoolConfig<T>(int capacityInitial = 0, int capacityMax = 100)
    {
    	_poolsConfig.Add(typeof(T), new Dictionary<string, int>
		{
			{ "capacityInitial", capacityInitial },
			{ "capacityMax", capacityMax }
		});
    }
}
