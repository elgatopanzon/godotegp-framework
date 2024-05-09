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

using System;
using System.Collections.Generic;
using System.Linq;

// public partial class PackedDictionaryReal<TKey, TValue>
// {
// 	// max size this dictionary can be (no growing)
// 	private int _maxSize;
//
// 	// current size of the dictionary
// 	private int _currentSize;
//
// 	// data stores for keys and values
// 	private PackedArray<TKey> _keys;
// 	private PackedArray<TValue> _values;
//
// 	// expose Keys and Values properties
// 	public Span<TKey> Keys {
// 		get {
// 			return _keys.Span;
// 		}
// 	}
// 	public Span<TValue> Values {
// 		get {
// 			return _values.Span;
// 		}
// 	}
// 	public ReadOnlySpan<TKey> KeysSegment {
// 		get {
// 			return _keys.ArraySegment;
// 		}
// 	}
// 	public ReadOnlySpan<TValue> ValuesSegment {
// 		get {
// 			return _values.ArraySegment;
// 		}
// 	}
// 	public PackedArray<TKey> PKeys {
// 		get {
// 			return _keys;
// 		}
// 	}
// 	public PackedArray<TValue> PValues {
// 		get {
// 			return _values;
// 		}
// 	}
//
// 	// allow accessing indexes like regular dictionary
// 	public TValue this[TKey key] {
// 		get {
// 			return Get(key);
// 		}
// 		set {
// 			Insert(key, value);
// 		}
// 	}
//
// 	public PackedDictionaryReal(int maxSize = 0)
// 	{
// 		_maxSize = maxSize;
//
// 		_keys = new();
// 		_values = new();
// 	}
//
// 	public TValue Get(TKey key)
// 	{
// 		return _values[GetIndex(key)];
// 	}
// 	public ref TValue GetRef(TKey key)
// 	{
// 		return ref _values.GetRef(GetIndex(key));
// 	}
//
// 	public int GetIndex(TKey key)
// 	{
// 		return _keys.IndexOf(key);
// 	}
//
// 	public void Add(TKey key, TValue value)
// 	{
// 		Insert(key, value);
// 	}
//
// 	public void Insert(TKey key, TValue value)
// 	{
// 		int keyIndex = GetIndex(key);
//
// 		if (keyIndex == -1)
// 		{
// 			keyIndex = _currentSize;
// 			_currentSize++;
// 		}
//
// 		// add the key and value to the backing arrays
// 		_keys.Insert(keyIndex, key);
// 		_values.Insert(keyIndex, value);
// 	}
//
// 	public void Remove(TKey key)
// 	{
// 		int keyIndex = GetIndex(key);
//
// 		if (keyIndex != -1)
// 		{
// 			// remove the key and value from the backing store
// 			_keys.RemoveAt(keyIndex);
// 			_values.RemoveAt(keyIndex);
//
// 			_currentSize--;
// 		}
// 	}
//
// 	public bool ContainsKey(TKey key)
// 	{
// 		return (GetIndex(key) != -1);
// 	}
//
// 	public bool TryGetValue(TKey key, out TValue value)
// 	{
// 		value = default(TValue);
//
// 		int foundIndex = GetIndex(key);
//
// 		if (foundIndex != -1)
// 		{
// 			value = _values[foundIndex];
// 			return true;
// 		}
//
// 		return false;
// 	}
// }

public partial class PackedDictionary<TKey, TValue>
{
	// max size this dictionary can be (no growing)
	private int _maxSize;

	// size of buckets
	private int _bucketSize;

	// current size of the dictionary
	private PackedArray<int> _currentSize;

	// data stores for keys and values
	private PackedArray<PackedArray<TKey>> _keys;
	private PackedArray<PackedArray<TValue>> _values;

	// expose Keys and Values properties
	internal TKey[] Keys {
		get {
			return JoinBuckets<TKey>(_keys);
		}
	}
	internal TValue[] Values {
		get {
			return JoinBuckets<TValue>(_values);
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

	public PackedDictionary(int maxSize = 0, int bucketSize = 10)
	{
		_maxSize = maxSize;
		_bucketSize = bucketSize;

		_keys = new();
		_values = new();
		_currentSize = new();

		// init buckets and bucket sizes
		for (int i = 0; i < bucketSize; i++)
		{
			_keys[i] = new();
			_values[i] = new();
			_currentSize[i] = 0;
		}
	}

	public T[] JoinBuckets<T>(PackedArray<PackedArray<T>> buckets)
	{
		T[] joined = new T[buckets.Span.ToArray().Sum(a => a.Count)];
		int position = 0;
		foreach (var a in buckets.Span)
		{
			Array.Copy(a.Span.ToArray(), 0, joined, position, a.Count);
			position += a.Count;
		}

		return joined;
	}

	public TValue Get(TKey key)
	{
		int bucket = GetBucket(key);

		return _values[bucket][GetIndex(key, bucket)];
	}
	public ref TValue GetRef(TKey key)
	{
		int bucket = GetBucket(key);
		int index = GetIndex(key, bucket);

		return ref _values[bucket].GetRef(GetIndex(key, bucket));
	}

	public int GetIndex(TKey key, int bucket)
	{
		return System.Array.IndexOf(_keys[bucket].RawArray, key);
	}

	public int GetBucket(TKey key)
	{
		uint hashCode = (uint)key.GetHashCode();
		uint bucketCount = (uint) _bucketSize;
		uint maxSize = 10;
		ulong multiplier = ulong.MaxValue / maxSize + 1;
        uint highbits = (uint)(((((multiplier * hashCode) >> 32) + 1) * bucketCount) >> 32);

        int bucket = (int) highbits;

        return bucket;
	}

	public void Add(TKey key, TValue value)
	{
		Insert(key, value);
	}

	public void Insert(TKey key, TValue value)
	{
		int bucket = GetBucket(key);
		int keyIndex = GetIndex(key, bucket);

		if (keyIndex == -1)
		{
			keyIndex = _currentSize[bucket];
			_currentSize[bucket]++;
		}

		// add the key and value to the backing arrays
		_keys[bucket].Insert(keyIndex, key);
		_values[bucket].Insert(keyIndex, value);
	}

	public void Remove(TKey key)
	{
		int bucket = GetBucket(key);
		int keyIndex = GetIndex(key, bucket);

		// remove the key and value from the backing store
		_keys[bucket].RemoveAt(keyIndex);
		_values[bucket].RemoveAt(keyIndex);

		_currentSize[bucket]--;
	}

	public bool ContainsKey(TKey key)
	{
		int bucket = GetBucket(key);
		
		return (GetIndex(key, bucket) != -1);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		value = default(TValue);

		int bucket = GetBucket(key);
		int foundIndex = GetIndex(key, bucket);

		if (foundIndex != -1)
		{
			value = _values[GetBucket(key)][foundIndex];
			return true;
		}

		return false;
	}
}

