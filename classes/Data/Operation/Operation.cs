namespace GodotEGP.Data.Operation;

using System.ComponentModel;

using GodotEGP.Threading;
using GodotEGP.Logging;
using GodotEGP.Event.Events;
using GodotEGP.Data.Operator;
using GodotEGP.Objects.Extensions;

public abstract partial class Operation : BackgroundJob, IOperation
{
	public abstract void Load();
	public abstract void Save();
}

// base class for operation classes interfacing with operator classes
public abstract partial class Operation<T> : Operation, IOperation
{
	public abstract IOperator CreateOperator();
	public abstract Operator GetOperator();

	protected RunWorkerCompletedEventArgs _completedArgs;

	protected object _dataObject;

	public void __On_OperatorComplete(RunWorkerCompletedEventArgs e)
	{
		// once operator worker is completed, run the operation worker
		_completedArgs = e;
		Run();
	}
	public void __On_OperatorError(RunWorkerCompletedEventArgs e)
	{
		// forward the completed args to simulate an error
		_On_RunWorkerError(this, e);
	}

	// operation thread methods
	public override void DoWork(object sender, DoWorkEventArgs e)
	{
		LoggerManager.LogDebug("Starting operation thread");

		// for now, if the _dataObject is null then we can assume that this is a
		// load request, therefore we proceed to create the loaded instance
		try
		{
			OperationResult<T> resultObj = new OperationResult<T>(_completedArgs.Result);

			// LoggerManager.LogDebug($"Created object instance of {typeof(T).Name}", "", "object", resultObj);
			LoggerManager.LogDebug($"Created object instance of {typeof(T).Name}");

			if (resultObj.ResultObject != null)
			{
				e.Result = resultObj;
			}
		}
		catch (System.Exception)
		{
			// copy over the completed args from the operator thread
			e.Result = _completedArgs.Result;
		}

		ReportProgress(100);
	}

	public override void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		LoggerManager.LogDebug("Data operation thread progress", "", "progress", e.ProgressPercentage);
	}

	public override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Data operation thread completed");
	}

	public override void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Data operation thread error");
	}

	// override event methods to send different events
	public override void EmitEventDoWork(object sender, DoWorkEventArgs e)
	{
		this.Emit<DataOperationWorking>((ev) => ev.SetDoWorkEventArgs(e));
	}

	public override void EmitEventProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		this.Emit<DataOperationProgress>((ev) => ev.SetProgressChangesEventArgs(e));
	}

	public override void EmitEventRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		this.Emit<DataOperationComplete>((ev) => ev.SetRunWorkerCompletedEventArgs(e));
	}

	public override void EmitEventRunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		this.Emit<DataOperationError>((ev) => ev.SetRunWorkerCompletedEventArgs(e));
	}
}
