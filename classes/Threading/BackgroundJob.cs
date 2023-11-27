namespace GodotEGP.Threading;

using System.ComponentModel;
using System;

using GodotEGP.Logging;
using GodotEGP.Event.Events;
using GodotEGP.Objects.Extensions;

public partial class BackgroundJob
{
	protected BackgroundWorker worker;
	public Action<DoWorkEventArgs> OnWorking;
	public Action<ProgressChangedEventArgs> OnProgress;
	public Action<RunWorkerCompletedEventArgs> OnComplete;
	public Action<RunWorkerCompletedEventArgs> OnError;

	public bool IsCompleted { get; set; }

	private RunWorkerCompletedEventArgs _completedArgs;

	public RunWorkerCompletedEventArgs CompletedArgs {
		get { return _completedArgs; }
	}

	public BackgroundJob()
	{
	}

	public void _setup()
	{
		worker = new BackgroundWorker();
		worker.DoWork += new DoWorkEventHandler(_On_DoWork);
		worker.ProgressChanged += new ProgressChangedEventHandler(_On_ProgressChanged);
		worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_On_RunWorkerCompleted);
		worker.WorkerReportsProgress = true;
		worker.WorkerSupportsCancellation = true;
	}

	public virtual void Run()
	{
		_setup();
		worker.RunWorkerAsync();
	}

	public virtual void ReportProgress(int progress)
	{
		worker.ReportProgress(progress);
	}

	// handlers for background worker events
	public virtual void _On_DoWork(object sender, DoWorkEventArgs e)
	{
		DoWork(sender, e);

		if (OnWorking != null)
		{
			OnWorking(e);
		}

		EmitEventDoWork(sender, e);
	}

	public virtual void _On_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		ProgressChanged(sender, e);

		if (OnProgress != null)
		{
			OnProgress(e);
		}

		EmitEventProgressChanged(sender, e);
	}

	public virtual void _On_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
	 	_completedArgs = e;

		if (e.Error != null)
		{
			_On_RunWorkerError(sender, e);
			return;
		}

		RunWorkerCompleted(sender, e);

		if (OnComplete != null)
		{
			OnComplete(e);
		}

		EmitEventRunWorkerCompleted(sender, e);

		IsCompleted = true;
	}

	public virtual void _On_RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		RunWorkerError(sender, e);

		if (OnError != null)
		{
			OnError(e);
		}

		EmitEventRunWorkerError(sender, e);
	}

	public virtual void EmitEventDoWork(object sender, DoWorkEventArgs e)
	{
		this.Emit<BackgroundJobWorking>((ev) => ev.SetDoWorkEventArgs(e));
	}

	public virtual void EmitEventProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		this.Emit<BackgroundJobProgress>((ev) => ev.SetProgressChangesEventArgs(e));
	}

	public virtual void EmitEventRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		this.Emit<BackgroundJobComplete>((ev) => ev.SetRunWorkerCompletedEventArgs(e));
	}

	public virtual void EmitEventRunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
		this.Emit<BackgroundJobError>((ev) => ev.SetRunWorkerCompletedEventArgs(e));
	}

	// override these to do the work
	public virtual void DoWork(object sender, DoWorkEventArgs e)
	{
	}

	public virtual void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
	}

	public virtual void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
	}

	public virtual void RunWorkerError(object sender, RunWorkerCompletedEventArgs e)
	{
	}
}
