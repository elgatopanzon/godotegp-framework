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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

public partial class ChainableLambda : ChainablePassthrough
{
	public Func<object, Task<object>> Lambda { get; set; }

	public ChainableLambda(Func<object, Task<object>> lambda)
	{
		InitChainable(lambda);
	}

	public ChainableLambda(Func<object, object> lambda)
	{
		InitChainable(lambda);
	}

	public ChainableLambda()
	{
		base.InitChainable();
	}

	/************************
	*  Object pool methods  *
	************************/

	public override void Reset()
	{
		Lambda = null;
		base.Reset();
	}
	
	public override void Init(params object[] p)
	{
		InitChainable((p != null && p.Length >= 1) ? p[0] : null);
	}

	public void InitChainable(Func<object, Task<object>> lambda)
	{
		Lambda = lambda;

		base.InitChainable();
	}

	public void InitChainable(Func<object, object> lambda)
	{
		Lambda = async (x) => {
			return lambda(x);
		};

		base.InitChainable();
	}


	public static ChainableLambda FromMethod(object method)
	{
		var chainable = method.CreateInstance<ChainableLambda>(null);
		var lambda = method.CreateInstance<ChainableLambdaDynamic>();
		lambda.MethodObject = method;
		chainable.Target = lambda;

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
	public ChainableLambda(Func<T1, Task<T2>> lambda) : base(null)
	{
		InitChainable(async (x) => {
			return (T2) await lambda((T1) x);
		});
	}

	public ChainableLambda(Func<T1, T2> lambda) : base(null)
	{
		InitChainable((x) => {
			return (T2) lambda((T1) x);
		});
	}

	public ChainableLambda() : base(null)
	{
		InitChainable();
	}

	public static ChainableLambda<T1, T2> FromMethod(Func<T1, T2> method)
	{
		var chainableLambda = method.CreateInstance<ChainableLambda<T1, T2>>(null);
		chainableLambda.InitChainable(async (x) => {
			return method((T1) x);
    	});

    	return chainableLambda;
	}

	public static ChainableLambda<T1, T2> FromMethodAsync(Func<T1, Task<T2>> method)
	{
		var chainableLambda = method.CreateInstance<ChainableLambda<T1, T2>>(null);
		chainableLambda.InitChainable(async (x) => {
			return await method((T1) x);
    	});

    	return chainableLambda;
	}

}

public partial class ChainableLambdaDynamic : ChainablePassthrough
{
	public object MethodObject { get; set; }

	public ChainableLambdaDynamic(object method)
	{
		InitChainable(method);
	}
	public ChainableLambdaDynamic()
	{
	}

	/************************
	*  Object pool methods  *
	************************/

	public override void Reset()
	{
		MethodObject = null;
		base.Reset();
	}
	
	public override void Init(params object[] p)
	{
		InitChainable((p != null && p.Length >= 1) ? p[0] : null);
	}

	public override void InitChainable(object methodObject)
	{
		if (methodObject != null)
		{
			MethodObject = methodObject;
		}

		base.InitChainable();
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
