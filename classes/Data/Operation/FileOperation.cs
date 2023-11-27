namespace GodotEGP.Data.Operation;

using GodotEGP.Data.Operator;
using GodotEGP.Data.Endpoint;

using GodotEGP.Logging;

// operation class for File operators
public partial class FileOperation<T> : Operation<T>
{
	FileOperator _dataOperator;

	public override IOperator CreateOperator()
	{
		var dataOperator = new FileOperator();

		dataOperator.OnComplete = __On_OperatorComplete;
		dataOperator.OnError = __On_OperatorError;

		return dataOperator;
	}

	public override Operator GetOperator()
	{
		return _dataOperator;
	}

	public FileOperation(FileEndpoint fileEndpoint, object dataObject = null)
	{
		LoggerManager.LogDebug($"Creating instance");
		LoggerManager.LogDebug($"fileEndpoint {fileEndpoint}");

		_dataObject = dataObject;

		// create instance of the operator
		_dataOperator = (FileOperator) CreateOperator();

		// set the data endpoint object
		_dataOperator.SetDataEndpoint(fileEndpoint);
	}

	public override void Load() {
		_dataOperator.Load();
	}

	public override void Save() {
		_dataOperator.Save(_dataObject);
	}
}
