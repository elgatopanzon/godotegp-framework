/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IPoolableObject
 * @created     : Wednesday Apr 17, 2024 17:30:57 CST
 */

namespace GodotEGP.Objects.ObjectPool;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial interface IPoolableObjectResetable
{
	public void Reset();
}

public partial interface IPoolableObject : IPoolableObjectResetable
{
	public void Init(params object[] p);
}

public partial interface IPoolableObject<TSelf> : IPoolableObjectResetable
{
	public TSelf Init(params object[] p);
}

public partial interface IPoolableObject<T, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1);
}
public partial interface IPoolableObject<T, T2, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1, T2 p2);
}
public partial interface IPoolableObject<T, T2, T3, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1, T2 p2, T3 p3);
}
public partial interface IPoolableObject<T, T2, T3, T4, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1, T2 p2, T3 p3, T4 p4);
}
public partial interface IPoolableObject<T, T2, T3, T4, T5, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1, T2 p2, T3 p3, T4 p4, T5 p5);
}
public partial interface IPoolableObject<T, T2, T3, T4, T5, T6, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);
}
public partial interface IPoolableObject<T, T2, T3, T4, T5, T6, T7, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);
}
public partial interface IPoolableObject<T, T2, T3, T4, T5, T6, T7, T8, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);
}
public partial interface IPoolableObject<T, T2, T3, T4, T5, T6, T7, T8, T9, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);
}
public partial interface IPoolableObject<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TSelf> : IPoolableObjectResetable
{
	public TSelf Init(T p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10);
}
