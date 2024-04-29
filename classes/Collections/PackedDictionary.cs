/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : PackedDictionary
 * @created     : Sunday Apr 28, 2024 11:45:30 CST
 */

namespace GodotEGP.Collections;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Collections.Generic;

public partial class PackedDictionary<TKey, TValue>
{
	// max size this dictionary can be (no growing)
	private int _maxSize;

	// current size of the dictionary
	private int _currentSize;

	// data stores for keys and values
	private PackedArray<TKey> _keys;
	private PackedArray<TValue> _values;

	// expose Keys and Values properties
	public TKey[] Keys {
		get {
			return _keys.Array;
		}
	}
	public TValue[] Values {
		get {
			return _values.Array;
		}
	}

	// allow accessing indexes like regular dictionary
	public TValue this[TKey key] {
		get {
			return Get(key);
		}
		set {
			Insert(key, value);
		}
	}

	public PackedDictionary(int maxSize = 0)
	{
		_maxSize = maxSize;

		_keys = new(32);
		_values = new(32);
	}

	public TValue Get(TKey key)
	{
		return _values[GetIndex(key)];
	}
	public ref TValue GetRef(TKey key)
	{
		return ref _values.GetRef(GetIndex(key));
	}

	public int GetIndex(TKey key)
	{
		return System.Array.IndexOf(_keys.Array, key);
	}

	public void Add(TKey key, TValue value)
	{
		Insert(key, value);
	}

	public void Insert(TKey key, TValue value)
	{
		int keyIndex = GetIndex(key);

		if (keyIndex == -1)
		{
			keyIndex = _currentSize;
			_currentSize++;
		}

		// add the key and value to the backing arrays
		_keys.Insert(keyIndex, key);
		_values.Insert(keyIndex, value);
	}

	public void Remove(TKey key)
	{
		int keyIndex = GetIndex(key);

		// remove the key and value from the backing store
		_keys.RemoveAt(keyIndex);
		_values.RemoveAt(keyIndex);

		_currentSize--;
	}

	public bool ContainsKey(TKey key)
	{
		return (GetIndex(key) != -1);
	}

	public bool TryGetValueRef(TKey key, out TValue value)
	{
		value = default(TValue);

		int foundIndex = GetIndex(key);

		if (foundIndex != -1)
		{
			value = _values[foundIndex];
			return true;
		}

		return false;
	}
}

