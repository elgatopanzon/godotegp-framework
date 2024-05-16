/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IndexMap
 * @created     : Saturday May 11, 2024 17:49:10 CST
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

public partial class IndexMap<T>
{
	// size of the underlying data array
	private int _dataSizeCurrent;
	private int _dataSizeMax;

	public int Length
	{
		get {
			return _dataSizeCurrent;
		}
	}
	public int Count {
		get {
			return Length;
		}
	}

	// size of the data index
	private int _dataIndexSizeCurrent;

	// underlying data array
	private T[] _array;
	internal T[] RawArray {
		get {
			return _array;
		}
	}
	public Span<T> Span {
		get {
			return _array.AsSpan().Slice(0, _dataSizeCurrent);
		}
	}
	// provide the Span named Values, like a Dictionary
	public Span<T> Values {
		get {
			return Span;
		}
	}

	// array segment instance, updated when changed
	private ArraySegment<T> _arraySegment;
	public ArraySegment<T> ArraySegment {
		get {
			return _arraySegment;
		}
	}

	// holds int index to array indexes
	private int[] _dataToIndexMap;

	// holds array indexes to int indexes
	private int[] _indexToDataMap;

	public T this[int index] {
		get {
			return _array[_dataToIndexMap[index]];
		}
		set {
			Set(index, value);
		}
	}
	public T this[uint index] {
		get {
			return _array[_dataToIndexMap[index]];
		}
		set {
			Set(index, value);
		}
	}

	public IndexMap(int maxSize = 0, double growMultiplier = 1.61803398874989484820458683436)
	{
		_array = new T[maxSize];
		_indexToDataMap = new int[maxSize];
		_dataToIndexMap = new int[_dataIndexSizeCurrent];

		_dataSizeMax = maxSize;

		ClearIndexMap(0);
		ClearDataIndexMap(0);
		CreateArraySegment();
	}

	public void CreateArraySegment()
	{
		_arraySegment = new ArraySegment<T>(_array, 0, _dataSizeCurrent);
	}

	// similar method to dictionary's Add()
	public void Add(int index, T value)
	{
		Set(index, value);
	}

	// set the value for the given index
	public void Set(uint index, T value)
	{
		Set((int) index, value);
	}
	public void Set(int index, T value)
	{
		int insertDataIndex = _dataSizeCurrent;

		LoggerManager.LogDebug("Setting item at index", "", "index", index);

		// check if the current index exists so we can simply replace it
		int indexOfData = IndexOfData(index);
		if (indexOfData != -1)
		{
			LoggerManager.LogDebug("Index already exists in map", "", "index", index);

			insertDataIndex = indexOfData;
		}
		else
		{
			LoggerManager.LogDebug("Index doesn't exists in map", "", "index", index);

			// first grow data index if needed
			GrowDataIndex(index);

			// grow data to allow inserting
			GrowData();

			// increase the size, since we'll be inserting the new item
			_dataSizeCurrent++;
		}

		LoggerManager.LogDebug("Inserting data at real index", "", "index", insertDataIndex);

		// add/set the data
		_array[insertDataIndex] = value;

		// update the index maps
		_dataToIndexMap[index] = insertDataIndex;
		_indexToDataMap[insertDataIndex] = index;
	}

	// get the value for the given index
	// public T Get(int index)
	// {
	// 	return _array[_dataToIndexMap[index]];
	// }
	public ref T GetRef(int index)
	{
		return ref _array[_dataToIndexMap[index]];
	}

	// public T Get(uint index)
	// {
	// 	return Get((int) index);
	// }
	public ref T GetRef(uint index)
	{
		return ref GetRef((int) index);
	}

	// remove the data at the given index
	public bool Unset(uint index)
	{
		return Unset((int) index);
	}
	public bool Unset(int index)
	{
		if (IndexOfData(index) != -1)
		{
			LoggerManager.LogDebug("Unsetting value at index", "", "index", index);

			// move the end element to the deleted element's position
			int lastElementIndex = _dataSizeCurrent - 1;
			int removedElementIndex = _dataToIndexMap[index];

			_array[removedElementIndex] = _array[lastElementIndex];

			// update the map of indexes to reflect the change
			int indexOfLastElement = _indexToDataMap[lastElementIndex];
			_dataToIndexMap[indexOfLastElement] = removedElementIndex;
			_indexToDataMap[removedElementIndex] = indexOfLastElement;

			// invalidate the old index mappings
			_dataToIndexMap[index] = -1;
			_indexToDataMap[lastElementIndex] = -1;

			// decrease array managed size
			_dataSizeCurrent--;

			return true;
		}

		return false;
	}

	public bool TryGetValue(int index, out T value)
	{
		if (IndexOfData(index) != -1)
		{
			value = this[index];
			return true;
		}

		value = default(T);
		return false;
	}

	/******************************
	*  Array management methods  *
	******************************/

	// get the index of the real data value
	public int IndexOfData(uint index)
	{
		return IndexOfData((int) index);
	}
	public int IndexOfData(int index)
	{
		if (_dataIndexSizeCurrent > index)
		{
			return _dataToIndexMap[index];
		}

		return -1;
	}

	// grow the underlying data array by the defined grow size
	public void GrowData()
	{
		if (_dataSizeCurrent >= _dataSizeMax)
		{
			ResizeData(Convert.ToInt32(Math.Max(1, _dataSizeMax) * 1.6));
			ClearIndexMap(_dataSizeCurrent);
		}
	}
	// grow the data index to the required index size
	public void GrowDataIndex(int index)
	{
		if (index >= _dataIndexSizeCurrent)
		{
			int newSize = Convert.ToInt32(Math.Max(_dataIndexSizeCurrent, index + 1));
			int oldSize = _dataIndexSizeCurrent;
			ResizeDataIndex(newSize);
			ClearDataIndexMap(oldSize);
		}
	}

	// resize the underlying data and index map
	public void ResizeData(int newSize)
	{
		System.Array.Resize<T>(ref _array, newSize);
		System.Array.Resize<int>(ref _indexToDataMap, newSize);

		LoggerManager.LogDebug("Resizing data", typeof(T).Name, "size", $"{_dataSizeMax} => {newSize}");

		_dataSizeMax = newSize;
	}
	// resize the data index map
	public void ResizeDataIndex(int newSize)
	{
		System.Array.Resize<int>(ref _dataToIndexMap, newSize);

		LoggerManager.LogDebug("Resizing data index", typeof(T).Name, "size", $"{_dataIndexSizeCurrent} => {newSize}");

		_dataIndexSizeCurrent = newSize;
	}

	// mark the extra indexes as invalid
	public void ClearIndexMap(int startFrom = 0)
	{
		LoggerManager.LogDebug("Clearing index map", typeof(T).Name, "from", startFrom);

		for (int i = startFrom; i < _dataSizeMax; i++)
		{
			_indexToDataMap[i] = -1;
		}
	}
	public void ClearDataIndexMap(int startFrom = 0)
	{
		LoggerManager.LogDebug("Clearing data index map", typeof(T).Name, "from", startFrom);

		for (int i = startFrom; i < _dataIndexSizeCurrent; i++)
		{
			_dataToIndexMap[i] = -1;
		}
	}
}
