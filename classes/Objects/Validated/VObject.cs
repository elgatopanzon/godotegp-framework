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
					if (!sourceProperty.IsDefault())
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

public partial class ObjectTest : VObject
{
	private readonly VValue<List<string>> _stringListTest;
	private readonly VValue<Dictionary<string, int>> _dictionarySizeTest;

	// private Value<List<string>> _stringListTest = new Value<List<string>>()
	// 	.Default(new List<string> {"a", "b", "c"})
	// 	.AllowedSize(3, 8)
	// 	.NotNull()
	// 	;

	public List<string> StringListTest
	{
		get { return _stringListTest.Value; }
		set { _stringListTest.Value = value; }
	}

	// private Value<Dictionary<string, int>> _dictionarySizeTest = new Value<Dictionary<string, int>>()
	// 	.Default(new Dictionary<string, int> {{"a", 1}, {"b", 1}, {"c", 1}})
	// 	.AllowedSize(3, 8)
	// 	;

	public Dictionary<string, int> DictionarySizeTest
	{
		get { return _dictionarySizeTest.Value; }
		set { _dictionarySizeTest.Value = value; }
	}

	private readonly VValue<string> _stringTest;
	// private Value<string> _stringTest = new Value<string>()
	// 	.Default("string")
	// 	.AllowedLength(5, 15)
	// 	.AllowedValues(new string[] {"string"})
	// 	;

	public string StringTest
	{
		get { return _stringTest.Value; }
		set { _stringTest.Value = value; }
	}

	private readonly VValue<int> _intTest;
	// private Value<int> _intTest = new Value<int>()
	// 	.Default(5)
	// 	.AllowedRange(2, 8)
	// 	;

	public int IntTest
	{
		get { return _intTest.Value; }
		set { _intTest.Value = value; }
	}

	private readonly VValue<double> _doubleTest;
	// private Value<double> _doubleTest = new Value<double>()
	// 	.Default(5)
	// 	.AllowedRange(2.5, 8.8)
	// 	;

	public double DoubleTest
	{
		get { return _doubleTest.Value; }
		set { _doubleTest.Value = value; }
	}

	private readonly VValue<ulong> _ulongTest;
	// private Value<ulong> _ulongTest = new Value<ulong>()
	// 	.Default(5)
	// 	.AllowedRange(2, 8)
	// 	;

	public ulong UlongTest
	{
		get { return _ulongTest.Value; }
		set { _ulongTest.Value = value; }
	}

	private VValue<int[]> _intArrayTest;
	// private Value<int[]> _intArrayTest = new Value<int[]>()
	// 	.Default(new int[] {1,2,3})
	// 	.AllowedSize(3, 8)
	// 	.AllowedValues(new List<int> {1,2,3})
	// 	.UniqueItems()
	// 	;

	public int[] IntArrayTest
	{
		get { return _intArrayTest.Value; }
		set { _intArrayTest.Value = value; }
	}

	// public Value<int[]> IntPrototypeTest = new Value<int[]>()
	// 	.Prototype(IntArrayTest)
	// 	.Default(new int[] {1,2,3,4})
	// 	;

	private readonly  VValue<Vector2> _vector2Test;
	// private Value<Vector2> _vector2Test = new Value<Vector2>()
	// 	.Default(new Vector2(1, 1))
	// 	.AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
	// 	;

	public Vector2 Vector2Test
	{
		get { return _vector2Test.Value; }
		set { _vector2Test.Value = value; }
	}

	private readonly VValue<List<VValue<Vector2>>> _recursiveTest;
	// private Value<List<Value<Vector2>>> _recursiveTest = new Value<List<Value<Vector2>>>()
	// 	.Default(new List<Value<Vector2>>()
	// 			{
	// 				new Value<Vector2>().Default(new Vector2(1, 1)),
	// 				new Value<Vector2>().Default(new Vector2(2, 2)),
	// 				new Value<Vector2>().Default(new Vector2(3, 3)),
	// 			}
	// 			)
	// 	// .AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
	// 	;

	public List<VValue<Vector2>> RecursiveTest
	{
		get { return _recursiveTest.Value; }
		set { _recursiveTest.Value = value; }
	}

	// public ObjectTest(List<string> stringListTest)
	// {
	// 	StringListTest = stringListTest;
	// }
	//
	private VNative<ObjectTest2> _objectTest;
		// .Default(new Vector2(1, 1))
		// .AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
		// ;

	public ObjectTest2 ObjectTestt
	{
		get { return _objectTest.Value; }
		set { _objectTest.Value = value; }
	}

	public ObjectTest()
	{
		
		_stringListTest = AddValidatedValue<List<string>>()
            .Default(new List<string> { "a", "b", "c" })
            .AllowedSize(3, 8)
            .NotNull()
            ;
        _dictionarySizeTest = AddValidatedValue<Dictionary<string, int>>()
            .Default(new Dictionary<string, int> { { "a", 1 }, { "b", 1 }, { "c", 1 } })
            .AllowedSize(3, 8)
            ;
        _stringTest = AddValidatedValue<string>()
            .Default("string")
            .AllowedLength(5, 15)
            .AllowedValues(new string[] { "string" })
            ;
        _intTest = AddValidatedValue<int>()
            .Default(5)
            .AllowedRange(2, 8)
            ;
        _doubleTest = AddValidatedValue<double>()
            .Default(5)
            .AllowedRange(2.5, 8.8)
            ;
        _ulongTest = AddValidatedValue<ulong>()
            .Default(5)
            .AllowedRange(2, 8)
            ;
        _intArrayTest = AddValidatedValue<int[]>()
            .Default(new int[] { 1, 2, 3 })
            .AllowedSize(3, 8)
            .AllowedValues(new List<int> { 1, 2, 3 })
            .UniqueItems()
            ;
        _vector2Test = AddValidatedValue<Vector2>()
            .Default(new Vector2(1, 1))
            .AddConstraint(new Constraint.Vector2MinMaxValue<Vector2>(1, 1, 1, 1))
            ;
        _recursiveTest = AddValidatedValue<List < VValue<Vector2>>>()
            .Default(new List<VValue<Vector2>>()
                    {
                    new VValue<Vector2>().Default(new Vector2(1, 1)),
                    new VValue<Vector2>().Default(new Vector2(2, 2)),
                    new VValue<Vector2>().Default(new Vector2(3, 3)),
                    }
                    )
            // .AddConstraint(new ValidationConstraintVector2MinMaxValue<Vector2>(1, 1, 1, 1))
            ;

        _objectTest = AddValidatedNative<ObjectTest2>()
        	.Default(new ObjectTest2());
	}

}

public partial class ObjectTest2 : VObject
{
	private readonly VValue<string> _stringTest;

	public string StringTest
	{
		get { return _stringTest.Value; }
		set { _stringTest.Value = value; }
	}

	private VValue<int> _intTest;

	public int IntTest
	{
		get { return _intTest.Value; }
		set { _intTest.Value = value; }
	}

	private VValue<double> _doubleTest;


	public ObjectTest2()
	{
        _stringTest = AddValidatedValue<string>()
            .Default("string100")
            .AllowedLength(5, 15)
            // .AllowedValues(new string[] { "string" })
            ;
        _intTest = AddValidatedValue<int>()
            .Default(50)
            .AllowedRange(20, 80)
            ;
	}
}
