namespace GodotEGP.Objects.Validated;

using Godot;
using System;
using System.Collections.Generic;
using GodotEGP.Objects.Extensions;
using System.Reflection;
using System.Linq;

using GodotEGP.Logging;
using GodotEGP.Event.Events;
using GodotEGP.Objects.ObjectPool;

public partial class VObject
{
	protected VObject _parent;

	protected List<VValue> Properties { get; set; } = new();

	public VObject(VObject parent = null)
	{
		Init();
		_parent = parent;
		ValidateFields();
	}


	/*********************************
	*  Object pool handler methods  *
	*********************************/
	
	public virtual void Reset()
	{
		_parent = null;
		for (int i = 0; i < Properties.Count; i++)
		{
			Properties[i].Reset();
		}
	}
	public virtual void Init()
	{
		for (int i = 0; i < Properties.Count; i++)
		{
			Properties[i].Init();
			Properties[i].Parent = this;
		}
	}


	/*********************************
	*  Property management methods  *
	*********************************/

    protected VValue<T> AddValidatedValue<T>(object parent = null)
    {
        var val = typeof(VValue<T>).CreateInstance<VValue<T>>();
        val.Parent = parent;

        Properties.Add(val);
        return val;
    }

    protected VNative<T> AddValidatedNative<T>(object parent = null) where T : VObject
    {
        var val = typeof(VNative<T>).CreateInstance<VNative<T>>();
        val.Parent = parent;

        Properties.Add(val);
        return val;
    }

	public List<VValue> GetProperties()
	{
		return Properties;
	}


	public VObject SetParent(VObject parent)
	{
		_parent = parent;
		return this;
	}


	/************************
	*  Validation methods  *
	************************/

	public void ValidateFields()
	{
		Type t = this.GetType();

		LoggerManager.LogDebug($"Validating object fields for {t.Name}");

		foreach (FieldInfo field in t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
		{
			// LoggerManager.LogDebug($"Validating object field {field.Name}");

			if (field.GetType().GetMethod("Validate") != null)
			{
				if (field.GetValue(this) is IVValue vv)
				{
					vv.Validate();
				}
			}
		}
	}


	/*******************
	*  Merge methods  *
	*******************/
	
	public virtual void MergeFrom(VObject sourceObj)
	{
		LoggerManager.LogDebug($"Merging {sourceObj.GetType().Name}");

		for (int i = 0; i < sourceObj.Properties.Count; i++)
		{
			VValue sourceProperty = sourceObj.Properties[i];

			// LoggerManager.LogDebug("Evaluating property from source object", "", "sourcePropType", sourceProperty.GetType());
			// LoggerManager.LogDebug("", "", "sourceProp", sourceProperty);
			// LoggerManager.LogDebug("", "", "thisProp", Properties[i]);
			// LoggerManager.LogDebug("", "", "isDefault", sourceProperty.IsDefault());
			// LoggerManager.LogDebug("", "", "isNull", sourceProperty.IsNull());

			if (!sourceProperty.IsNull())
			{
				if (sourceProperty.RawValue is VObject validatedObjectSource)
				{
					if (Properties[i].RawValue is VObject validatedObjectThis)
					{
						// LoggerManager.LogDebug($"Property is type {sourceObj.GetType().Name}, recursive merging");

						validatedObjectThis.MergeFrom(validatedObjectSource);
					}
				}
				else
				{
					if (!sourceProperty.IsDefault() || sourceProperty.HasBeenSet)
					{
						// LoggerManager.LogDebug("Merging!", "", "sourceProp", sourceProperty);
						if (sourceProperty.MergeCollections)
						{
							Properties[i].MergeCollection(sourceProperty);
						}
						else
						{
							Properties[i].RawValue = sourceProperty.RawValue;
						}
					}
				}
			}

		}

		// LoggerManager.LogDebug($"Merging {sourceObj.GetType().Name} finished", "", "obj", this);
		LoggerManager.LogDebug($"Merging {sourceObj.GetType().Name} finished");
	}


	/***************
	*  Callbacks  *
	***************/

	public void _onValueChange(object o, object v, object nv)
	{
		LoggerManager.LogDebug("Value changed in object", "", "owner", this.GetType().Name);
		// LoggerManager.LogDebug("", "", "vo", o.GetType().FullName);
		// LoggerManager.LogDebug("", "", "value", v);
		// LoggerManager.LogDebug("", "", "newValue", nv);

		this.Emit<ValidatedValueChanged>((e) => {
			e.SetValue(nv);
			e.SetPrevValue(v);
		});

		if (_parent != null)
		{
			_parent._onValueChange(this, v, nv);
		}
	}
}

public interface IMergeFrom<in T>
{
    void MergeFrom(T sourceObj);
}

public class VObjectObjectPoolHandler : ObjectPoolHandler<VObject>
{
	public override VObject OnReturn(VObject instance)
	{
		instance.Reset();
		return instance;
	}
	public override VObject OnTake(VObject instance)
	{
		instance.Init();
		return instance;
	}
}
