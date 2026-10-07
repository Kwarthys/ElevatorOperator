using Godot;
using System;
using System.Collections.Generic;

public abstract class ElevatorUser
{
    public enum UserElevatorState { Init, Waiting, GoingIn, Elevating, Leaving }
    public enum UserMovementState { Inside, Outside, Leaving, Entering }
    public UserElevatorState m_elevatorState = UserElevatorState.Init;
    public UserMovementState m_movementState;
    public Vector2 m_position;
    public int m_destination { get; protected set; }
    public int elevatorIndex = -1;
    public int targetElevatorIndex = -1;

    public float m_horizontalTarget { get; private set; }
    public bool m_walking { get; private set; } = false;
    private float m_walkSpeed;

    public ElevatorUser(float walkSpeed)
    {
        m_walkSpeed = walkSpeed;
    }

    protected abstract void ManageOutside();
    protected abstract void ManageInside();
    protected abstract void ManageLeaving(List<Elevator> elevators);
    protected abstract void ManageEntering(List<Elevator> elevators);

    public virtual string GetDebugText() { return ""; }
    public virtual float GetPatience() { return 0.3f; }

    public void UpdateBehavior(double dt, List<Elevator> elevators)
    {
        switch(m_movementState)
        {
            case UserMovementState.Outside: ManageOutside(); break;
            case UserMovementState.Inside: ManageInside(); break;
            case UserMovementState.Leaving: ManageLeaving(elevators); break;
            case UserMovementState.Entering: ManageEntering(elevators); break;
        }
    }

    public bool NeedsALift()
    {
        switch(m_movementState)
        {
            case UserMovementState.Leaving:
            case UserMovementState.Entering:
                return true;
            default:
                return false;
        }
    }

    public void UpdateWalk(double dt)
    {
        if(m_horizontalTarget != m_position.X)
        {
            m_walking = !Utils.SpeedMove(dt, m_walkSpeed, m_position.X, m_horizontalTarget, out float newPos);
            m_position.X = newPos;
        }
    }

    int GetAvailableElevator(List<Elevator> elevators)
    {
        for(int i = 0; i < elevators.Count; ++i)
        {
            if(elevators[i].IsAvailable() && Mathf.RoundToInt(elevators[i].m_position) == Mathf.RoundToInt(m_position.Y))
                return i;
        }
        return -1;
    }

    protected void ManageElevatorRide(List<Elevator> elevators)
    {
        switch(m_elevatorState)
        {
            case UserElevatorState.Init:
            {
                targetElevatorIndex = GetAvailableElevator(elevators);
                if(targetElevatorIndex != -1)
                {
                    m_elevatorState = UserElevatorState.Waiting; // Shortcut to waiting without calling the elevator
                    break;
                }

                if(m_walking)
                    break;

                m_elevatorState = UserElevatorState.Waiting;
                ElevatorCallManager.CallElevator(Mathf.RoundToInt(m_position.Y));
                break;
            }
            case UserElevatorState.Waiting:
            {
                targetElevatorIndex = GetAvailableElevator(elevators);
                if(targetElevatorIndex == -1)
                    break;

                m_elevatorState = UserElevatorState.GoingIn;
                float randomXOffset = 0.06f * (0.5f - GD.Randf());
                SetWalkTarget(elevators[targetElevatorIndex].GetHorizontalPos() + randomXOffset);
                break;
            }
            case UserElevatorState.GoingIn:
            {
                if(elevators[targetElevatorIndex].IsAvailable() == false)
                {
                    m_elevatorState = UserElevatorState.Init;
                    SetHorizontalTargetNearestInside();
                    targetElevatorIndex = -1;
                    break;
                }

                float distanceToElevator = Mathf.Abs(elevators[targetElevatorIndex].GetHorizontalPos() - m_position.X);
                if(distanceToElevator < 0.05f) // bit of flexibility
                {
                    elevatorIndex = targetElevatorIndex;
                    targetElevatorIndex = -1;
                    elevators[elevatorIndex].RequestFloor(m_destination);
                    m_elevatorState = UserElevatorState.Elevating;
                }
                break;
            }
            case UserElevatorState.Elevating:
            {
                if(elevators[elevatorIndex].AreDoorsBlocking())
                    break;

                if(elevators[elevatorIndex].IsBroken())
                {
                    SetHorizontalTargetNearestInside();
                    m_elevatorState = UserElevatorState.Init;
                    m_position.Y = elevators[elevatorIndex].m_position;
                    elevatorIndex = -1;
                    break;
                }

                if(elevators[elevatorIndex].m_position == m_destination)
                {
                    StatisticsManager.IncrNumberOfTravels();
                    elevators[elevatorIndex].ClearFloorRequest(m_destination);
                    m_elevatorState = UserElevatorState.Leaving;
                    m_position.Y = elevators[elevatorIndex].m_position;
                    elevatorIndex = -1;
                    break;
                }
                break;
            }
        }
    }

    public void SetHorizontalTargetNearestInside() { SetHorizontalTargetNearest(true); }
    public void SetHorizontalTargetNearestOutside() { SetHorizontalTargetNearest(false); }

    public void SetHorizontalTargetNearest(bool inside)
    {
        if(m_position.X > 0.5f)
            SetWalkTarget(inside ? 0.9f : 1.1f);
        else
            SetWalkTarget(inside ? 0.1f : -0.1f);
    }

    public void SetWalkTarget(float target) { m_horizontalTarget = target; m_walking = true; }
    protected void SetHorizontalTargetInnerSides() { SetWalkTarget(GD.Randf() > 0.5f ? 0.1f : 0.9f); }
    protected void SetHorizontalTargetOuterSides() { SetWalkTarget(GD.Randf() > 0.5f ? -0.1f : 1.1f); }
}
