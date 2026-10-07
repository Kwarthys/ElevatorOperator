using Godot;
using System;
using System.Collections.Generic;

public partial class ElevatorTechos : ElevatorUser
{
    private Elevator m_targetElevator;
    private bool m_jobsDone = false;
    public ElevatorTechos(float walkSpeed, Elevator targetElevator) : base(walkSpeed)
    {
        m_targetElevator = targetElevator;
        m_movementState = UserMovementState.Outside;

        SetHorizontalTargetOuterSides();
        m_position.Y = 0;
        m_position.X = m_horizontalTarget;
    }

    public bool IsJobDone() { return m_jobsDone; }

    protected override void ManageOutside() // setup elevator route to reach damaged elevator
    {
        if(m_jobsDone)
            return;

        m_destination = Mathf.RoundToInt(m_targetElevator.m_position);
        SetHorizontalTargetNearestInside();
        m_movementState = UserMovementState.Entering;

        if(m_destination == 0)
            m_elevatorState = UserElevatorState.Leaving; // Bypass most of the state machine
        else
            m_elevatorState = UserElevatorState.Init;

    }

    protected override void ManageInside() // here we repair the elevator, then leave
    {
        if(m_targetElevator.IsBroken())
        {
            m_targetElevator.Repair();
            return;
        }

        m_jobsDone = true;
        m_destination = 0;
        m_movementState = UserMovementState.Leaving;

        if(m_destination == m_position.Y)
        {
            m_elevatorState = UserElevatorState.Leaving; // Bypass most of the state machine
        }
        else
        {
            m_elevatorState = UserElevatorState.Init;
            SetHorizontalTargetNearestInside();
        }
    }

    protected override void ManageLeaving(List<Elevator> elevators)
    {
        if(m_elevatorState != UserElevatorState.Leaving)
        {
            ManageElevatorRide(elevators);
        }
        else
        {
            SetHorizontalTargetNearestOutside();
            m_movementState = UserMovementState.Outside;
        }
    }
    protected override void ManageEntering(List<Elevator> elevators)
    {
        if(m_elevatorState != UserElevatorState.Leaving)
        {
            ManageElevatorRide(elevators);
        }
        else
        {
            if(m_walking == false)
            {
                SetWalkTarget(m_targetElevator.GetHorizontalPos());
                return;
            }

            float distToElevator = Mathf.Abs(m_position.X - m_targetElevator.GetHorizontalPos());
            if(distToElevator < 0.05)
            {
                m_movementState = UserMovementState.Inside;
            }
        }
    }

}
