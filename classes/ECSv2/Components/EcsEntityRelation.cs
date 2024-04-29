/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EcsEntityRelation
 * @created     : Monday Apr 29, 2024 12:47:00 CST
 */

namespace GodotEGP.ECSv2.Components;

public struct EcsEntityRelation<TRelation> : IComponent
{
	public TRelation Relation;
	public Entity Entity;
}
