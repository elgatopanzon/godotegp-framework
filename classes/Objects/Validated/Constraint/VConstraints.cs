namespace GodotEGP.Objects.Validated.Constraint;

using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using GodotEGP.Objects.Extensions;

public partial class VConstraint<T>
{
	public virtual bool Validate(T value)
	{
		return true;
	}
}

public partial class UniqueItems<T> : VConstraint<T>
{
	public override bool Validate(T value)
	{
		if (value is IList list)
		{
			List<object> seen = new List<object>();

			foreach (var item in list)
			{
				if (seen.Contains(item))
				{
					Exception e = new ValidationNonUniqueItemException($"Item {item} is non-unique");
					e.Data.Add("nonUniqueItem", item);

					throw e;
				}

				seen.Add(item);
			}
		}
		return true;
	}

	public partial class ValidationNonUniqueItemException : Exception
	{
		public ValidationNonUniqueItemException() { }
		public ValidationNonUniqueItemException(string message) : base(message) { }
		public ValidationNonUniqueItemException(string message, Exception inner) : base(message, inner) { }
		protected ValidationNonUniqueItemException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
	}
}

public partial class MinMaxLength<T> : VConstraint<T>
{
	private int _minLength;
	private int _maxLength;

	public MinMaxLength(int minLength, int maxLength)
	{
		_minLength = minLength;
		_maxLength = maxLength;
	}

	// validate method
	public override bool Validate(T value)
	{
		if (value.TryCast<string>(out string val))
		{
			if (val.Length < _minLength && _minLength > 0)
			{
				throw new ValidationMinLengthException($"'{value}' is less than minLength of {_minLength}");
			}
			if (val.Length > _maxLength && _maxLength > 0)
			{
				throw new ValidationMaxLengthException($"'{value}' is greater than maxLength of {_maxLength}");
			}
		}

		return true;
	}

	// exceptions
	public partial class ValidationMinLengthException : Exception
	{
		public ValidationMinLengthException(string message) : base(message) {}
	}

	public partial class ValidationMaxLengthException : Exception
	{
		public ValidationMaxLengthException(string message) : base(message) {}
	}
}

public partial class MinMaxItems<T> : VConstraint<T>
{
	private int _minItems;
	private int _maxItems;

	public MinMaxItems(int minItems, int maxItems)
	{
		_minItems = minItems;
		_maxItems = maxItems;
	}

	// validate method
	public override bool Validate(T value)
	{
		if (value is IList list)
		{
			if (list.Count < _minItems && _minItems > 0)
			{
				throw new ValidationMinItemsException($"{typeof(T).Name}'s '{list.Count}' items is less than the required {_minItems}");
			}
			if (list.Count > _maxItems && _maxItems > 0)
			{
				throw new ValidationMaxItemsException($"{typeof(T).Name}'s '{list.Count}' items is more than allowed {_maxItems}");
			}
		}
		else if (value is ICollection collection)
		{
			if (collection.Count < _minItems && _minItems > 0)
			{
				throw new ValidationMinItemsException($"{typeof(T).Name}'s '{collection.Count}' properties is less than the required {_minItems}");
			}
			if (collection.Count > _maxItems && _maxItems > 0)
			{
				throw new ValidationMaxItemsException($"{typeof(T).Name}'s '{collection.Count}' properties is more than allowed {_maxItems}");
			}
		}
		else 
		{
			throw new NotImplementedException($"Validation for {typeof(T).Name} is not implemented");
		}

		return true;
	}

	// exceptions
	public partial class ValidationMinItemsException : Exception
	{
		public ValidationMinItemsException(string message) : base(message) {}
	}

	public partial class ValidationMaxItemsException : Exception
	{
		public ValidationMaxItemsException(string message) : base(message) {}
	}
}

public partial class MinMaxValue<T> : VConstraint<T>
{
	private T _minValue;
	private T _maxValue;

	public MinMaxValue(T minValue, T maxValue)
	{
		_minValue = minValue;
		_maxValue = maxValue;
	}

	// validate method
	public override bool Validate(T value)
	{
		if (Comparer<T>.Default.Compare(value, _minValue) < 0 && !_minValue.Equals(0))
		{
			throw new ValidationMinValueException($"'{value}' is less than minValue of {_minValue}");
		}
		if (Comparer<T>.Default.Compare(value, _maxValue) > 0 && !_maxValue.Equals(0))
		{
			throw new ValidationMaxValueException($"'{value}' is greater than maxValue of {_maxValue}");
		}

		return true;
	}

	// exceptions
	public partial class ValidationMinValueException : Exception
	{
		public ValidationMinValueException(string message) : base(message) {}
	}

	public partial class ValidationMaxValueException : Exception
	{
		public ValidationMaxValueException(string message) : base(message) {}
	}
}

public partial class Vector2MinMaxValue<T> : VConstraint<T>
{
	private MinMaxValue<double> _xConstraint;
	private MinMaxValue<double> _yConstraint;

	public Vector2MinMaxValue(double minXValue, double maxXValue, double minYValue, double maxYValue)
	{
		_xConstraint = new MinMaxValue<double>(minXValue, maxXValue);
		_yConstraint = new MinMaxValue<double>(minYValue, maxYValue);
	}

	// validate method
	public override bool Validate(T value)
	{
		if (value is Vector2 v2)
		{
			try
			{
				_xConstraint.Validate(v2.X);
			}
			catch (System.Exception ei)
			{
				Exception e = new ValidationXValueException(ei.Message);
				throw e;
			}

			try
			{
				_yConstraint.Validate(v2.Y);
			}
			catch (System.Exception ei)
			{
				Exception e = new ValidationYValueException(ei.Message);
				throw e;
			}
		}
		return true;
	}

	// exceptions
	public partial class ValidationXValueException : Exception
	{
		public ValidationXValueException(string message) : base(message) {}
	}

	public partial class ValidationYValueException : Exception
	{
		public ValidationYValueException(string message) : base(message) {}
	}
}

public partial class AllowedValues<T> : VConstraint<T>
{
	private IList _allowedValues;

	public AllowedValues(IList allowedValues)
	{
		_allowedValues = allowedValues;
	}

	public override bool Validate(T value)
	{
		if (value is IList list)
		{
			foreach (var item in list)
			{
				if (!_allowedValues.Contains(item))
				{
					Exception e = new ValidationIllegalItemException($"The item {item} is not an allowed item");
					e.Data.Add("allowedValues", _allowedValues);

					throw e;
				}
			}
		}
		else
		{
			if (!_allowedValues.Contains(value))
			{
				Exception e = new ValidationIllegalItemException($"{value} is not an allowed value");
				e.Data.Add("allowedValues", _allowedValues);

				throw e;
			}
		}

		return true;
	}

	public partial class ValidationIllegalItemException : Exception
	{
		public ValidationIllegalItemException() { }
		public ValidationIllegalItemException(string message) : base(message) { }
		public ValidationIllegalItemException(string message, Exception inner) : base(message, inner) { }
		protected ValidationIllegalItemException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
	}
}

