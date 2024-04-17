/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableStreamBuffer
 * @created     : Friday Mar 29, 2024 23:16:28 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Chainables.Exceptions;

using System;
using System.Linq;
using System.Collections.Generic;


public partial class ChainableStreamBuffer
{
	public List<object> _objects { get; set; } = new();
	internal Type ObjectType { 
		get {
			return _objects.FirstOrDefault().GetType();
		}
	}

	public void Add(object obj)
	{
		_objects.Add(obj);
	}

	public object Merge()
	{
		if (_objects.Count == 0)
		{
			throw new ChainableStreamBufferMergeEmpty("There are no objects to merge!");
		}

		// make sure all objects in the list are of the same type
		ValidateObjectTypes();

		dynamic first = _objects.First();

		// attempt to merge the object via addition operator
		try
		{
			foreach (var obj in _objects.Skip(1))
			{
				dynamic o = obj;
				first += o;
			}
		}

		// if it fails, try to merge it as a collection
		catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
		{
			bool didProcessArray = false;
			foreach (dynamic obj in _objects.Skip(1))
			{
				foreach (var listItem in obj)
				{
					// first try to add item if its a simple list
					try
					{
						first.Add(listItem);
					}

					// if we catch an exception, try to merge it as a dictionary
					catch (System.Exception)
					{
						try
						{
							first[listItem.Key] = listItem.Value;
						}

						// if we still catch an exception try to re-create the array
						catch (System.Exception)
						{
							try
							{
								if (!didProcessArray)
								{
									dynamic firstBk = first;
									first = new List<object>();

									foreach (var item in firstBk)
									{
										first.Add(item);
									}
								}
								first.Add(listItem);
							}

							// throw if we got this far (not array, not list,
							// not dictionary)
							catch
							{
								throw;
							}
						}
					}
				}
			}
		}
		catch (System.Exception e)
		{
			throw new ChainableStreamBufferMergeNotSupported($"Merging objects of type {ObjectType.Name} is not supported", e);
		}

		// return the merged object
		return first;
	}

	public void ValidateObjectTypes()
	{
		foreach (var obj in _objects)
		{
			if (obj.GetType() != ObjectType)
			{
				throw new ChainableStreamBufferMismatchingTypesException();
			}
		}
	}
}
