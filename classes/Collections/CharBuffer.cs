/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CharBuffer
 * @created     : Wednesday May 01, 2024 00:52:56 CST
 */

namespace GodotEGP.Collections;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.CompilerServices;

public partial struct CharBuffer<T> where T : IBuffer<char>
{
	ManagedBuffer<char, T> _buffer;
	public string String
	{
		get { 
			return Regex.Replace(new String(_buffer.Array), @"[^\u0020-\u007F]+", string.Empty);
		}
		set { 
			_buffer.Array = value.ToCharArray();
		}
	}

	public new string ToString()
	{
		return String;
	}

	// allow implicit conversion to/from string
	public static implicit operator string(CharBuffer<T> buffer) => buffer.ToString();
	public static implicit operator CharBuffer<T>(string str) => new CharBuffer<T>() { String = str };
}
