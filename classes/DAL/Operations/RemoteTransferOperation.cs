/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : RemoteTransferOperation
 * @created     : Friday Feb 02, 2024 12:26:36 CST
 */

namespace GodotEGP.DAL.Operations;

using GodotEGP.DAL.Operators;
using GodotEGP.DAL.Endpoints;

using GodotEGP.Logging;

// operation class for File operators
public partial class RemoteTransferOperation<T> : DataOperation<T>
{
	RemoteTransferOperator _dataOperator;
	HTTPEndpoint _httpEndpoint;

	public override IOperator CreateOperator()
	{
		var dataOperator = new RemoteTransferOperator();

		dataOperator.OnComplete = __On_OperatorComplete;
		dataOperator.OnError = __On_OperatorError;

		return dataOperator;
	}

	public override Operator GetOperator()
	{
		return _dataOperator;
	}

	public RemoteTransferOperation(FileEndpoint fileEndpoint, HTTPEndpoint httpEndpoint = null)
	{
		LoggerManager.LogDebug($"Creating instance");
		LoggerManager.LogDebug($"fileEndpoint", "", "fileEndpoint", fileEndpoint);
		LoggerManager.LogDebug($"httpEndpoint", "", "httpEndpoint", httpEndpoint);

		_httpEndpoint = httpEndpoint;

		// create instance of the operator
		_dataOperator = (RemoteTransferOperator) CreateOperator();

		// set the data endpoint object
		_dataOperator.SetDataEndpoint(fileEndpoint);
		_dataOperator.SetHttpEndpoint(httpEndpoint);
	}

	public override void Load() {
		Working = true;
		_dataOperator.Load();
	}

	public override void Save() {
		Working = true;
		_dataOperator.Save();
	}
}
