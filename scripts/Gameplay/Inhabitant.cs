using Godot;
using System;
using System.Collections.Generic;
using System.Security;

public partial class Inhabitant : ElevatorUser
{
    private int m_insideDestination;
    private float m_patience = 1.0f;
    private float m_lastPatience = 1.0f;
    private UserSchedule m_schedule;
    public UserSchedule GetSchedule() { return m_schedule; }

    public Inhabitant(int buildingDestination, float walkSpeed) : base(walkSpeed)
    {
        m_insideDestination = buildingDestination;

        m_schedule = UserSchedule.Generate();
        if(m_schedule.ShouldLeave())
        {
            m_movementState = UserMovementState.Outside;
            m_destination = 0;
        }
        else
        {
            m_movementState = UserMovementState.Inside;
            m_destination = buildingDestination;
        }

        SetHorizontalTargetOuterSides();
        m_position.Y = m_destination;
        m_position.X = m_horizontalTarget;
    }

    protected override void ManageOutside()
    {
        if(m_schedule.ShouldBack() == false) // equivalent but clearer than ShouldLeave
            return;

        // User is outside and must come back, make him reach elevator floor
        SetHorizontalTargetNearestInside();
        m_destination = m_insideDestination;
        m_movementState = UserMovementState.Entering;
        m_elevatorState = UserElevatorState.Init;
    }

    protected override void ManageInside()
    {
        if(m_schedule.ShouldLeave() == false) // equivalent but clearer than ShouldBack
            return;

        // User is inside and must leave, make him reach elevator floor
        SetHorizontalTargetNearestInside();
        m_destination = 0;
        m_movementState = UserMovementState.Leaving;
        m_elevatorState = UserElevatorState.Init;
    }

    protected override void ManageLeaving(List<Elevator> elevators)
    {
        if(m_elevatorState != UserElevatorState.Leaving)
        {
            ManageElevatorRide(elevators);

            if(elevatorIndex > -1 && ShouldReCall())
                elevators[elevatorIndex].RequestFloor(m_destination);
        }
        else
        {
            SetHorizontalTargetOuterSides();
            m_movementState = UserMovementState.Outside;
        }

        if(m_schedule.ShouldBack())
        {
            m_patience = 0.0f;
        }
        else
        {
            int leaveTime = m_schedule.GetLeaveTimeInMinute();
            int backTime = m_schedule.GetBackTimeInMinute();
            ComputePatience(leaveTime, backTime);

            if(m_elevatorState == UserElevatorState.Waiting && ShouldReCall())
            {
                ElevatorCallManager.CallElevator(Mathf.RoundToInt(m_position.Y)); // send call again
            }
        }
    }

    protected override void ManageEntering(List<Elevator> elevators)
    {
        if(m_elevatorState != UserElevatorState.Leaving)
        {
            ManageElevatorRide(elevators);

            if(elevatorIndex > -1 && ShouldReCall())
                elevators[elevatorIndex].RequestFloor(m_destination);
        }
        else
        {
            SetHorizontalTargetOuterSides();
            m_movementState = UserMovementState.Inside;
        }

        if(m_schedule.ShouldLeave())
        {
            m_patience = 0.0f;
        }
        else
        {
            int leaveTime = m_schedule.GetLeaveTimeInMinute();
            int backTime = m_schedule.GetBackTimeInMinute();
            ComputePatience(backTime, leaveTime);

            if(m_elevatorState == UserElevatorState.Waiting && ShouldReCall())
            {
                ElevatorCallManager.CallElevator(Mathf.RoundToInt(m_position.Y)); // send call again
            }
        }
    }

    private void ComputePatience(int startMinute, int endMinute)
    {
        if(endMinute < startMinute)
            endMinute += 24 * 60; // Put back next day to make sure we only back after leaving

        int now = GameClockManager.clock.TimeOfDayInMinutes();

        if(now < startMinute)
            now += 24 * 60; // same idea

        m_lastPatience = m_patience;
        m_patience = 1.0f - Mathf.InverseLerp(startMinute, endMinute, now);
    }

    public bool ShouldReCall()
    {
        foreach(float t in UserManager.impatienceThresholds)
        {
            if(m_lastPatience > t && m_patience <= t)
                return true;
        }
        return false;
    }

    public override string GetDebugText()
    {
        string text = m_elevatorState + " - " + m_movementState + " " + Mathf.RoundToInt(m_position.Y) + "/" + m_destination;
        text += "\nLeaves: " + (m_schedule.leaveHour < 10 ? "0" : "") + m_schedule.leaveHour + ":" + (m_schedule.leaveMinute < 10 ? "0" : "") + m_schedule.leaveMinute
        + "\nBacks:   " + (m_schedule.backHour < 10 ? "0" : "") + m_schedule.backHour + ":" + (m_schedule.backMinute < 10 ? "0" : "") + m_schedule.backMinute;

        return text;
    }

    public override float GetPatience() { return m_patience; }
}
