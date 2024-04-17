/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableSchemaDefinition
 * @created     : Sunday Mar 31, 2024 13:42:20 CST
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
using System.Collections.Generic;
using System.Linq;

public partial class ChainableSchemaDefinition
{
	public string Name { get; set; }
	public List<Type> Types { get; set; } = new();
	public Dictionary<string, Type> Properties { get; set; } = new();
	public Dictionary<string, List<MethodParameters>> Parameters { get; set; } = new();

	public static ChainableSchemaDefinition BuildInputSchema(IChainable chainable)
	{
		return ChainableSchemaDefinition.BuildDefinition(chainable, output: false);
	}
	public static ChainableSchemaDefinition BuildOutputSchema(IChainable chainable)
	{
		return ChainableSchemaDefinition.BuildDefinition(chainable, output: true);
	}

	public static ChainableSchemaDefinition BuildDefinition(IChainable chainable, bool output = false)
	{
		var def = new ChainableSchemaDefinition();

		def.Name = (output == false) ? $"{chainable.Name}Input" : $"{chainable.Name}Output";

		def.Types = ChainableSchemaDefinition.GetObjectTypes(chainable, output:output);

		// list of properties for the object excluding properties from the base
		// Chainable class
		var excludeProperties = typeof(Chainable).GetProperties();
		var properties = chainable.GetType().GetProperties();

		// set the properties names and types
		foreach (PropertyInfo prop in properties)
		{
			def.Properties.Add(prop.Name, prop.GetType());
		}
		
		// remove excluded properties
		foreach (PropertyInfo prop in excludeProperties)
		{
			def.Properties.Remove(prop.Name);
		}

		// get method properties for Run(), Batch() and Stream() methods
		foreach (Type type in GetParentTypes(chainable.GetType()).Concat(new Type[] { chainable.GetType() }))
		{
			// LoggerManager.LogDebug("Getting method params for class", "", "class", type.Name);

			foreach (var methodName in new string[] { nameof(chainable.Run), nameof(chainable.Batch), nameof(chainable.Stream) })
			{
				var methods = type.GetMethods();

				foreach (MethodInfo method in methods)
				{
					if (method.Name == methodName)
					{
						// LoggerManager.LogDebug("Method FOUND on class", "", methodName, type.Name);

						// build a struct of MethodParameters
						var parameters = new MethodParameters();
						parameters.Parameters = new();
						parameters.Name = type.Name;
						parameters.MethodInfo = method;

						foreach (var param in method.GetParameters().Reverse().ToArray())
						{
							parameters.Parameters.Add(new() {
								Name = param.Name?.ToString(),
								Type = param.ParameterType,
								Default = param.DefaultValue,
								Optional = param.IsOptional,
							});
						}

						if (!def.Parameters.TryGetValue(methodName, out var m))
						{
							def.Parameters.Add(methodName, new());
						}

						def.Parameters[methodName].Add(parameters);
					}
				}
			}
		}

		return def;
	}

	public static List<Type> GetObjectTypes(IChainable obj, bool output = false)
	{
		Type baseInterface = (output == false) ? typeof(IChainableInput) : typeof(IChainableOutput);
		List<Type> validTypes = new();

		foreach (var interf in obj.GetType().GetInterfaces())
		{
			if (interf.GetInterfaces().Contains(baseInterface))
			{
				foreach (var generic in interf.GetGenericArguments())
				{
					validTypes.Add(generic);
				}
			}
		}

		return validTypes;
	}

	public static IEnumerable<Type> GetParentTypes(Type type)
	{
    	// is there any base type?
    	if (type == null)
    	{
        	yield break;
    	}

    	// return all implemented or inherited interfaces
    	foreach (var i in type.GetInterfaces())
    	{
        	yield return i;
    	}

    	// return all inherited types
    	var currentBaseType = type.BaseType;
    	while (currentBaseType != null)
    	{
        	yield return currentBaseType;
        	currentBaseType= currentBaseType.BaseType;
    	}
	}
}

public struct MethodParameter
{
	public string Name { get; set; }
	public Type Type { get; set; }
	public object Default { get; set; }
	public bool Optional { get; set; }
}

public struct MethodParameters
{
	public string Name { get; set; }
	public List<MethodParameter> Parameters { get; set; }
	internal MethodInfo MethodInfo { get; set; }
}
