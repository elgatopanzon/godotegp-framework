/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Chainable
 * @created     : Wednesday Mar 27, 2024 13:28:23 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Chainables.Exceptions;
using GodotEGP.Chainables.Extensions;

using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

public enum ExecutionMode
{
	Run = 0,
	Batch = 1,
	Stream = 2,
}

public partial class Chainable : IChainable
{
	private string _name;
	public string Name {
		get {
			if (_name == null)
			{
				return GetType().Name;
			}
			else
			{
				return _name;
			}
		}
	}

	public ExecutionMode ExecutionMode { get; set; }

	// input parameter and output parameter
	public object Input { get; set; }
	public object Output { get; set; }

	// stack of chainables to enable stack processing mode
	public List<IChainable> Stack { get; set; } = new();
	public object StackInput { get; set; }
	public object StackOutput { get; set; }
	public bool IsStack { 
		get {
			return (Stack.Count > 0);
		}
	}

	public bool _bufferedStream { get; set; } = false;
	public ChainableStreamBuffer _streamBuffer { get; set; } = new();

	public Chainable _chainableBuffer { get; set; }

	public Exception Error { get; set; }
	public bool Success { 
		get {
			return (Error == null);
		}
	}

	private ChainableSchema _schema;
	public ChainableSchema Schema {
		get {
			if (_schema == null)
			{
				_schema = ChainableSchema.BuildFromObject(this);
			}

			return _schema;
		}
	}
	public ChainableSchemaDefinition InputSchema {
		get {
			if (Stack.Count == 0)
			{
				return Schema.Input;
			}
			else
			{
				return Stack.First().InputSchema;
			}
		}
	}
	public ChainableSchemaDefinition OutputSchema {
		get {
			if (Stack.Count == 0)
			{
				return Schema.Output;
			}
			else
			{
				return Stack.Last().OutputSchema;
			}
		}
	}

	public ChainableConfig Config { get; set; } = new();
	public bool RunWithConfig { get; set; } = true;

	public bool TryInputAs<T>(out T c)
	{
		if (Input.TryCoerce<T>(out T coerced))
		{
			c = coerced;
			return true;
		}
		
		throw new NotSupportedException($"Unable to coerce Input to {typeof(T)}!");
	}
	public bool TryOutputAs<T>(out T c)
	{
		if (Output.TryCoerce<T>(out T coerced))
		{
			c = coerced;
			return true;
		}
		
		throw new NotSupportedException($"Unable to coerce Output to {typeof(T)}!");
	}

	public Chainable(object? input = null)
	{
		Input = input;
	}


	/****************************
	*  chain stacking methods  *
	****************************/

	public static Chainable operator |(IChainable a, Chainable b)
	{
		var stack = new Chainable();
		stack.RunWithConfig = false;

		foreach (var chainable in new List<IChainable>() { a, b })
		{
			// add the stack's chainables to the new stack
			if (chainable.Stack.Count > 0)
			{
				foreach (var c in chainable.Stack)
				{
					stack.Stack.Add(c);
				}
			}
			// if it's a normal chainable, add it to the stack
			else
			{
				stack.Stack.Add(chainable);
			}
		}

		// validate the schema of each item in the stack
		stack.ValidateChainSchemas();

		return stack;
	}

	public static IChainable operator |(Chainable a, Dictionary<string, object> b)
	{
		// create the chainable from the dictionary
		var chainableAssign = new ChainableAssign();

		LoggerManager.LogDebug("Creating ChainableAssign from dictionary");

		foreach (var assign in b)
		{
			LoggerManager.LogDebug("Assigning value to chainable", "", assign.Key, assign.Value.ToString());

			chainableAssign.Assign(assign.Key, assign.Value);
		}

		// return the stack
		return a | chainableAssign;
	}

	public static Chainable operator +(IChainable a, Chainable b)
	{
		return a | b;
	}


	/*****************************
	*  chain execution methods  *
	*****************************/

	public async virtual Task<object> RunWithoutConfig(object? input = null)
	{
		RunWithConfig = (!IsStack);

		LoggerManager.LogDebug("Running without config");

		// set the input object
		if (input != null)
		{
			Input = input;
		}

		// start the worker
		try
		{
			ValidateInputType(input);

			_HookPreRun();
			_EmitEventExecuting();

			Output = await _Process();

			_HookPostRun();

			ValidateOutputType(Output);
		}
		catch (System.Exception e)
		{
			if (HandleThrownException(e) != null)
			{
				throw;
			}
		}

		_HookPostExecution();
		_EmitEventFinished();

		// return the chainable result
		return Output;
	}

	public async virtual Task<object[]> BatchWithoutConfig(object[]? input = null)
	{
		LoggerManager.LogDebug("Batch running without config");

		// set the input object
		if (input != null)
		{
			Input = input;
		}

		_HookPreBatch();
		_EmitEventExecuting();

		List<object> outputs = new();
		int batchSize = ((object[]) Input).Count();
		foreach (var inputItem in (object[]) Input)
		{
			LoggerManager.LogDebug($"Executing batch run {outputs.Count + 1}/{batchSize}", "", "input", inputItem);

			try
			{
				ValidateInputType(inputItem);

				outputs.Add(await RunWithoutConfig(inputItem));

				ValidateOutputType(outputs.Last());

				LoggerManager.LogDebug($"Finished batch run {outputs.Count}/{batchSize}", "", "output", outputs.Last());
			}
			catch (System.Exception e)
			{
				if (HandleThrownException(e) != null)
				{
					throw;
				}
			}
		}

		RunWithConfig = (!IsStack);

		Output = outputs;

		LoggerManager.LogDebug("Batch output", "", "output", outputs);

		_HookPostBatch();
		_HookPostExecution();
		_EmitEventFinished();

		return outputs.ToArray();
	}

	public async virtual IAsyncEnumerable<object> StreamWithoutConfig(object? input = null)
	{
		LoggerManager.LogDebug("Streaming without config");

		RunWithConfig = (!IsStack);

		// set the input object
		if (input != null)
		{
			Input = input;
		}

		LoggerManager.LogDebug("Stream starting input", GetType().Name, "input", Input);
		LoggerManager.LogDebug("Stream starting input type", GetType().Name, "inputType", Input.GetType().Name);

		ValidateInputType(Input);

		// pass the process stream iterator to the transform method.
		// this allows performing a process, then transforming it e.g. a network
		// request with multiple outputs which need to be transformed directly
		// before they are returned.
		// the default process implementation just returns the iterator to allow
		// passthrough.
		LoggerManager.LogDebug("Stream supported", GetType().Name);

		_HookPreStream();
		_EmitEventExecuting();
		
		await foreach (var output in StreamTransform(_ProcessStream()))
		{
			ValidateOutputType(output);

			_HookPostStreamOutput(output);
			_EmitEventStreamOutput(output);

			yield return output;
		}

		_HookPostStream();
		// _EmitEventFinished();

		// TODO: figure out a way around this
		// try
		// {
		// 	await foreach (var output in StreamTransform(_ProcessStream()))
		// 	{
		// 		yield return output;
		// 	}
		// }
		// catch (System.Exception e)
		// {
		// 	if (HandleThrownException(e) != null)
		// 	{
		// 		throw;
		// 	}
		// }
	}

	public async Task<object> Run(object? input = null)
	{
		ExecutionMode = ExecutionMode.Run;
		_HookPreExecution();

		// using Config.Fields dictionary we can find any matching method
		// definition in the schema's parameters definition which satisfies the
		// requirements:
		// 1. no required parameters are missing from the fields values
		// 2. 1 or more optional parameters are set in the fields values
		var paramsAndMethod = GetInvokeParamsAndMethod("Run", input);
		SetClassPropertiesFromCustomParams();

		if (!RunWithConfig)
		{
			paramsAndMethod.Method = null;
		}

		// return the invoked object
		if (paramsAndMethod.Method != null)
		{
			RunWithConfig = false;

			return await (Task<object>) paramsAndMethod.Method.Invoke(this, paramsAndMethod.Params);
		}

		return await RunWithoutConfig(input);
	}

	public async virtual Task<object[]> Batch(object[]? input = null)
	{
		ExecutionMode = ExecutionMode.Batch;
		_HookPreExecution();

		// using Config.Fields dictionary we can find any matching method
		// definition in the schema's parameters definition which satisfies the
		// requirements:
		// 1. no required parameters are missing from the fields values
		// 2. 1 or more optional parameters are set in the fields values
		var paramsAndMethod = GetInvokeParamsAndMethod("Batch", input);
		SetClassPropertiesFromCustomParams();

		if (!RunWithConfig)
		{
			paramsAndMethod.Method = null;
		}

		// return the invoked object
		if (paramsAndMethod.Method != null)
		{
			RunWithConfig = false;

			return await (Task<object[]>) paramsAndMethod.Method.Invoke(this, paramsAndMethod.Params);
		}

		return await BatchWithoutConfig(input);
	}

	public async virtual IAsyncEnumerable<object> Stream(object? input = null)
	{
		ExecutionMode = ExecutionMode.Stream;
		_HookPreExecution();

		var paramsAndMethod = GetInvokeParamsAndMethod("Stream", input);
		SetClassPropertiesFromCustomParams();

		var enumerable = StreamWithoutConfig(input);

		if (!RunWithConfig)
		{
			paramsAndMethod.Method = null;
		}

		if (paramsAndMethod.Method != null)
		{
			enumerable = (IAsyncEnumerable<object>) paramsAndMethod.Method.Invoke(this, paramsAndMethod.Params);
		}

		RunWithConfig = false;
		await foreach (var output in enumerable)
		{
			yield return output;
		}

		_HookPostExecution();
	}

	public void SetClassPropertiesFromCustomParams()
	{
		foreach (var param in Config.Params)
		{
			if (InputSchema.Properties.ContainsKey(param.Key))
			{
				PropertyInfo prop = this.GetType().GetProperty(param.Key);
				prop.SetValue(this, param.Value, null);
			}
		}
	}

	public (object[] Params, MethodInfo Method) GetInvokeParamsAndMethod(string runMethodName, object input = null)
	{
		List<object> methodInvokeParams = new() {  };
		MethodInfo methodInfo = null;

		Config.Params["input"] = input;

		// match up configurable param values and set them as their method param
		// values
		foreach (var configurable in Config.ConfigurableParams)
		{
			if (Config.Params.TryGetValue(configurable.Value.Id, out var cValue))
			{
				Config.Params[configurable.Key] = cValue;

				LoggerManager.LogDebug("Setting param from configurable param", "", configurable.Key, $"{configurable.Value.Id} ({cValue})");
			}
		}

		LoggerManager.LogDebug("Chainable config", "", "config", Config);

		// set the method params to invoke by matching field names and types to
		// method parameter definitions
		if (Config.Params.Count > 0)
		{
			// loop over each parameter and find any matching methods with the
			// same params, short listing them until we're left with a matching
			// one or none
			List<MethodInfo> matchingMethods = new();
			Dictionary<string, List<MethodInfo>> matchingMethodsTemp = new();
			int matchingParamsCount = 0;

			foreach (var param in Config.Params)
			{
				matchingMethodsTemp[param.Key] = new();

				bool isMatchingParam = false;

				foreach (var methods in InputSchema.Parameters[runMethodName])
				{
					// skip generic methods
					if (methods.MethodInfo.IsGenericMethod)
					{
						LoggerManager.LogDebug("Skipping generic method", "", "method", methods.MethodInfo.ToString());
						continue;
					}
					// create a matching list of params which match the name of
					// the current config param, have the same type, or can be
					// converted to the provided parameter type
					var matching = methods.Parameters.Where(x => x.Name == param.Key && (x.Type == param.Value.GetType() || new Type[] {typeof(object), typeof(object[]), typeof(IEnumerable<object>), typeof(List<object>)}.Contains(x.Type)));

					if (matching.Count() > 0)
					{
						// LoggerManager.LogDebug($"Found {matching.Count()} matching {runMethodName}() method(s) for {param.Key} on {methods.Name}", "", param.Key, methods.MethodInfo.ToString());

						// add it to the matching methods
						if (!matchingMethodsTemp[param.Key].Contains(methods.MethodInfo))
						{
							matchingMethodsTemp[param.Key].Add(methods.MethodInfo);
						}

						isMatchingParam = true;
					}
					else
					{
						// LoggerManager.LogDebug($"Found NO matching {runMethodName}() method(s) for {methods.Name}", "", param.Key, param.Value.GetType());

						if (matchingMethodsTemp[param.Key].Contains(methods.MethodInfo))
						{
							LoggerManager.LogDebug("Removing non-matching temp method", "", "method", methods.MethodInfo.ToString());

							matchingMethodsTemp[param.Key].Remove(methods.MethodInfo);
						}

						// check if there's any name matches but type mismatches
						if (methods.Parameters.Where(x => x.Name == param.Key).Count() > 0)
						{
							// TODO: fix this/improve this, because technically
							// it's wrong considering that it just doesn't match
							// for this method we are currently on!
							// throw new ChainableConfigParamsTypeMismatchException($"The configured parameter '{param.Key} ({param.Value.GetType().Name})' does not match any types for {runMethodName}() method on {this.GetType()}!");			
							//
							// what we'll do is include it as a matching method
							// anyway, which will trigger an invoke exception
							// during the chain
							matchingMethodsTemp[param.Key].Add(methods.MethodInfo);
							isMatchingParam = true;
						}
					}
				}

				if (isMatchingParam)
				{
					matchingParamsCount++;
				}
			}

			Dictionary<MethodInfo, int> methodSeenCount = new();
			foreach (var methods in matchingMethodsTemp)
			{
				LoggerManager.LogDebug("Matching methods for param", "", "param", methods.Key);

				foreach (var method in methods.Value)
				{
					if (!methodSeenCount.TryGetValue(method, out var c))
					{
						methodSeenCount[method] = 0;
					}

					methodSeenCount[method]++;

					// LoggerManager.LogDebug(method.Name, methodSeenCount[method].ToString(), "method", method.ToString());

					if (methodSeenCount[method] >= matchingParamsCount)
					{
						// LoggerManager.LogDebug($"{method.Name} matches!", "", "method", method.ToString());
						matchingMethods.Add(method);
					}
				}
			}

			LoggerManager.LogDebug("Matching methods count", "", "matchCount", matchingMethods.Count);

			// pick out the last matching method
			// TODO: fix this to pick out methods ordered by counts of object
			// types as least desirable so that we prioritise the typed methods
			if (matchingMethods.Count > 0)
			{
				methodInfo = matchingMethods.Last();

				foreach (var param in methodInfo.GetParameters())
				{
					if (Config.Params.TryGetValue(param.Name, out var p))
					{
						methodInvokeParams.Add(p);
					}
					else
					{
						methodInvokeParams.Add(param.DefaultValue);
					}
				}

				LoggerManager.LogDebug("Invoke params", "", "params", methodInvokeParams);
			}

			// if there's no matching methods, then throw type mismatch
			// exception
			else
			{
				throw new ChainableConfigParamsTypeMismatchException($"No matching methods for provided parameters for {this.GetType().Name}!");
			}
		}

		return (methodInvokeParams.ToArray(), methodInfo);
	}

	public bool CanConvertType(object from, Type typeTo)
	{
		try
		{
			Convert.ChangeType(from, typeTo);

			return true;
		}
		catch (System.Exception)
		{
			return false;
		}
	}


	/*************************************
	*  generic chain execution methods  *
	*************************************/
	
	public async virtual Task<T2> Run<T1, T2>(object? input)
	{
		var result = await Run(input.Coerce<T1>());
		return result.Coerce<T2>();
	}

	public async virtual Task<List<T2>> Batch<T1, T2>(List<object>? batchInput)
		where T1 : notnull, new()
		where T2 : notnull, new()
	{
		var result = await Batch(batchInput.ToArray().CoerceArray<object, T1>().CoerceArray<T1, object>());
		return result.CoerceArray<object, T2>().ToList();
	}

	public async virtual Task<List<T2>> Batch<TInput, T1, T2>(List<TInput>? batchInput)
		where T1 : notnull, new()
		where T2 : notnull, new()
	{
		return await Batch<T1, T2>(batchInput.Coerce<List<object>>());
	}

	public async virtual IAsyncEnumerable<T2> Stream<T1, T2>(object? input)
	{
		await foreach (var output in Stream(input.Coerce<T1>()))
		{
			yield return output.Coerce<T2>();
		}
	}

	/***************************
	*  chain process methods  *
	***************************/
	public async virtual Task<object> _Process()
	{
		if (Stack.Count > 0)
		{
			LoggerManager.LogDebug("Processing chainable stack", GetType().Name, "stackSize", Stack.Count);

			// reset input and output state
			StackInput = null;
			StackOutput = null;
			Output = null;

			_HookPreStack();

			foreach (var chainable in Stack)
			{
				// if the stack output is set, then use it as the chained input
				if (StackOutput != null)
				{
					StackInput = StackOutput;
				}
				// if there's no stack output, then use this chain's input as the
				// starter
				else
				{
					StackInput = Input;
				}

				// merge the current config with the next chainable
				chainable.Config.Merge(this.Config);

				// start the chainable with the stack input
				StackOutput = await chainable.Run(StackInput);

				// merge the output config with this one
				Config.Merge(chainable.Config);

				_HookNextStackItem();

				LoggerManager.LogDebug("Finished stack item output", GetType().Name, "chainable", StackOutput);
			}

			// this chain's output is now the last stack item's output
			Output = StackOutput;

			_HookPostStack();

			LoggerManager.LogDebug("Finished processing stack!", GetType().Name);

			return StackOutput;
		}
		else
		{
			if (HasOwnProcessStreamMethod() || HasOwnStreamTransformMethod())
			{
				await foreach (var output in StreamWithoutConfig(Input))
				{
					_streamBuffer.Add(output);
				}
				
				// if running in batch mode with a streaming object, instead of
				// merging the batch output assign the output to the last
				// buffered item
				if (ExecutionMode != ExecutionMode.Batch)
				{
					Output = _streamBuffer.Merge();
				}
				else
				{
					Output = _streamBuffer._objects.Last();
				}
			}
			else
			{
				LoggerManager.LogDebug("Chainable passthrough", GetType().Name, "input", Input);

				// if (ExecutionMode != ExecutionMode.Batch)
				// {
					Output = Input;
				// }
			}

			return Output;
		}
	}


	/*****************************
	*  chain streaming methods  *
	*****************************/

	public async IAsyncEnumerable<object> _ProcessStreamStep(IChainable currentChainable, object input)
	{
		var currentIdx = Stack.IndexOf(currentChainable);
		var nextIdx = currentIdx + 1;

		if (Stack.Count > nextIdx)
		{
			var nextChainable = Stack[nextIdx];

			LoggerManager.LogDebug("Step: streaming to next chainable", GetType().Name, "nextType", nextChainable.GetType().Name);

			LoggerManager.LogDebug("Step: current step input", GetType().Name, "input", input);
			LoggerManager.LogDebug("Step: current stack input", GetType().Name, "stackInput", StackInput);
			
			LoggerManager.LogDebug("Step: has own _ProcessStream()", GetType().Name, "processStream", nextChainable.HasOwnProcessStreamMethod());
			LoggerManager.LogDebug("Step: has own StreamTransform()", GetType().Name, "processStream", nextChainable.HasOwnStreamTransformMethod());

			if (!nextChainable.HasOwnProcessStreamMethod() && !nextChainable.HasOwnStreamTransformMethod())
			{
				LoggerManager.LogDebug("Step: next chainable not stream native", GetType().Name, "chainableType", nextChainable.GetType().Name);

				_bufferedStream = true;
			}

			// merge the current config with the next chainable
			nextChainable.Config.Merge(this.Config);

			_HookNextStackItem();

			if (!_bufferedStream)
			{

				await foreach (var output in nextChainable.Stream(input))
				{
					Input = output;
					await foreach (var nested in _ProcessStreamStep(nextChainable, Input))
					{
						yield return nested;
					}
				}
			}
			else
			{

				if (_chainableBuffer == null)
				{
					LoggerManager.LogDebug("Step: buffering remaining chainables", GetType().Name);

					var nextStackIdx = Stack.IndexOf(nextChainable);
					_chainableBuffer = new();
					_chainableBuffer.Stack = Stack.GetRange(nextStackIdx, (Stack.Count - nextStackIdx));

					yield return input;
				}

				else
				{
					await foreach (var output in nextChainable.Stream(input))
					{
						yield return output;
					}
				}
			}

			this.Config.Merge(nextChainable.Config);
		}
		else
		{
			LoggerManager.LogDebug("Step: yielding final value", GetType().Name, "value", input);

			yield return input;
		}
	}

	public async virtual IAsyncEnumerable<object> _ProcessStream()
	{
		// if we're in stack mode then we manage the streaming
		if (Stack.Count > 0)
		{
			// Stream(): returns an async iterator, default implementation is to
			// yield the result of _Run() with the input
			// Real implementations will override _ProcessStream() to yield
			// streamed outputs

			// StreamTransform(): accepts an iterator as input and returns an
			// iterator, with the default implementation being to buffer the
			// outputs of the input iterator, then to await our implementation
			// of Stream()
			
			LoggerManager.LogDebug("Streaming chainable stack", GetType().Name, "stackSize", Stack.Count);
			LoggerManager.LogDebug("Streaming chainable stack input", GetType().Name, "input", Input);

			// merge the current config with the next chainable
			Stack.First().Config.Merge(this.Config);

			_HookPreStack();

			// start the streaming process for the first item, then follow up
			// with the recursive step processor
			await foreach (var output in Stack.First().Stream(Input))
			{
				LoggerManager.LogDebug("First streaming stack output", GetType().Name, "output", output);

				this.Config.Merge(Stack.First().Config);

				Input = output;

				_HookNextStackItem();

				await foreach (var step in _ProcessStreamStep(Stack.First(), Input))
				{
					LoggerManager.LogDebug("Step streaming stack output", GetType().Name, "step", step);

					if (!_bufferedStream)
					{
						yield return step;
					}
					else
					{
						LoggerManager.LogDebug("Ignoring streaming step due to buffered streaming", GetType().Name, "step", step);

						Output = step;

						_streamBuffer.Add(Output);
					}
				}
			}

			// if a non-streaming chainable is encounted, then the inputs from
			// the stream are buffered. this is because non-streaming chainables
			// are designed to accept only a final input, so we must buffer the
			// output then pass it as the finalised input
			if (_bufferedStream)
			{
				LoggerManager.LogDebug("Buffered chainables stack size", GetType().Name, "bufferedStackSize", _chainableBuffer.Stack.Count);
				LoggerManager.LogDebug("Buffered stream output", GetType().Name, "bufferedOutput", _streamBuffer);

				// merge the current config with the next chainable
				_chainableBuffer.Config.Merge(this.Config);

				_HookNextStackItem();

				// TODO: proper way to concat the _streamBuffer which is
				// required for native objects and other types
				await foreach (var buffered in _chainableBuffer.StreamWithoutConfig(_streamBuffer.Merge()))
				{
					yield return buffered;
				}

				this.Config.Merge(_chainableBuffer.Config);
			}

			_HookPostStack();
		}

		// if we're not in stack mode, then this is a default implementation of
		// streaming which means this chainable doesn't support streaming and we
		// have to instead work with the non-streamed _Process() method?
		else
		{
			if (!HasOwnStreamTransformMethod())
			{
				LoggerManager.LogDebug("Process stream passthrough _Process()", GetType().Name, "input", Input);

				yield return await _Process();
			}
			else
			{
				LoggerManager.LogDebug("Process stream passthrough input", GetType().Name, "input", Input);

				yield return Input;
			}
		}
	}

	public async IAsyncEnumerable<object> _ProcessStreamWrapper()
	{
		yield return await _Process();
	}

	public bool HasOwnProcessStreamMethod()
	{
		var streamProcessMethodInfo = this.GetType().GetMethod("_ProcessStream");
		var streamProcessSupported = streamProcessMethodInfo.GetBaseDefinition().DeclaringType != streamProcessMethodInfo.DeclaringType;

		return streamProcessSupported;
	}

	public bool HasOwnStreamTransformMethod()
	{
		var streamTransformMethodInfo = this.GetType().GetMethod("StreamTransform");
		var streamTransformSupported = streamTransformMethodInfo.GetBaseDefinition().DeclaringType != streamTransformMethodInfo.DeclaringType;

		return streamTransformSupported;
	}

	public async virtual IAsyncEnumerable<object> StreamTransform(IAsyncEnumerable<object> input = null)
	{
		LoggerManager.LogDebug("Stream transform passthrough implementation", GetType().Name);

		// loop over provided input enumerator
		// override this method to do a transformation with the streamed input
		// and stream the output
		await foreach (var output in input)
		{
			LoggerManager.LogDebug("Streaming passthrough input value", GetType().Name, "output", output);

			yield return output;
		}
	}

	
	/**************************************
	*  validation and object management  *
	**************************************/

	public bool ValidateInputType(object obj)
	{
		var res = ValidateObjectTypeMatchesSchema(obj, false);

		if (!res)
		{
			throw new ChainableInputSchemaTypeException($"'{obj.GetType()}' not a valid input type for '{GetType().Name}'!");			
		}

		return res;
	}
	public bool ValidateOutputType(object obj)
	{
		var res = ValidateObjectTypeMatchesSchema(obj, true);

		if (!res)
		{
			throw new ChainableOutputSchemaTypeException($"'{obj?.GetType()}' not a valid output type for '{GetType().Name}'!");			
		}

		return res;
	}

	public bool ValidateObjectTypeMatchesSchema(object obj, bool output = false)
	{
		return Schema.ObjectIsValidType(obj, output:output);
	}
	
	// check the input and output schemas of each chainable to spot incompatible
	// inputs/outputs
	public void ValidateChainSchemas()
	{
		for (int i = 0; i < Stack.Count; i++)
		{
			var inputChainable = Stack[i];

			if (Stack.Count > i+1)
			{
				var outputChainable = Stack[i+1];

				// check if the input chainable's output is present in the
				// output chainable's input schema
				bool validChainLink = (inputChainable.OutputSchema.Types.Count == 0 || outputChainable.OutputSchema.Types.Count == 0);
				foreach (var type in inputChainable.OutputSchema.Types)
				{
					if (outputChainable.Schema.IsValidType(type, output:false))
					{
						validChainLink = true;
					}
				}

				if (!validChainLink)
				{
					throw new ChainableChainSchemaMismatchException($"Chainable {i+1} ({inputChainable.GetType().Name}) => {i+2} ({outputChainable.GetType().Name}) does not match output->input schema!");
				}
			}
		}
	}

	public IChainable Clone()
	{
		// create the clone
		var clone = (IChainable) this.MemberwiseClone();

		// clone the reference type objects
		clone.Config = this.Config.Clone();
		clone.Stack = new(this.Stack);

		// reset temp object values
		clone.Reset();

		LoggerManager.LogDebug("Created clone of object", "", "objectType", clone.GetType());

		return clone;
	}

	public void Reset()
	{
		// reset operational values
		Error = null;
		RunWithConfig = (!IsStack);

		// reset stream buffer objects
		_bufferedStream = false;
		_streamBuffer = new();
		_chainableBuffer = new();

		// reset input/output objects
		Input = null;
		Output = null;
		StackInput = null;
		StackOutput = null;
	}

	public async Task<object> TryGetChainableResult(object mightBeAChainable, object input)
	{
		if (mightBeAChainable is IChainable chainable)
		{
			return await chainable.Run(input);
		}
		else
		{
			return mightBeAChainable;
		}
	}


	/********************************
	*  exception & error handling  *
	********************************/
	
	public virtual Exception? HandleThrownException(Exception e)
	{
		Error = e;

		// if we encountered an exception during operating a stack, then we need
		// to evaluate it
		if (IsStack)
		{
			LoggerManager.LogDebug("Exception thrown during chain step", GetType().Name, "e", e);

			_EmitEventError();

			return null;
		}

		// if we encountered an exception from execution, then we need to set
		// Error and throw it
		else
		{
			LoggerManager.LogDebug("Exception thrown by execution", GetType().Name, "e", e);

			_EmitEventError();

			return Error;
		}
	}


	/******************
	*  hook methods  *
	******************/

	// executed before any execution is done
	public virtual void _HookPreExecution()
	{
		
	}

	// executed after the execution has been done
	public virtual void _HookPostExecution()
	{
		
	}
	
	// executed before the Run() method executes _Process()
	public virtual void _HookPreRun()
	{
		
	}

	// executed after the Run() method executes _Process()
	public virtual void _HookPostRun()
	{
		
	}

	// executed before the Batch() method executes Run()
	public virtual void _HookPreBatch()
	{
		
	}

	// executed after the Batch() method executes Run()
	public virtual void _HookPostBatch()
	{
		
	}

	// executed before the Stream() method calls _StreamTransform()
	public virtual void _HookPreStream()
	{
		
	}

	// executed after the Stream() method calls _StreamTransform()
	public virtual void _HookPostStream()
	{
		
	}

	// executed before the Stream() method yields the output
	public virtual void _HookPostStreamOutput(object output)
	{
		
	}

	// executed before a stack is about to be processed
	public virtual void _HookPreStack()
	{
		
	}

	// executed after a stack has been processed
	public virtual void _HookPostStack()
	{
		
	}

	// executed before the next stack item
	public virtual void _HookNextStackItem()
	{
		
	}

	/************************
	*  event emit methods  *
	************************/
	public void _EmitEventExecuting()
	{
		this.Emit<EventChainableExecuting>((e) => e.Input = Input);
	}
	public void _EmitEventFinished()
	{
		this.Emit<EventChainableFinished>((e) => e.Output = Output);
	}
	public void _EmitEventError()
	{
		this.Emit<EventChainableError>();
	}
	public void _EmitEventStreamOutput(object output)
	{
		this.Emit<EventChainableStreamOutput>((e) => e.Output = output);
	}
	
}
