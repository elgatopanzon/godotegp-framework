/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Buffer
 * @created     : Wednesday May 01, 2024 12:17:50 CST
 */

namespace GodotEGP.Collections;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Runtime.CompilerServices;

public partial interface IBuffer<T> {
	public int Length { get; }
	public T[] Array { get; set; }
}


public struct ManagedBuffer<T, TT> where TT : IBuffer<T>
{
	private TT _buffer;

	public int Length
	{
		get { return _buffer.Length; }
	}
	public T[] Array
	{
		get { return _buffer.Array; }
		set { _buffer.Array = value; }
	}
}

public struct ManagedBufferTest
{
	public ManagedBufferTest()
	{
		ManagedBuffer<char, Buffer16<char>> buf = new();

		foreach (var entry in buf.Array)
		{
			
		}
	}
}


/*************
*  Buffers  *
*************/

[InlineArray(16)]
public struct Buffer16<T> : IBuffer<T> where T : unmanaged
{
	private T _obj;
	public int Length
	{
		get { return 16; }
	}
	public T[] Array
	{
		get {
			Span<T> span = this;

			return span.ToArray();
		}
		set {
			for (int i = 0; i < this.Length; i++)
			{
				if (value.Length >= i+1)
				{
					this[i] = value[i];
				}
			}
		}
	}
}

[InlineArray(32)]
public struct Buffer32<T> : IBuffer<T> where T : unmanaged
{
	private T _obj;
	public int Length
	{
		get { return 32; }
	}
	public T[] Array
	{
		get {
			Span<T> span = this;

			return span.ToArray();
		}
		set {
			for (int i = 0; i < this.Length; i++)
			{
				if (value.Length >= i+1)
				{
					this[i] = value[i];
				}
			}
		}
	}
}

[InlineArray(64)]
public struct Buffer64<T> : IBuffer<T> where T : unmanaged
{
	private T _obj;
	public int Length
	{
		get { return 64; }
	}
	public T[] Array
	{
		get {
			Span<T> span = this;

			return span.ToArray();
		}
		set {
			for (int i = 0; i < this.Length; i++)
			{
				if (value.Length >= i+1)
				{
					this[i] = value[i];
				}
			}
		}
	}
}

[InlineArray(128)]
public struct Buffer128<T> : IBuffer<T> where T : unmanaged
{
	private T _obj;
	public int Length
	{
		get { return 128; }
	}
	public T[] Array
	{
		get {
			Span<T> span = this;

			return span.ToArray();
		}
		set {
			for (int i = 0; i < this.Length; i++)
			{
				if (value.Length >= i+1)
				{
					this[i] = value[i];
				}
			}
		}
	}
}

[InlineArray(256)]
public struct Buffer256<T> : IBuffer<T> where T : unmanaged
{
	private T _obj;
	public int Length
	{
		get { return 256; }
	}
	public T[] Array
	{
		get {
			Span<T> span = this;

			return span.ToArray();
		}
		set {
			for (int i = 0; i < this.Length; i++)
			{
				if (value.Length >= i+1)
				{
					this[i] = value[i];
				}
			}
		}
	}
}

[InlineArray(512)]
public struct Buffer512<T> : IBuffer<T> where T : unmanaged
{
	private T _obj;
	public int Length
	{
		get { return 512; }
	}
	public T[] Array
	{
		get {
			Span<T> span = this;

			return span.ToArray();
		}
		set {
			for (int i = 0; i < this.Length; i++)
			{
				if (value.Length >= i+1)
				{
					this[i] = value[i];
				}
			}
		}
	}
}

[InlineArray(1024)]
public struct Buffer1024<T> : IBuffer<T> where T : unmanaged
{
	private T _obj;
	public int Length
	{
		get { return 1024; }
	}
	public T[] Array
	{
		get {
			Span<T> span = this;

			return span.ToArray();
		}
		set {
			for (int i = 0; i < this.Length; i++)
			{
				if (value.Length >= i+1)
				{
					this[i] = value[i];
				}
			}
		}
	}
}
