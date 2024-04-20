namespace GodotEGP.Config;

using System;

using GodotEGP.Objects.Validated;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.DAL.Endpoints;
using GodotEGP.DAL.Operations;
using GodotEGP.Service;
using GodotEGP.Event.Events;

public abstract partial class ConfigObject : IConfigObject
{
	internal abstract object RawValue { get; set; }
	internal abstract IDataEndpoint DataEndpoint { get; set; }
	internal abstract bool Loading { get; set; }
	internal abstract string Name { get; set; }

	public abstract void Load();
	public abstract void Save();

	public static ConfigObject Create(string parameterTypeName)
    {
        Type parameterType = Type.GetType(parameterTypeName);
        Type genericType = typeof(ConfigObject<>).MakeGenericType(parameterType);
        return (ConfigObject) Activator.CreateInstance(genericType);
    }
}

public partial class ConfigObject<T> : ConfigObject where T : VObject, new()
{
	private string _name;
	internal override string Name
	{
		get { return _name; }
		set { _name = value; }
	}

	private bool _loading;
	internal override bool Loading
	{
		get { return _loading; }
		set { _loading = value; }
	}

	private VNative<T> _validatedObject;
	public T Value
	{
		get { return _validatedObject.Value; }
		set { _validatedObject.Value = value; }
	}

	internal override object RawValue {
		get {
			return Value;
		}
		set {
			Value = (T) value;
		}
	}

	private IDataEndpoint _dataEndpoint;
	internal override IDataEndpoint DataEndpoint
	{
		get { return _dataEndpoint; }
		set { _dataEndpoint = value; }
	}

	public ConfigObject()
	{
		_validatedObject = new VNative<T>();
		_validatedObject.Value = new T();
	}

	public override void Load()
	{
		_loading = true;
		var dopf = ServiceRegistry.Get<DataService>().DataOperationFromEndpoint<T>(_dataEndpoint, Value, onCompleteCb: _OnCompleteCb, onErrorCb: _OnErrorCb);
		dopf.Load();
	}
	public override void Save()
	{
		var dopf = ServiceRegistry.Get<DataService>().DataOperationFromEndpoint<T>(_dataEndpoint, Value, onCompleteCb: _OnCompleteCb, onErrorCb: _OnErrorCb);
		dopf.Save();
	}

	public void _OnCompleteCb(IEvent e)
	{
		if (e is DataOperationComplete ee)
		{
			if (ee.RunWorkerCompletedEventArgs.Result is DataOperationResult<T> resultObj)
			{
				// if (RawValue is T co)
				// {
				// 	co.MergeFrom((T) resultObj.ResultObject);
				// }

				// LoggerManager.LogDebug("Config object merged with loaded object", "", "configObj", RawValue);

				// overwrite created object's values with loaded object
				if (resultObj.ResultObject is T)
				{
					RawValue = resultObj.ResultObject;
				}

				_loading = false;

				this.Emit<DataOperationComplete>((eee) => {
						eee.SetRunWorkerCompletedEventArgs(ee.RunWorkerCompletedEventArgs);
					});
			}
		}
	}
	public void _OnErrorCb(IEvent e)
	{
		_loading = false;

		if (e is DataOperationError ee)
		{
			this.Emit<DataOperationError>((eee) => {
					eee.SetRunWorkerCompletedEventArgs(ee.RunWorkerCompletedEventArgs);
				});
		}
	}
}
