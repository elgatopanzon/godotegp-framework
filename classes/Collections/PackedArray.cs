/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : PackedArray
 * @created     : Saturday Apr 20, 2024 19:31:01 CST
 */

namespace GodotEGP.Collections;

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
	private double _growMultiplier;

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
	private int[] _dataToIndexMap;
	private int[] _indexToDataMap;

	// allow accessing indexes like regular array
	public T this[int index] {
		get {
			return Get(index);
		}
		set {
			Insert(index, value);
		}
	}

	public PackedArray(int maxSize = 1, double growMultiplier = 1.61803398874989484820458683436)
	{
		_maxSize = maxSize;
		_growMultiplier = growMultiplier;

		// init the data array, and the map arrays
		_array = new T[maxSize];
		_dataToIndexMap = new int[maxSize];
		_indexToDataMap = new int[maxSize];

		LoggerManager.LogDebug("Init packed array", typeof(T).Name, "size", maxSize);

		ClearDataIndexes();
	}

	public void ClearDataIndexes(int startFrom = 0)
	{
		LoggerManager.LogDebug("Clearing data indexes", typeof(T).Name);

		for (int i = startFrom; i < _maxSize; i++)
		{
			_dataToIndexMap[i] = -1;
			_indexToDataMap[i] = -1;
		}
	}

	public T Get(int index)
	{
		int dataIndex = _dataToIndexMap[index];
		return _array[dataIndex];
	}
	public ref T GetRef(int index)
	{
		int dataIndex = _dataToIndexMap[index];
		return ref _array[dataIndex];
	}

	// add value and grow array size
	public void Add(T value)
	{
		LoggerManager.LogDebug("Appending value", typeof(T).Name, _currentSize.ToString(), value);

		Insert(_currentSize, value);
	}

	public void Resize(int newSize)
	{
		System.Array.Resize<T>(ref _array, newSize);
		System.Array.Resize<int>(ref _dataToIndexMap, newSize);
		System.Array.Resize<int>(ref _indexToDataMap, newSize);

		LoggerManager.LogDebug("Resizing array", typeof(T).Name, "size", $"{_maxSize} => {newSize}");

		_maxSize = newSize;
	}

	public void Insert(int index, T value)
	{
		// resize the array to double size once it's full
		if (_currentSize >= _maxSize)
		{
			Resize(Convert.ToInt32(_maxSize * _growMultiplier));
			ClearDataIndexes(_currentSize);
		}

		LoggerManager.LogDebug("Inserting value", typeof(T).Name, index.ToString(), value);

		// add data to the end of the array at current size
		_array[_currentSize] = value;

		// add entries to index maps
		_dataToIndexMap[index] = _currentSize;
		_indexToDataMap[_currentSize] = index;

		LoggerManager.LogDebug("_dataToIndexMap", typeof(T).Name, "_dataToIndexMap", _indexToDataMap);
		LoggerManager.LogDebug("_indexToDataMap", typeof(T).Name, "_indexToDataMap", _dataToIndexMap);
		
		// increment current size
		_currentSize++;

		LoggerManager.LogDebug("_array", typeof(T).Name, "_array", _array);
	}

	// remove and keep array packed
	public void RemoveAt(int index)
	{
		LoggerManager.LogDebug("Removing index", typeof(T).Name, "index", index);

		// move the end element to the deleted element's position
		int lastElementIndex = _currentSize - 1;
		int removedElementIndex = _dataToIndexMap[index];

		LoggerManager.LogDebug("Last element index", typeof(T).Name, "lastElementIndex", lastElementIndex);
		LoggerManager.LogDebug("_array", typeof(T).Name, "_array", _array);
		LoggerManager.LogDebug("_dataToIndexMap", typeof(T).Name, "_dataToIndexMap", _indexToDataMap);
		LoggerManager.LogDebug("_indexToDataMap", typeof(T).Name, "_indexToDataMap", _dataToIndexMap);
		LoggerManager.LogDebug("_dataToIndexMap[index]", typeof(T).Name, "_dataToIndexMap[index]", _indexToDataMap[index]);
		LoggerManager.LogDebug("_array[lastElementIndex]", typeof(T).Name, "_array[lastElementIndex]", _array[lastElementIndex]);

		_array[removedElementIndex] = _array[lastElementIndex];

		// update the map of indexes to reflect the change
		int indexOfLastElement = _indexToDataMap[lastElementIndex];
		_dataToIndexMap[indexOfLastElement] = removedElementIndex;
		_indexToDataMap[removedElementIndex] = indexOfLastElement;

		// invalidate the old index mappings
		_dataToIndexMap[index] = -1;
		_indexToDataMap[lastElementIndex] = -1;

		// _array[_dataToIndexMap[index]] = _array[lastElementIndex];
        //
		// // update the map of indexes to reflect the change
		// _indexToDataMap[_dataToIndexMap[lastElementIndex]] = _dataToIndexMap[index];
		// _dataToIndexMap[_dataToIndexMap[index]] = _dataToIndexMap[lastElementIndex];
        //
		// // invalidate the old index mappings
		// _indexToDataMap[index] = -1;
		// _dataToIndexMap[lastElementIndex] = -1;

		// decrease array size
		_currentSize--;

		LoggerManager.LogDebug("_dataToIndexMap", typeof(T).Name, "_dataToIndexMap", _indexToDataMap);
		LoggerManager.LogDebug("_indexToDataMap", typeof(T).Name, "_indexToDataMap", _dataToIndexMap);

		LoggerManager.LogDebug("_array", typeof(T).Name, "_array", _array);
		LoggerManager.LogDebug("_indexToDataMap", typeof(T).Name, "_indexToDataMap", _dataToIndexMap);
	}

	public void Remove(T value)
	{
		LoggerManager.LogDebug("Removing value", typeof(T).Name, "value", value);

		int index = IndexOf(value);

		if (index != -1)
		{
			LoggerManager.LogDebug("Found value at real index", typeof(T).Name, "index", index);
			LoggerManager.LogDebug("Found value indexToData", typeof(T).Name, "indexToData", _indexToDataMap[index]);
			LoggerManager.LogDebug("Found value dataToIndex", typeof(T).Name, "dataToIndex", _dataToIndexMap[index]);

			LoggerManager.LogDebug("_dataToIndexMap", typeof(T).Name, "_dataToIndexMap", _indexToDataMap);
			LoggerManager.LogDebug("_indexToDataMap", typeof(T).Name, "_indexToDataMap", _dataToIndexMap);

			RemoveAt(_indexToDataMap[index]);
		}

		LoggerManager.LogDebug("_array", typeof(T).Name, "_array", _array);
		LoggerManager.LogDebug("_indexToDataMap", typeof(T).Name, "_indexToDataMap", _dataToIndexMap);
	}

	public bool ContainsIndex(int index)
	{
		return _dataToIndexMap[index] != -1;
	}

	public bool Contains(T value)
	{
		return (IndexOf(value) != -1);
	}

	public int IndexOf(T value)
	{
		for (int i = 0; i < _currentSize; i++)
		{
			if (_array[i].Equals(value))
			{
				return i;
			}
		}

		return -1;
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
                return _array[_dataToIndexMap[_enumeratorOrderedIndexMap[_enumeratorPosition]]];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }

    public void SetOrderedIndexMap()
    {
        _enumeratorOrderedIndexMap = _indexToDataMap.OrderBy(x => x).ToArray();
    }

    public IEnumerable<T> OrderedArray
    {
		get {
			return _indexToDataMap.Where((x) => x >= 0).OrderBy(x => x).Select<int, T>((x) => {
				return _array[_dataToIndexMap[x]];
			});
		}
    }
}

public partial class PackedArrayDictBacked<T> : IEnumerable, IEnumerator
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
		set {
			Insert(index, value);
		}
	}

	public PackedArrayDictBacked(int maxSize = 0)
	{
		_maxSize = maxSize;

		// init the data array, and the map arrays
		_array = new T[maxSize];
		_indexToDataMap = new(maxSize);
		_dataToIndexMap = new(maxSize);
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
		_indexToDataMap.Remove(index);
		_dataToIndexMap.Remove(lastElementIndex);

		// decrease array size
		_currentSize--;
	}

	IEnumerator IEnumerable.GetEnumerator()
    {
       return (IEnumerator) GetEnumerator();
    }

    public PackedArrayDictBacked<T> GetEnumerator()
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
			return _dataToIndexMap.Values.OrderBy(x => x).Select<int, T>((x) => {
				return _array[_indexToDataMap[x]];
			});
		}
    }
}

public partial class PackedArrayDictionary<T> : IEnumerable, IEnumerator
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
	private Dictionary<int, T> _dict;
	public IEnumerable<T> Array {
		get {
			return _dict.Values;
		}
	}

	// allow accessing indexes like regular array
	public T this[int index] {
		get {
			return Get(index);
		}
		set {
			Insert(index, value);
		}
	}

	public PackedArrayDictionary(int maxSize = 0)
	{
		_maxSize = maxSize;

		// init the data array, and the map arrays
		_dict = new(maxSize);
	}

	public T Get(int index)
	{
		return _dict[index];
	}
	public ref T GetRef(int index)
	{
		return ref _dict.Values.ToArray()[index];
	}

	// add value and grow array size
	public void Add(T value)
	{
		Insert(_currentSize, value);
	}

	public void Insert(int index, T value)
	{
		// add data to the end of the array at current size
		_dict[_currentSize] = value;

		// increment current size
		_currentSize++;
	}

	// remove and keep array packed
	public void RemoveAt(int index)
	{
		// move the end element to the deleted element's position
		_dict.Remove(index);

		// decrease array size
		_currentSize--;
	}

	public bool ContainsIndex(int index)
	{
		return _dict.ContainsKey(index);
	}

	IEnumerator IEnumerable.GetEnumerator()
    {
       return (IEnumerator) GetEnumerator();
    }

    public PackedArrayDictionary<T> GetEnumerator()
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
                return _dict[_enumeratorPosition];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }

    public void SetOrderedIndexMap()
    {
        // _enumeratorOrderedIndexMap = _dataToIndexMap.OrderBy(x => x).ToArray();
    }

    public IEnumerable<T> OrderedArray
    {
		get {
			// return _dataToIndexMap.Where((x) => x >= 0).OrderBy(x => x).Select<int, T>((x) => {
			// 	return _array[_indexToDataMap[x]];
			// });
			return _dict.Values;
		}
    }
}
