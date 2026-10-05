using Godot;
using System;
using System.Collections.Generic;

public class ElevatorUser
{
    public enum UserElevatorState { Init, Waiting, GoingIn, Elevating, Leaving }
    public enum UserScheduleState { Inside, Outside, Leaving, ComingBack }
    public UserElevatorState elevatorState = UserElevatorState.Init;
    public UserScheduleState scheduleState;
    public Vector2 m_position;
    public int m_destination { get; private set; }
    public int insideDestination { get; private set; }
    public int elevatorIndex = -1;
    public int targetElevatorIndex = -1;

    public float m_horizontalTarget { get; private set; }
    public bool m_walking { get; private set; } = false;
    private float m_walkSpeed;
    private float m_patience = 1.0f;
    private float m_lastPatience = 1.0f;

    private UserSchedule m_schedule;

    public UserSchedule GetSchedule() { return m_schedule; }

    public ElevatorUser(int buildingDestination, float walkSpeed)
    {
        insideDestination = buildingDestination;
        m_walkSpeed = walkSpeed;

        m_schedule = UserSchedule.Generate();
        if (m_schedule.ShouldLeave())
        {
            scheduleState = UserScheduleState.Outside;
            m_destination = 0;
        }
        else
        {
            scheduleState = UserScheduleState.Inside;
            m_destination = insideDestination;
        }

        SetHorizontalTargetOuterSides();
        m_position.Y = m_destination;
        m_position.X = m_horizontalTarget;
    }

    public void UpdateBehavior(double dt, List<Elevator> elevators)
    {
        switch (scheduleState)
        {
            case UserScheduleState.Outside: ManageOutside(); break;
            case UserScheduleState.Inside: ManageInside(); break;
            case UserScheduleState.Leaving: ManageLeaving(elevators); break;
            case UserScheduleState.ComingBack: ManageComingBack(elevators); break;
        }
    }

    public bool NeedsALift()
    {
        switch (scheduleState)
        {
            case UserScheduleState.Leaving:
            case UserScheduleState.ComingBack:
                return true;
            default:
                return false;
        }
    }

    public void UpdateWalk(double dt)
    {
        if (m_horizontalTarget != m_position.X)
        {
            m_walking = !Utils.SpeedMove(dt, m_walkSpeed, m_position.X, m_horizontalTarget, out float newPos);
            m_position.X = newPos;
        }
    }

    private void ManageOutside()
    {
        if (m_schedule.ShouldBack() == false) // equivalent but clearer than ShouldLeave
            return;

        // User is outside and must come back, make him reach elevator floor
        SetHorizontalTargetNearestInside();
        m_destination = insideDestination;
        m_walking = true;
        scheduleState = UserScheduleState.ComingBack;
        elevatorState = UserElevatorState.Init;
    }

    private void ManageInside()
    {
        if (m_schedule.ShouldLeave() == false) // equivalent but clearer than ShouldBack
            return;

        // User is inside and must leave, make him reach elevator floor
        SetHorizontalTargetNearestInside();
        m_destination = 0;
        m_walking = true;
        scheduleState = UserScheduleState.Leaving;
        elevatorState = UserElevatorState.Init;
    }

    private void ManageLeaving(List<Elevator> elevators)
    {

        if (elevatorState != UserElevatorState.Leaving)
        {
            ManageElevatorRide(elevators);
        }
        else
        {
            SetHorizontalTargetOuterSides();
            scheduleState = UserScheduleState.Outside;
        }

        if (m_schedule.ShouldBack())
        {
            m_patience = 0.0f;
        }
        else
        {
            int leaveTime = m_schedule.GetLeaveTimeInMinute();
            int backTime = m_schedule.GetBackTimeInMinute();
            ComputePatience(leaveTime, backTime);

            if (elevatorState == UserElevatorState.Waiting && ShouldReCall())
            {
                ElevatorCallManager.CallElevator(Mathf.RoundToInt(m_position.Y)); // send call again
            }
        }
    }

    private void ManageComingBack(List<Elevator> elevators)
    {
        if (elevatorState != UserElevatorState.Leaving)
        {
            ManageElevatorRide(elevators);
        }
        else
        {
            SetHorizontalTargetOuterSides();
            scheduleState = UserScheduleState.Inside;
        }

        if (m_schedule.ShouldLeave())
        {
            m_patience = 0.0f;
        }
        else
        {
            int leaveTime = m_schedule.GetLeaveTimeInMinute();
            int backTime = m_schedule.GetBackTimeInMinute();
            ComputePatience(backTime, leaveTime);

            if (elevatorState == UserElevatorState.Waiting && ShouldReCall())
            {
                ElevatorCallManager.CallElevator(Mathf.RoundToInt(m_position.Y)); // send call again
            }
        }
    }

    int GetAvailableElevator(List<Elevator> elevators)
    {
        for (int i = 0; i < elevators.Count; ++i)
        {
            if (elevators[i].IsAvailable() && Mathf.RoundToInt(elevators[i].m_position) == Mathf.RoundToInt(m_position.Y))
                return i;
        }
        return -1;
    }

    private void ManageElevatorRide(List<Elevator> elevators)
    {
        switch (elevatorState)
        {
            case UserElevatorState.Init:
                {
                    targetElevatorIndex = GetAvailableElevator(elevators);
                    if (targetElevatorIndex != -1)
                    {
                        elevatorState = UserElevatorState.Waiting; // Shortcut to waiting without calling the elevator
                        break;
                    }

                    if (m_walking)
                        break;

                    elevatorState = UserElevatorState.Waiting;
                    ElevatorCallManager.CallElevator(Mathf.RoundToInt(m_position.Y));
                    break;
                }
            case UserElevatorState.Waiting:
                {
                    targetElevatorIndex = GetAvailableElevator(elevators);
                    if (targetElevatorIndex == -1)
                        break;

                    elevatorState = UserElevatorState.GoingIn;
                    float randomXOffset = 0.06f * (0.5f - GD.Randf());
                    SetWalkTarget(elevators[targetElevatorIndex].GetHorizontalPos() + randomXOffset);
                    break;
                }
            case UserElevatorState.GoingIn:
                {
                    if (elevators[targetElevatorIndex].IsAvailable() == false)
                    {
                        elevatorState = UserElevatorState.Init;
                        SetHorizontalTargetNearestInside();
                        targetElevatorIndex = -1;
                        break;
                    }

                    float distanceToElevator = Mathf.Abs(elevators[targetElevatorIndex].GetHorizontalPos() - m_position.X);
                    if (distanceToElevator < 0.05f) // bit of flexibility
                    {
                        elevatorIndex = targetElevatorIndex;
                        targetElevatorIndex = -1;
                        elevators[elevatorIndex].RequestFloor(m_destination);
                        elevatorState = UserElevatorState.Elevating;
                    }
                    break;
                }
            case UserElevatorState.Elevating:
                {
                    if (elevators[elevatorIndex].AreDoorsBlocking())
                        break;

                    if (elevators[elevatorIndex].IsBroken())
                    {
                        SetHorizontalTargetNearestInside();
                        elevatorState = UserElevatorState.Init;
                        m_position.Y = elevators[elevatorIndex].m_position;
                        elevatorIndex = -1;
                        break;
                    }

                    if (elevators[elevatorIndex].m_position == m_destination)
                    {
                        StatisticsManager.IncrNumberOfTravels();
                        elevators[elevatorIndex].ClearFloorRequest(m_destination);
                        elevatorState = UserElevatorState.Leaving;
                        m_position.Y = elevators[elevatorIndex].m_position;
                        elevatorIndex = -1;
                        break;
                    }
                    break;
                }
        }
    }

    private void ComputePatience(int startMinute, int endMinute)
    {
        if (endMinute < startMinute)
            endMinute += 24 * 60; // Put back next day to make sure we only back after leaving

        int now = GameClockManager.clock.TimeOfDayInMinutes();

        if (now < startMinute)
            now += 24 * 60; // same idea

        m_lastPatience = m_patience;
        m_patience = 1.0f - Mathf.InverseLerp(startMinute, endMinute, now);
    }

    public bool ShouldReCall()
    {
        foreach (float t in UserManager.impatienceThresholds)
        {
            if (m_lastPatience > t && m_patience <= t)
                return true;
        }
        return false;
    }

    public void SetHorizontalTargetNearestInside() { SetHorizontalTargetNearest(true); }
    public void SetHorizontalTargetNearestOutside() { SetHorizontalTargetNearest(false); }
    public float GetPatience() { return m_patience; }

    public void SetHorizontalTargetNearest(bool inside)
    {
        if (m_position.X > 0.5f)
            SetWalkTarget(inside ? 0.9f : 1.1f);
        else
            SetWalkTarget(inside ? 0.1f : -0.1f);
    }

    public void SetWalkTarget(float target) { m_horizontalTarget = target; }
    private void SetHorizontalTargetInnerSides() { SetWalkTarget(GD.Randf() > 0.5f ? 0.1f : 0.9f); }
    private void SetHorizontalTargetOuterSides() { SetWalkTarget(GD.Randf() > 0.5f ? -0.1f : 1.1f); }

    public string GetScheduleDebugText()
    {
        string text = elevatorState + " - " + scheduleState + " " + Mathf.RoundToInt(m_position.Y) + "/" + m_destination;
        text += "\nLeaves: " + (m_schedule.leaveHour < 10 ? "0" : "") + m_schedule.leaveHour + ":" + (m_schedule.leaveMinute < 10 ? "0" : "") + m_schedule.leaveMinute
        + "\nBacks:   " + (m_schedule.backHour < 10 ? "0" : "") + m_schedule.backHour + ":" + (m_schedule.backMinute < 10 ? "0" : "") + m_schedule.backMinute;

        return text;
    }
}
