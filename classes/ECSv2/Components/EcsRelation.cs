/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EcsRelation
 * @created     : Monday Apr 29, 2024 12:45:39 CST
 */

namespace GodotEGP.ECSv2.Components;

public struct EcsRelation<TLeft, TRight> : IComponent
{
	public TLeft Relation;
	public TRight Target;
}
