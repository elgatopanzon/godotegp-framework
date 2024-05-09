/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SystemScheduler
 * @created     : Wednesday May 08, 2024 16:21:39 CST
 */

namespace GodotEGP.ECSv3.Systems;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.ECSv3;
using GodotEGP.ECSv3.Components;
using GodotEGP.ECSv3.Queries;
using GodotEGP.ECSv3.Systems;

using System;
using System.Collections.Generic;

public partial class SystemScheduler
{
	private ECS _core;
	private SystemManager _systemManager;
	private QueryManager _queryManager;

	// holds query IDs for phase entity IDs
	private Dictionary<Entity, Entity> _phaseQueries;

	// the global delta time
	private double _deltaTime;

	// keep a rolling stack of process phase entities
	private ProcessPhaseList _processPhaseList;

	public SystemScheduler(ECS core, SystemManager systemManager, QueryManager queryManager)
	{
		_core = core;
		_systemManager = systemManager;
		_queryManager = queryManager;

		// set the default process phase list
		_processPhaseList = new();
		_processPhaseList.AddPhase(_core.RegisterComponent<OnStartupPhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<PreLoadPhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<PreUpdatePhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<OnUpdatePhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<PostUpdatePhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<FinalPhase>());

		_phaseQueries = new();

		// trigger a query creation for the new phase list
		SetProcessPhaseList(_processPhaseList);
	}

	// set the process phase list to a custom one
	public void SetProcessPhaseList(ProcessPhaseList processPhaseList)
	{
		_processPhaseList = processPhaseList;

		// create queries for each of the phases if they don't already exist
		while (_processPhaseList.TryNext(out Entity phaseEntity))
		{
			_core.Add<EcsProcessPhase>(phaseEntity);

			// skip making the query if it already exists
			if (_phaseQueries.ContainsKey(phaseEntity))
			{
				continue;
			}

			// create query to get all EcsSystem entities for this phase
			Entity e = _core.RegisterSystemQuery(_core.CreateQuery()
				.Has<EcsSystem>()
				.Has(phaseEntity)
				.Build()
				, $"phase_{_core.GetEntityName(phaseEntity)}");

			_phaseQueries.Add(phaseEntity, e);
		}

		// reset the phase list
		_processPhaseList.Reset();
	}

	/******************************
	*  System execution methods  *
	******************************/

	// update all registered systems
	public void Update(double deltaTime)
	{
		_deltaTime = deltaTime;

		// loop through each phase and run the query for the phase
		while (_processPhaseList.TryNext(out Entity phaseEntity))
		{
			// update systems for phase
			LoggerManager.LogDebug("Running update phase", "", "phase", _core.GetEntityName(phaseEntity));

			Span<Entity> systemEntities = _queryManager.QueryResults(_phaseQueries[phaseEntity]).Entities.Span;
			int systemCount = systemEntities.Length;

			for (int i = 0; i < systemCount; i++)
			{
				Run(systemEntities[i]);
			}

			LoggerManager.LogDebug("Finished update phase", "", "phase", _core.GetEntityName(phaseEntity));
		}

		_processPhaseList.Reset();
	}
	
	public void Run(Entity entity)
	{
		Run(_systemManager.GetSystemInstance(entity));
	}
	public void Run(string name)
	{
		Run(_systemManager.GetSystemInstance(name));
	}
	public void Run(SystemInstance system)
	{
		// if the system has a query id, then run it with a loop of the query
		// results
		if (system.QueryEntity != 0)
		{
			// check if the system's query is live
			Query query = _queryManager.GetQuery(system.QueryEntity);

			QueryResult results;

			// run the query if it's not live
			if (!query.IsLiveQuery)
			{
				results = _queryManager.RunQuery(query);
			}
			// if it's live, fetch the existing results
			else
			{
				results = _queryManager.QueryResults(system.QueryEntity);
			}

			// run the system with the resulting entity list
			_runSystem(system, results.Entities.Span);

			// set the last update time
			system.LastUpdateTime = DateTime.Now;
		}

		// if the system has no query, then run it without anything
		else
		{
			_updateSystem(system, 0, 0);

			// set the last update time
			system.LastUpdateTime = DateTime.Now;
		}
	}

	// run the system with the given entities
	public void _runSystem(SystemInstance system, Span<Entity> entities)
	{
		int length = entities.Length;

		if (length == 0)
		{
			return;
		}

		LoggerManager.LogDebug("Starting Systems update process", system.System.GetType().Name, "entityCount", length);

		for (int i = 0; i < length; i++)
		{
			_updateSystem(system, entities[i], i);
		}

		LoggerManager.LogDebug("Finished Systems update process", system.System.GetType().Name, "entityCount", length);
	}

	// run the Update() method for the given system instance
	public void _updateSystem(SystemInstance system, Entity entity, int index)
	{
		LoggerManager.LogDebug("Running System's update process", system.System.GetType().Name, "entity", entity);

		// call Update() on the system instance
		system.Update(entity, index, _core, _deltaTime);

		LoggerManager.LogDebug("Finished System's update process", system.System.GetType().Name, "entity", entity);
	}
}

public partial class ProcessPhaseList
{
	// hold a list of IEcsProcessPhase entities
	private PackedArray<Entity> _phases;

	// holds the current phase index
	private int _currentPhaseIndex;

	public ProcessPhaseList()
	{
		Clear();
	}

	// add a phase entity ID to the list
	public void AddPhase(Entity entity)
	{
		_phases.Add(entity);
	}

	// clear the phase list
	public void Clear()
	{
		_phases = new();
	}

	// get the next phase entity from the list until the end
	public bool TryNext(out Entity phase)
	{
		phase = 0;
		bool isLast = _currentPhaseIndex == _phases.Count;

		if (!isLast)
		{
			phase = _phases[_currentPhaseIndex];
			_currentPhaseIndex++;
		}

		return !isLast;
	}

	// reset the phase index to 0
	public void Reset()
	{
		_currentPhaseIndex = 0;
	}
}
