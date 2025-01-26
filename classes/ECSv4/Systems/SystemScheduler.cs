/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SystemScheduler
 * @created     : Wednesday May 08, 2024 16:21:39 CST
 */

namespace GodotEGP.ECSv4.Systems;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Components;
using GodotEGP.ECSv4.Queries;
using GodotEGP.ECSv4.Systems;

using System;
using System.Collections.Generic;

public partial class SystemScheduler
{
	private ECS _core;
	private QueryManager _queryManager;

	// holds query IDs for phase entity IDs
	private Dictionary<int, Entity> _phaseQueries;

	// the global delta time
	private double _deltaTime;

	// keep a rolling stack of process phase entities
	private ProcessPhaseList _processPhaseList;

	public SystemScheduler(ECS core, QueryManager queryManager)
	{
		_core = core;
		_queryManager = queryManager;

		// set the default process phase list
		_processPhaseList = new();
		_processPhaseList.AddPhase(_core.RegisterComponent<OnStartupPhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<PreLoadPhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<PreUpdatePhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<OnUpdatePhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<PostUpdatePhase>());
		_processPhaseList.AddPhase(_core.RegisterComponent<FinalPhase>());
		_processPhaseList.Reset();

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
			if (_phaseQueries.ContainsKey(phaseEntity.Id))
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

		ComponentStore<EcsSystem> systemConfigComponents = _core.GetComponentStore<EcsSystem>();

		// loop through each phase and run the query for the phase
		foreach (var phase in _processPhaseList.Phases.Values)
		{
			// update systems for phase
			// LoggerManager.LogDebug("Running update phase", "", "phase", _core.GetEntityName(phaseEntity));

			QueryEntities entities = _queryManager.QueryEntities(_phaseQueries[phase]);
			foreach (var entity in entities.Entities)
			{
				EcsSystem systemConfig = systemConfigComponents.Get(entity);
				Query query;

				// fetch the query results for this system if it has one
				if (systemConfig.QueryEntity != 0)
				{
					// check if the system's query is live
					query = _queryManager.GetQuery(systemConfig.QueryEntity);

					// run the query if it's not live
					if (!query.IsLiveQuery)
					{
						_queryManager.RunQuery(query);
					}
				}
				else
				{
					query = default(Query);
				}

				systemConfig.UpdateAction(systemConfig.DeltaTime, _deltaTime, _core, query);
			}

			// LoggerManager.LogDebug("Finished update phase", "", "phase", _core.GetEntityName(phaseEntity));
		}

		// _processPhaseList.Reset();
	}
	
	// public void Run(Entity entity)
	// {
	// 	Run(_systemManager.GetSystemInstance(entity));
	// }
	// public void Run(string name)
	// {
	// 	Run(_systemManager.GetSystemInstance(name));
	// }
	// public void Run(SystemInstance system)
	// {
	// 	Query query;
    //
	// 	// fetch the query results for this system if it has one
	// 	if (system.QueryEntity != 0)
	// 	{
	// 		// check if the system's query is live
	// 		query = _queryManager.GetQuery(system.QueryEntity);
	// 		QueryResult results = query.Results;
    //
	// 		// run the query if it's not live
	// 		if (!query.IsLiveQuery)
	// 		{
	// 			_queryManager.RunQuery(query);
	// 		}
	// 	}
	// 	else
	// 	{
	// 		query = default(Query);
	// 	}
    //
	// 	// update the system
	// 	system.Update(_core, _deltaTime, query);
	// }

	// // run the system with the given entities
	// public void _runSystem(SystemInstance system, Query query)
	// {
	// 	// LoggerManager.LogDebug("Starting Systems update process", system.System.GetType().Name, "entityCount", query.Results.Entities.Count);
    //
	// 	foreach (var entity in query.Results.Entities)
	// 	{
	// 		_updateSystem(system, entity.Value, query);
	// 	}
    //
	// 	// LoggerManager.LogDebug("Finished Systems update process", system.System.GetType().Name, "entityCount", query.Results.Entities.Count);
	// }
    //
	// // run the Update() method for the given system instance
	// public void _updateSystem(SystemInstance system, Entity entity, Query query)
	// {
	// 	// LoggerManager.LogDebug("Running System's update process", system.System.GetType().Name, "entity", entity);
    //
	// 	// call Update() on the system instance
	// 	system.Update(entity, 0, _core, _deltaTime, query);
    //
	// 	// LoggerManager.LogDebug("Finished System's update process", system.System.GetType().Name, "entity", entity);
	// }
}

public partial class ProcessPhaseList
{
	// hold a list of IEcsProcessPhase entities
	private Dictionary<int, Entity> _phases;
	public Dictionary<int, Entity> Phases
	{
		get {
			return _phases;
		}
	}

	// holds the current phase index
	private int _currentPhaseIndex;

	public ProcessPhaseList()
	{
		Clear();
	}

	// add a phase entity ID to the list
	public void AddPhase(Entity entity)
	{
		_phases[_currentPhaseIndex] = entity;
		_currentPhaseIndex++;
	}

	// clear the phase list
	public void Clear()
	{
		_phases = new();
	}

	// get the next phase entity from the list until the end
	public bool TryNext(out Entity phase)
	{
		phase = default(Entity);
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
