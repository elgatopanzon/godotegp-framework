/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableSchema
 * @created     : Sunday Mar 31, 2024 13:32:44 CST
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

public partial class ChainableSchema
{
	public ChainableSchemaDefinition Input { get; set; }
	public ChainableSchemaDefinition Output { get; set; }

	// create a ChainableSchema object from the given IChainable
	public static ChainableSchema BuildFromObject(IChainable chainable)
	{
		var schema = chainable.CreateInstance<ChainableSchema>();

		// set the input and output schema
		schema.Input = ChainableSchemaDefinition.BuildInputSchema(chainable);
		schema.Output = ChainableSchemaDefinition.BuildOutputSchema(chainable);

		// return the final object
		return schema;
	}

	public bool ObjectIsValidType(object obj, bool output = false)
	{
		if (obj == null)
			return false;

		return IsValidType(obj.GetType(), output:output);
	}

	public bool IsValidType(Type type, bool output = false)
	{
		var schemaTypes = (output == false) ? Input.Types : Output.Types;
		return (schemaTypes.Contains(type) || schemaTypes.Count == 0);
	}
}

