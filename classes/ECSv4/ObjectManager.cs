/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ObjectManager
 * @created     : Monday May 20, 2024 11:09:45 CST
 */

namespace GodotEGP.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using System;
using System.Runtime.CompilerServices;

public partial class ObjectManager
{
	private EntityManager _entityManager;

	// map of object instances
	private IndexMap<object> _objects;
	public IndexMap<object> Objects
	{
		get {
			return _objects;
		}
	}

	// current size of active objects array
	private int _objectsSize;

	public ObjectManager(EntityManager entityManager)
	{
		_entityManager = entityManager;
		_objects = new();
	}


	/*****************************
	*  Register object methods  *
	*****************************/

	// internal register command to add an object to the store
	protected Entity _register(object obj, string name)
	{
		Entity id = _entityManager.Create();

		// set name if we have a name
		if (name != String.Empty)
		{
			_entityManager.SetEntityName(id, $"{name}_{id}");
		}

		_objects[id] = obj;

		return id;
	}

	// register an instance of T
	public Entity RegisterObject<T>(T obj) where T : class
	{
		return _register(obj, $"{typeof(T).Name}");
	}

	// register and create a new instance of T
	public Entity RegisterObject<T>() where T : class, new()
	{
		return _register(new T(), $"{typeof(T).Name}");
	}

	// register an object without the use of T
	public Entity RegisterObject(object obj)
	{
		return _register(obj, $"object");
	}


	/********************************
	*  De-register object methods  *
	********************************/

	protected void _deregister(int id)
	{
		_objects[id] = null;
		_objects.Unset(id);
	}

	// deregister an object by ID
	public void DeregisterObject(int id)
	{
		_deregister(id);
	}


	/************************
	*  Get object methods  *
	************************/
	
	public T Get<T>(Entity id) where T : class
	{
		return Unsafe.As<T>(_objects[id]);
	}
}

