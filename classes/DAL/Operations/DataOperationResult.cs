namespace GodotEGP.DAL.Operations;

using System.Collections.Generic;
using Newtonsoft.Json;

using GodotEGP.Objects.Validated;
using GodotEGP.Logging;

using GodotEGP.Objects.Extensions;

// accept a result object and create a ValidatedObject from T
public partial class DataOperationResult<T>
{
	private T _resultObject;

	public T ResultObject
	{
		get { return _resultObject; }
		set { _resultObject = value; }
	}

	public DataOperationResult(object rawObject)
	{
		// LoggerManager.LogDebug("Creating result object from raw data", "", "raw", rawObject);
		if (rawObject is null)
		{
			LoggerManager.LogDebug("Result object null");
			return;
		}

		LoggerManager.LogDebug("Creating result object", "", "rawType", rawObject.GetType().Name);

		if (typeof(T).IsSubclassOf(typeof(VObject)) && rawObject is string)
		{
			// hold deserialisation errors
			List<string> errors = new List<string>();

			// create deserialised T object, for now it only supports strings of
			// JSON
			T deserialisedObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>((string) rawObject,
				new JsonSerializerSettings
    			{
        			Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
        			{
            			errors.Add(args.ErrorContext.Error.Message);
            			args.ErrorContext.Handled = true;
        			},
        			ObjectCreationHandling = ObjectCreationHandling.Replace
    			}
			);
			
			// LoggerManager.LogDebug($"{typeof(T).BaseType} object deserialised as {typeof(T).Name}", "", "object", deserialisedObj);
			LoggerManager.LogDebug($"{typeof(T).BaseType} object deserialised as {typeof(T).Name}");

			// store the deserialsed object
			ResultObject = deserialisedObj;
		}

		else
		{
			if (rawObject.TryCast<T>(out T casted))
			{
				ResultObject = casted;
			}
		}
	}
}

