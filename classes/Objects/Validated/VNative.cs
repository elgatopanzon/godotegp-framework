namespace GodotEGP.Objects.Validated;

using GodotEGP.Logging;

public partial class VNative<T> : VValue<T> where T : VObject
{
	public override VNative<T> Default(T defaultValue)
	{
		_default = ValidateValue(defaultValue);
		_value = defaultValue;

		LoggerManager.LogDebug("Setting default value", "", "default", defaultValue);
		LoggerManager.LogDebug("", "", "current", Value);
		return this;
	}

	public override VNative<T> Reset()
	{
		LoggerManager.LogDebug("Resetting value");

		return Default(_default);
	}

	public override VNative<T> ChangeEventsEnabled(bool changeEventsState = true)
	{
		ChangeEventsState = changeEventsState;
		return this;
	}
}

