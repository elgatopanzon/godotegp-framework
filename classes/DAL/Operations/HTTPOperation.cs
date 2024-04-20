namespace GodotEGP.DAL.Operations;

using GodotEGP.Logging;
using GodotEGP.DAL.Operators;
using GodotEGP.DAL.Endpoints;

// operation class for File operators
public partial class HTTPOperation<T> : DataOperation<T>
{
	HTTPOperator _dataOperator;

	public override IOperator CreateOperator()
	{
		var dataOperator = new HTTPOperator();

		dataOperator.OnComplete = __On_OperatorComplete;
		dataOperator.OnError = __On_OperatorError;

		return dataOperator;
	}

	public override Operator GetOperator()
	{
		return _dataOperator;
	}

	public HTTPOperation(HTTPEndpoint httpEndpoint, object dataObject = null)
	{
		LoggerManager.LogDebug($"Creating instance");
		LoggerManager.LogDebug($"httpEndpoint {httpEndpoint}");

		_dataObject = dataObject;

		// create instance of the operator
		_dataOperator = (HTTPOperator) CreateOperator();

		// set the data endpoint object
		_dataOperator.SetDataEndpoint(httpEndpoint);
	}

	public override void Load() {
		_dataOperator.Load();
		Loading();
	}

	public override void Save() {
		_dataOperator.Save(_dataObject);
		Saving();
	}
}

