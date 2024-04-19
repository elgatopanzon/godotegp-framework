namespace GodotEGP.Objects.Validated;

using Godot;
using System;
using System.Collections.Generic;
using GodotEGP.Objects.Extensions;
using System.Reflection;
using System.Linq;

using GodotEGP.Logging;
using GodotEGP.Event.Events;

public partial class VObject
{
	protected VObject _parent;

	protected List<VValue> Properties { get; } = new List<VValue>();
    protected VValue<T> AddValidatedValue<T>(object parent = null)
        {
            var val = new VValue<T>();
            val.Parent = parent;

            Properties.Add(val);
            return val;
        }

    protected VNative<T> AddValidatedNative<T>(object parent = null) where T : VObject
        {
            var val = new VNative<T>();
            val.Parent = parent;

            Properties.Add(val);
            return val;
        }

	public VObject(VObject parent = null)
	{
		_parent = parent;
		ValidateFields();
	}

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

	public List<VValue> GetProperties()
	{
		return Properties;
	}

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

