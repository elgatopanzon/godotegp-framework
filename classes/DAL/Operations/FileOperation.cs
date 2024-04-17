namespace GodotEGP.DAL.Operations;

using GodotEGP.DAL.Operator;
using GodotEGP.DAL.Endpoints;

using GodotEGP.Logging;

// operation class for File operators
public partial class FileOperation<T> : DataOperation<T>
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
		Working = true;
		_dataOperator.Load();
	}

	public override void Save() {
		Working = true;
		_dataOperator.Save(_dataObject);
	}
}
