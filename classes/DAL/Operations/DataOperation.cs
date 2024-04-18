namespace GodotEGP.DAL.Operations;

using System.ComponentModel;

using GodotEGP.Threading;
using GodotEGP.Logging;
using GodotEGP.Event.Events;
using GodotEGP.DAL.Operator;
using GodotEGP.Objects.Extensions;

public abstract partial class DataOperation : BackgroundJob, IDataOperation
{
	public bool Working { get; set; } = false;
	public bool IsLoad { get; set; }
	public abstract void Load();
	public abstract void Save();
	public abstract void Loading();
	public abstract void Saving();
}

// base class for operation classes interfacing with operator classes
public abstract partial class DataOperation<T> : DataOperation, IDataOperation
{
	public abstract IOperator CreateOperator();
	public abstract Operator GetOperator();

	protected RunWorkerCompletedEventArgs _completedArgs;

	protected object _dataObject;

	public bool Completed { get; set; } = false;

	public override void Loading()
	{
		Working = true;
		IsLoad = true;
	}
	public override void Saving()
	{
		Working = true;
		IsLoad = false;
	}

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
		LoggerManager.LogDebug("Starting operation thread", "", "isLoad", IsLoad);

		try
		{
		 DataOperationResult<T> resultObj = new DataOperationResult<T>(_completedArgs.Result);

			e.Result = resultObj;

			if (resultObj.ResultObject != null)
			{
				LoggerManager.LogDebug($"Created object instance of {typeof(T).Name}");
			}
			else
			{
				// don't allow null results while loading
				if (IsLoad)
				{
					LoggerManager.LogDebug($"Failed to create instance of {typeof(T).Name}");

					throw new System.IO.InvalidDataException("Result object is null");
				}
			}
		}
		catch (System.Exception ex)
		{
			// copy over the completed args from the operator thread
			e.Result = _completedArgs.Result;

			_error = ex;
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

		Completed = true;
		Working = false;
	}

	public override void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Data operation thread error");

		Completed = true;
		Working = false;
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
