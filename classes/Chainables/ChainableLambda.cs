/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableLambda
 * @created     : Friday Apr 05, 2024 16:40:55 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

public partial class ChainableLambda : ChainablePassthrough
{
	public Func<object, Task<object>> Lambda { get; set; }

	public ChainableLambda(Func<object, Task<object>> lambda)
	{
		Lambda = lambda;
	}

	public ChainableLambda(Func<object, object> lambda)
	{
		Lambda = async (x) => {
			return lambda(x);
		};
	}

	public static ChainableLambda FromMethod(object method)
	{
		var chainable = new ChainableLambda(null);
		chainable.Target = new ChainableLambdaDynamic(method);

		return chainable;
	}

	public async override Task<object> _Process()
	{
		if (Lambda != null)
			return await Lambda(Input);		
		else
			return await base._Process();
	}

	public async override IAsyncEnumerable<object> StreamTransform(IAsyncEnumerable<object> input = null)
	{
		await foreach (var stream in input)
		{
			if (Lambda != null)
				yield return await Lambda(Input);
			else
				yield return stream;
		}
	}
}

public partial class ChainableLambda<T1, T2> : ChainableLambda
{
	public ChainableLambda(Func<T1, Task<T2>> lambda) : base(async (x) => {
			return (T2) await lambda((T1) x);
		})
	{
	}

	public ChainableLambda(Func<T1, T2> lambda) : base((x) => {
			return (T2) lambda((T1) x);
		})
	{
	}

	public static ChainableLambda<T1, T2> FromMethod(Func<T1, T2> method)
	{
		return new ChainableLambda<T1, T2>(async (x) => {
			return method(x);
    	});
	}

	public static ChainableLambda<T1, T2> FromMethodAsync(Func<T1, Task<T2>> method)
	{
		return new ChainableLambda<T1, T2>(async (x) => {
			return await method(x);
    	});
	}
}

public partial class ChainableLambdaDynamic : ChainablePassthrough
{
	public object MethodObject { get; set; }

	public ChainableLambdaDynamic(object method)
	{
		MethodObject = method;
	}

	public async override Task<object> _Process()
	{

		var methodInfo = ((dynamic) MethodObject).Method;
		List<object> methodParams = new();

		LoggerManager.LogDebug("Lambda dynamic invoking method object", "", methodInfo.Name, methodInfo.ToString());

		Config.Params["input"] = Input;

		foreach (ParameterInfo param in methodInfo.GetParameters())
		{
			if (Config.Params.ContainsKey(param.Name))
			{
				methodParams.Add(Config.Params[param.Name]);
				LoggerManager.LogDebug("Lambda dynamic method param match", "", param.Name, Config.Params[param.Name]);
			}
			else
			{
				if (param.IsOptional)
				{
					methodParams.Add(param.DefaultValue);
				}
				else
				{
					LoggerManager.LogDebug("Lambda dynamic method param match not found", "", param.Name, param.Name.GetType().Name);

					throw new ChainableLambdaDynamicMethodParameterMissingException($"Required parameter '{param.Name}' ({param.Name.GetType().Name}) not found in config!");
				}
			}
		}

		return ((dynamic) MethodObject).DynamicInvoke(methodParams.ToArray());
		return "";
	}
}

public class ChainableLambdaDynamicMethodParameterMissingException : Exception
{
	public ChainableLambdaDynamicMethodParameterMissingException() { }
	public ChainableLambdaDynamicMethodParameterMissingException(string message) : base(message) { }
	public ChainableLambdaDynamicMethodParameterMissingException(string message, Exception inner) : base(message, inner) { }
	protected ChainableLambdaDynamicMethodParameterMissingException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
}
