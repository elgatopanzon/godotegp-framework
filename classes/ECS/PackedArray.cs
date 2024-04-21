/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : PackedArray
 * @created     : Saturday Apr 20, 2024 19:31:01 CST
 */

namespace GodotEGP.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public partial class PackedArray<T> : IEnumerable, IEnumerator
{
	// max size the array can be
	private int _maxSize;

	// current size of the array
	private int _currentSize;
	public int Length {
		get {
			return _currentSize;
		}
	}
	public int Count {
		get {
			return Length;
		}
	}

	// private array of T
	private T[] _array;
	public T[] Array {
		get {
			return _array;
		}
	}

	// array of index mappings to underlying array indexes
	private Dictionary<int, int> _indexToDataMap;
	private Dictionary<int, int> _dataToIndexMap;

	// allow accessing indexes like regular array
	public T this[int index] {
		get {
			return Get(index);
		}
	}

	public PackedArray(int maxSize = 0)
	{
		_maxSize = maxSize;

		// init the data array, and the map arrays
		_array = new T[maxSize];
		_indexToDataMap = new(maxSize);
		_dataToIndexMap = new(maxSize);

		for (int i = 0; i < maxSize; i++)
		{
			_indexToDataMap[i] = -1;
			_dataToIndexMap[i] = -1;
		}
	}

	public T Get(int index)
	{
		int dataIndex = _indexToDataMap[index];
		return _array[dataIndex];
	}

	// add value and grow array size
	public void Add(T value)
	{
		Insert(_currentSize, value);
	}

	public void Insert(int index, T value)
	{
		// add data to the end of the array at current size
		_array[_currentSize] = value;

		// add entries to index maps
		_indexToDataMap[index] = _currentSize;
		_dataToIndexMap[_currentSize] = index;
		
		// increment current size
		_currentSize++;
	}

	// remove and keep array packed
	public void RemoveAt(int index)
	{
		// move the end element to the deleted element's position
		int lastElementIndex = _currentSize - 1;
		_array[_dataToIndexMap[index]] = _array[lastElementIndex];

		// update the map of indexes to reflect the change
		_indexToDataMap[_dataToIndexMap[lastElementIndex]] = _dataToIndexMap[index];
		_dataToIndexMap[_dataToIndexMap[index]] = _dataToIndexMap[lastElementIndex];

		// invalidate the old index mappings
		_indexToDataMap[index] = -1;
		_dataToIndexMap[lastElementIndex] = -1;

		// decrease array size
		_currentSize--;
	}

	IEnumerator IEnumerable.GetEnumerator()
    {
       return (IEnumerator) GetEnumerator();
    }

    public PackedArray<T> GetEnumerator()
    {
        return this;
    }

	// Enumerators are positioned before the first element
    // until the first MoveNext() call.
    int _enumeratorPosition = -1;
    int[] _enumeratorOrderedIndexMap;

    public bool MoveNext()
    {
        _enumeratorPosition++;
        return (_enumeratorPosition < _currentSize);
    }

    public void Reset()
    {
        _enumeratorPosition = -1;
        SetOrderedIndexMap();
    }

    object IEnumerator.Current
    {
        get
        {
            return Current;
        }
    }

    public T Current
    {
        get
        {
            try
            {
        		if (_enumeratorOrderedIndexMap == null)
        		{
        			SetOrderedIndexMap();
        		}
                return _array[_indexToDataMap[_enumeratorOrderedIndexMap[_enumeratorPosition]]];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }

    public void SetOrderedIndexMap()
    {
        _enumeratorOrderedIndexMap = _dataToIndexMap.Values.OrderBy(x => x).ToArray();
    }

    public IEnumerable<T> OrderedArray
    {
		get {
			return _dataToIndexMap.Values.Where((x) => x >= 0).OrderBy(x => x).Select<int, T>((x) => {
				return _array[_indexToDataMap[x]];
			});
		}
    }
}