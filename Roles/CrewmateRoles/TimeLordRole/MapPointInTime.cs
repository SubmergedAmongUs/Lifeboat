using Submerged.Map.MonoBehaviours;
using Submerged.Systems.CustomSystems.Elevator;
using Submerged.Systems.CustomSystems.Elevator.Enums;

namespace Lifeboat.Roles.CrewmateRoles.TimeLordRole;

public struct MapPointInTime
{
    public static MapPointInTime Create(float time)
    {
        if (!SubmarineStatus.Instance) return new MapPointInTime();
            
        ElevatorState[] states = new ElevatorState[SubmarineStatus.Instance.Elevators.Count];
        for (int i = 0; i < states.Length; i++)
        {
            states[i] = new ElevatorState(SubmarineStatus.Instance.Elevators[i].System);
        }

        return new MapPointInTime
        {
            Time = time,
            States = states
        };
    }

    public void Rewind()
    {
        if (!SubmarineStatus.Instance) return;

        foreach (ElevatorState state in States)
        {
            state.System.LastStage = state.LastStage;
            state.System.LerpTimer = state.LerpTimer;
            state.System.Moving = state.Moving;
            state.System.TotalTimer = state.TotalTimer;
            state.System.UpperDeckIsTargetFloor = state.UpperDeckIsTargetFloor;
        }
    }

    public float Time;
    private ElevatorState[] States;
}
    
public struct ElevatorState
{
    public ElevatorMovementStage LastStage;
    public float LerpTimer;
    public bool Moving;
    public SubmarineElevatorSystem System;
    public float TotalTimer;

    public bool UpperDeckIsTargetFloor; // This is also the floor it is currently on if not moving

    public ElevatorState(SubmarineElevatorSystem system)
    {
        UpperDeckIsTargetFloor = system.UpperDeckIsTargetFloor;
        Moving = system.Moving;
        TotalTimer = system.TotalTimer;
        LerpTimer = system.LerpTimer;
        LastStage = system.LastStage;
        System = system;
    }
}