using Godot;
using System;

public class ElevatorUser
{
    public enum UserElevatorState { Outside, Waiting, GoingIn, Elevating, Leaving }
    public enum UserScheduleState { Inside, Outside, Leaving, ComingBack }
    public UserElevatorState elevatorState = UserElevatorState.Outside;
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

    private UserSchedule m_schedule;

    public ElevatorUser(int buildingDestination, float walkSpeed)
    {
        insideDestination = buildingDestination;
        m_walkSpeed = walkSpeed;


        m_schedule = UserSchedule.Generate();
        if(m_schedule.ShouldLeave())
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

    public void Update(double dt)
    {
        switch(scheduleState)
        {
            case UserScheduleState.Outside: ManageOutside(); break;
            case UserScheduleState.Inside: ManageInside(); break;
            case UserScheduleState.Leaving: ManageLeaving(); break;
            case UserScheduleState.ComingBack: ManageComingBack(); break;
        }
        UpdateWalk(dt);
    }

    private void UpdateWalk(double dt)
    {
        if(m_horizontalTarget != m_position.X)
        {
            m_walking = !Utils.SpeedMove(dt, m_walkSpeed, m_position.X, m_horizontalTarget, out float newPos);
            m_position.X = newPos;
        }
    }

    private void ManageOutside()
    {
        if(m_schedule.ShouldBack() == false) // equivalent but clearer as ShouldLeave
            return;

        // User is outside and must come back, make him reach elevator floor
        SetHorizontalTargetNearestInside();
        m_destination = insideDestination;
        m_walking = true;
        scheduleState = UserScheduleState.ComingBack;
    }

    private void ManageInside()
    {
        if(m_schedule.ShouldLeave() == false) // equivalent but clearer as ShouldBack
            return;

        // User is inside and must leave, make him reach elevator floor
        SetHorizontalTargetNearestInside();
        m_destination = 0;
        m_walking = true;
        scheduleState = UserScheduleState.Leaving;
    }

    private void ManageLeaving()
    {
        if(elevatorState == UserElevatorState.Outside && m_walking == false)
        {
            elevatorState = UserElevatorState.Waiting; // leave control to manager and elevators
        }
        else if(elevatorState == UserElevatorState.Leaving)
        {
            SetHorizontalTargetOuterSides();
            scheduleState = UserScheduleState.Outside;
            elevatorState = UserElevatorState.Outside;
        }

        if(m_schedule.ShouldBack())
            GD.Print("DONE - FINITO - STOP " + GetScheduleDebugText());
    }

    private void ManageComingBack()
    {
        if(elevatorState == UserElevatorState.Outside && m_walking == false)
        {
            elevatorState = UserElevatorState.Waiting; // leave control to manager and elevators
        }
        else if(elevatorState == UserElevatorState.Leaving)
        {
            SetHorizontalTargetOuterSides();
            scheduleState = UserScheduleState.Inside;
            elevatorState = UserElevatorState.Outside;
        }

        if(m_schedule.ShouldLeave())
            GD.Print("DONE - FINITO - STOP " + GetScheduleDebugText());
    }

    public void SetHorizontalTargetNearestInside() { SetHorizontalTargetNearest(true); }
    public void SetHorizontalTargetNearestOutside() { SetHorizontalTargetNearest(false); }

    public void SetHorizontalTargetNearest(bool inside)
    {
        if(m_position.X > 0.5f)
            m_horizontalTarget = inside ? 0.9f : 1.1f;
        else
            m_horizontalTarget = inside ? 0.1f : -0.1f;
    }

    public void SetWalkTarget(float target) { m_horizontalTarget = target; }
    private void SetHorizontalTargetInnerSides() { m_horizontalTarget = GD.Randf() > 0.5f ? 0.1f : 0.9f; }
    private void SetHorizontalTargetOuterSides() { m_horizontalTarget = GD.Randf() > 0.5f ? -0.1f : 1.1f; }

    public string GetScheduleDebugText()
    {
        string text = scheduleState + " " + Mathf.RoundToInt(m_position.Y) + "/" + m_destination;
        text += "\nLeaves: " + (m_schedule.leaveHour < 10 ? "0" : "") + m_schedule.leaveHour + ":" + (m_schedule.leaveMinute < 10 ? "0" : "") + m_schedule.leaveMinute
        + "\nBacks:   " + (m_schedule.backHour < 10 ? "0" : "") + m_schedule.backHour + ":" + (m_schedule.backMinute < 10 ? "0" : "") + m_schedule.backMinute;

        return text;
    }
}
