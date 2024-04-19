namespace GodotEGP.Objects.Validated;

using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;

using GodotEGP.Logging;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated.Constraint;
using GodotEGP.Objects.ObjectPool;

public abstract partial class VValue : Validated.IVValue, IPoolableObject
{
	public abstract bool Validate();
	public abstract bool IsDefault();
	public abstract bool IsNull();

	public abstract void Reset();
	public abstract void Init(params object[] p);

	public abstract void MergeCollection(VValue mergeFromVV);

	internal abstract object RawValue {set; get;}
	internal abstract object Parent {set; get;}
	internal abstract bool MergeCollections {get; set;}

	internal abstract bool HasBeenSet {get; set;}
}

public partial class VValue<T> : VValue
{
	protected T _value;
	protected T _default;
	protected bool NullAllowed = true;
	internal override bool HasBeenSet {get; set;}
	protected bool ChangeEventsState = false;

	private object _parent;
	internal override object Parent {
		set
		{
			_parent = value;
		}
		get
		{
			return _parent;
		}
	}

	internal override object RawValue {
		get {
			return Value;
		}
		set {
			Value = (T) value;
		}
	}

	public T Value
	{
		get { 
			return _value;
		}
		set { 
			_SetValue(value);
		}
	}
    protected bool _mergeCollections = false;
    internal override bool MergeCollections {
		get {
			return _mergeCollections;
		}
		set {
			_mergeCollections = value;
		}
    }

	protected List<VConstraint<T>> _constraints = new List<VConstraint<T>>();

	/*********************************
	*  Object pool handler methods  *
	*********************************/

	public override void Reset()
	{
		RawValue = Default(_default);
		HasBeenSet = false;
	}
	
	
	public override void Init(params object[] p)
	{
	}


	/*******************************
	*  VValue management methods  *
	*******************************/

	private void _SetValue(T newValue)
	{
		HasBeenSet = true;

		LoggerManager.LogDebug($"Setting value {this.GetType().Name}<{this.GetType().GetTypeInfo().GenericTypeArguments[0]}>", "", "value", newValue);

		newValue = ValidateValue(newValue);

		if (ChangeEventsState)
		{
			// Type root = this.Parent.GetType().BaseType;
			if (this.Parent is VObject vo)
			{
				vo._onValueChange(this, _value, newValue);
			}
		}

		_value = newValue;
	}


	public VValue()
	{
		Init();
	}
	
	public override bool IsDefault()
	{
		return (_default != null && _value != null && _value.Equals(_default));
	}
	public override bool IsNull()
	{
		return (_value == null);
	}

	public T GetDefault()
	{
		return _default;
	}

	public virtual VValue<T> Default(T defaultValue)
	{
		_default = ValidateValue(defaultValue);
		_value = defaultValue;

		// LoggerManager.LogDebug("Setting default value", "", "default", defaultValue);
		// LoggerManager.LogDebug("", "", "current", Value);
		return this;
	}

	public virtual VValue<T> Prototype(VValue<T> from)
	{
		_default = from._default;

		foreach (VConstraint<T> constraint in from._constraints)
		{
			_constraints.Add(constraint);
		}

		return this;
	}

	public VValue<T> NotNull()
	{
		NullAllowed = false;
		return this;
	}

	public virtual VValue<T> ChangeEventsEnabled(bool changeEventsState = true)
	{
		ChangeEventsState = changeEventsState;
		return this;
	}

	/********************************
	*  Constraint shortcut values  *
	********************************/

	// constraint classes to activate constraints on an object
	public virtual VValue<T> AllowedLength(int minLength = 0, int maxLength = 0)
	{
		return AddConstraint(this.CreateInstance<MinMaxLength<T>>(minLength, maxLength));
	}

	public virtual VValue<T> AllowedRange(T min, T max)
	{
		return AddConstraint(this.CreateInstance<MinMaxValue<T>>(min, max));
	}

	public virtual VValue<T> AllowedSize(int min, int max)
	{
		return AddConstraint(this.CreateInstance<MinMaxItems<T>>(min, max));
	}

	public virtual VValue<T> AllowedValues(IList allowedValues)
	{
		return AddConstraint(this.CreateInstance<AllowedValues<T>>(allowedValues));
	}

	public virtual VValue<T> UniqueItems()
	{
		return AddConstraint(this.CreateInstance<UniqueItems<T>>());
	}

	public VValue<T> AddConstraint(VConstraint<T> constraint)
	{
		_constraints.Add(constraint);

		if (Value != null && !Value.Equals(default(T)))
		{
			ValidateValue(Value);
		}

		return this;
	}


	/*******************
	*  Validate methods  *
	*******************/

	public virtual T ValidateValue(T value)
	{
		// LoggerManager.LogDebug("Validating value", "", "value", new Dictionary<string, string> { { "value", value?.ToString() } , { "default", _default?.ToString() }, { "type", value?.GetType().Name } });

		if (!NullAllowed && value == null)
		{
			throw new ValidationValueIsNullException($"The {typeof(T)} value is null and NullAllowed is false");
		}

		foreach (VConstraint<T> constraint in _constraints)
		{
			constraint.Validate(value);
		}
		return value;
	}

	public override bool Validate()
	{
		ValidateValue(_value);
		return true;
	}

	public partial class ValidationValueIsNullException : Exception
	{
		public ValidationValueIsNullException() { }
		public ValidationValueIsNullException(string message) : base(message) { }
		public ValidationValueIsNullException(string message, Exception inner) : base(message, inner) { }
		protected ValidationValueIsNullException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
	}


	/*******************
	*  Merge methods  *
	*******************/
	
	// provide method to merge collections
	public override void MergeCollection(VValue mergeFromVV)
	{
		if (Value is IDictionary col && mergeFromVV.RawValue is IDictionary sourceCol)
		{
			LoggerManager.LogDebug($"Merging collection of type {typeof(T).FullName}");

			foreach (DictionaryEntry entry in sourceCol)
			{
				// if the target dict does not contain a key from the source,
				// then add it to the target
				if (!col.Contains(entry.Key))
				{
					col.Add(entry.Key, entry.Value);
				}
				// if it exists, overwrite the value with source
				else
				{
					col[entry.Key] = entry.Value;
				}
			}

			LoggerManager.LogDebug($"Merging collection result {typeof(T).FullName}", "", "obj", Value);
		}
		else
		{
			LoggerManager.LogDebug($"Value of type {typeof(T).FullName} does not implement ICollection");
		}
	}
}

public class VValueObjectPoolHandler : ObjectPoolHandler<VValue>
{
	public override VValue OnReturn(VValue instance)
	{
		instance.Reset();
		return instance;
	}
	public override VValue OnTake(VValue instance, params object[] p)
	{
		instance.Init(p);
		return instance;
	}
}
