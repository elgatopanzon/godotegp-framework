/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Extensions
 * @created     : Wednesday Mar 27, 2024 14:56:42 CST
 */

namespace GodotEGP.Chainables.Extensions;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public static partial class ChainableExtensions
{
	public static T Bind<T>(this T chainable, string paramName, object value) where T : IChainable
	{
		var newChainable = chainable.Clone();

		newChainable.Config.Params[paramName] = value;

		return (T) newChainable;
	}

	public static T Config<T>(this T chainable,
			bool mergeCollections = true,
			List<string> tags = null,
			Dictionary<string, object> metadata = null, 
			string runName = "", int maxConcurrency = 0, 
			Dictionary<string, ChainableConfigurableParam> configurableParams = null, 
			Dictionary<string, object> runParams = null
			) where T : IChainable
	{
		var newChainable = chainable.Clone();

		newChainable.Config.Merge(new ChainableConfig() {
			Tags = (tags == null) ? new() : tags,
			Metadata = (metadata == null) ? new() : metadata,
			RunName = runName,
			MaxConcurrency = maxConcurrency,
			ConfigurableParams = (configurableParams == null) ? new() : configurableParams,
			Params = (runParams == null) ? new() : runParams,
			}, mergeCollections:mergeCollections);

		return (T) newChainable;
	}

	public static T ConfigurableParams<T>(this T chainable, string methodParamName, ChainableConfigurableParam configurableParam) where T : IChainable
	{
		return chainable.Config(mergeCollections: true, configurableParams:new() { { methodParamName, configurableParam } });
	}

	public static T Config<T>(this T chainable, ChainableConfig config) where T : IChainable
	{
		return chainable.Config(tags:config.Tags, metadata:config.Metadata, runName:config.RunName, maxConcurrency:config.MaxConcurrency, configurableParams:config.ConfigurableParams, runParams:config.Params);
	}

	public static void Merge<T>(this T config, ChainableConfig merge, bool mergeCollections = true) where T : ChainableConfig
	{
		// if mergeCollections is true, then we merge the config
		// otherwise we overwrite the config items which have been set
		if (mergeCollections)
		{
			config.Tags.Merge(merge.Tags);
			config.Metadata.Merge(merge.Metadata);
			config.ConfigurableParams.Merge(merge.ConfigurableParams);
			config.Params.Merge(merge.Params);
		}
		else
		{
			config.Tags = merge.Tags;
			config.Metadata = merge.Metadata;
			config.ConfigurableParams = merge.ConfigurableParams;
			config.Params = merge.Params;
		}

		if (config.MaxConcurrency != merge.MaxConcurrency)
		{
			config.MaxConcurrency = merge.MaxConcurrency;
		}
		if (config.RunName != merge.RunName)
		{
			config.RunName = merge.RunName;
		}
	}

	public static T Param<T>(this T chainable, string paramName, object paramValue) where T : IChainable
	{
		return chainable.Config(mergeCollections: true, runParams:new() {{ paramName, paramValue }});
	}

	public static T Assign<T>(this T chainable, string key, object value) where T : IChainableAssign
	{
		var clone = (T) chainable.Clone();

		clone.AssignedOutputs[key] = value;

		return clone;
	}

	public static T Pick<T>(this T chainable, string key) where T : IChainable
	{
		var clone = (T) chainable.Clone();

		clone.Stack.Add(chainable);
		clone.Stack.Add(new ChainableValueGetter() { Key = key });

		return clone;
	}

	public static T Pick<T>(this T chainable, string[] keys) where T : IChainable
	{
		var clone = (T) chainable.Clone();

		clone.Stack.Add(chainable);
		clone.Stack.Add(new ChainablePick(keys));

		return clone;
	}

	public static ChainableFallback WithFallback<T>(this T chainable, IChainable fallback) where T : IChainable
	{
		if (chainable is ChainableFallback cf)
		{
			cf.Fallbacks.Add(fallback);
			return cf;
		}

		// if we're not dealing with a ChainableFallback, wrap the current
		// chainable into one
		var withFallback = new ChainableFallback() {
			Chainable = chainable,
			Fallbacks = new() { fallback },
		};

		// clone config from original chainable
		withFallback.Config = chainable.Config.Clone();

		return withFallback;
	}

	public static ChainableRetry WithRetry<T>(this T chainable, int maxRetries = 3) where T : IChainable
	{
		if (chainable is ChainableRetry cr)
		{
			cr.MaxRetries = maxRetries;
			return cr;
		}

		// if we're not dealing with a ChainableFallback, wrap the current
		// chainable into one
		var withFallback = new ChainableRetry() {
			Chainable = chainable,
		 	MaxRetries = maxRetries
		};

		// clone config from original chainable
		withFallback.Config = chainable.Config.Clone();

		return withFallback;
	}


	// coerce and return the provided object into TOut
	public static TOut Coerce<TOut>(this object obj)
	{
		// if the type is the same, just return a casted version of our object
		if (obj.GetType() == typeof(TOut))
		{
			return (TOut) obj;
		}

		LoggerManager.LogDebug("Attempting to coerce type", "", obj.GetType().ToString(), typeof(TOut));

		try
		{
			if (obj.TryCoerce<TOut>(out var coerced))
			{
				LoggerManager.LogDebug("Successfully coerced type", "", obj.GetType().ToString(), typeof(TOut));

				return coerced;
			}
			else
			{
				throw new NotImplementedException($"No reasonable coercion for '{obj.GetType()}' => '{typeof(TOut)}'!");
			}
		}
		catch (System.Exception)
		{
			throw;
		}
	}

	// attempt coerce any object into the provided TOut type
	public static bool TryCoerce<TOut>(this object obj, out TOut coerced)
	{
		coerced = default(TOut);

		// try to cast the object first
		if (obj.TryCast<TOut>(out TOut casted))
		{
			coerced = casted;
			return true;
		}

		// try to use converter
		if (obj.TryConvert<TOut>(out TOut converted))
		{
			coerced = converted;
			return true;
		}

		// if the type is an array, list or dictionary prepare the generic types
		// for coercing the correct collection type
		var genericTypesIn = obj.GetType().GetGenericArguments();
		var genericTypesOut = typeof(TOut).GetGenericArguments();

		if (obj.GetType().IsArray)
		{
			genericTypesIn = new Type[] { obj.GetType().GetElementType() };
			genericTypesOut = new Type[] { typeof(TOut).GetElementType() };
		}

		foreach (var collectionCoerce in new List<(string InterfaceName, string MethodName)>() {
				("Array", nameof(ChainableExtensions.CoerceArray)),
				("IList", nameof(ChainableExtensions.CoerceList)),
				("IDictionary", nameof(ChainableExtensions.CoerceDictionary)),
			})
		{
			if (obj.GetType().GetInterfaces().Any(x => x.Name == collectionCoerce.InterfaceName) || (obj.GetType().IsArray && collectionCoerce.InterfaceName == "Array"))
			{
				MethodInfo method = typeof(ChainableExtensions).GetMethod(collectionCoerce.MethodName);
				MethodInfo generic = method.MakeGenericMethod(genericTypesIn.Concat(genericTypesOut).ToArray());
				coerced = (dynamic) generic.Invoke(null, new object[] { obj });

				return true;
			}
		}

		return false;
	}


	// attempt to convert the provided object to TOut using IConvertable
	public static bool TryConvert<TOut>(this object obj, out TOut converted)
	{
		converted = default(TOut);
		try
		{
			converted = (TOut) Convert.ChangeType(obj, typeof(TOut));
			return true;
		}
		catch (System.Exception e)
		{
			return false;
		}
	}

	// attempt to coerce an IList<T> into IList<TOut>
	public static IList<TOut> CoerceList<T, TOut>(this IList<T> obj)
	{
		LoggerManager.LogDebug("Attempting to coerce IList", "", obj.GetType().ToString(), typeof(TOut));

		return obj.AsEnumerable().ToList().ConvertAll(s => s.Coerce<TOut>());
	}

	// attempt to coerce an IDictionary<TKey, TValue> into IDictionary<TOutKey, TOutValue>
	public static IDictionary<TOutKey, TOutValue> CoerceDictionary<TKey, TValue, TOutKey, TOutValue>(this IDictionary<TKey, TValue> obj)
	{
		LoggerManager.LogDebug("Attempting to coerce IDictionary", "", obj.GetType().ToString(), $"{typeof(TOutKey)} {typeof(TOutValue)}");

		return obj.Keys.ToList().ConvertAll(s => s.Coerce<TOutKey>())
			.Zip(obj.Values.ToList().ConvertAll(s => s.Coerce<TOutValue>())
				, (k, v) => new { k, v })
			.ToDictionary(x => x.k, x => x.v);
	}

	public static TOut[] CoerceArray<T, TOut>(this T[] obj)
	{
		if (obj.GetType().IsArray)
		{
			LoggerManager.LogDebug("Attempting to coerce array", "", obj.GetType().ToString(), typeof(TOut[]));

			try
			{
				if (obj.TryCoerceArray<T, TOut>(out var coerced))
				{
					LoggerManager.LogDebug("Successfully coerced array", "", obj.GetType().ToString(), typeof(TOut[]));

					return coerced;
				}
				else
				{
					throw new NotImplementedException($"No reasonable coercion for '{obj.GetType()}' => '{typeof(TOut[])}'!");
				}
			}
			catch (System.Exception)
			{
				throw;
			}
		}

		throw new NotSupportedException("Given type is not an array!");
	}

	// attempt to coerce an array T[] into TOut[]
	public static bool TryCoerceArray<T, TOut>(this T[] obj, out TOut[] coerced)
	{
		coerced = default(TOut[]);

		if (obj.GetType().IsArray)
		{
			coerced = Array.ConvertAll(obj, s => s.Coerce<TOut>());
			return true;
		}

		return false;
	}

}
