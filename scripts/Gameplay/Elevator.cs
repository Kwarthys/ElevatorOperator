using Godot;
using System;

public class Elevator
{
    public float m_position { get; private set; }
    public float m_speed { get; private set; }
    public float m_doorSpeed { get; private set; }
    public float m_targetPosition;
    public bool m_moving { get; private set; } = false;
    public float m_doorPos { get; private set; } = 0.0f;
    private double m_broken = 0.0;

    private ElevatorDisplayer m_displayer;
    private CruiseSoundManager m_soundManager;

    public bool forceDisplayUpdate = false;

    private int m_requestedFloorFlags = 0;

    public float GetHorizontalPos() { return m_displayer.horizontalRatio; }

    public Elevator(float position, float speed, float doorSpeed, ElevatorDisplayer displayer)
    {
        m_position = position;
        m_speed = speed;
        m_targetPosition = position;
        m_displayer = displayer;
        m_doorSpeed = doorSpeed;

        m_displayer.Ready += () => m_displayer.UpdateDisplayPos(m_position, m_targetPosition);
        m_soundManager = m_displayer.cruiseSoundManager;
    }

    public void Update(double dt)
    {
        ManageDoors(dt);

        m_displayer.UpdateSigns(dt);

        if(forceDisplayUpdate)
            m_displayer.UpdateScale();

        if(m_position == m_targetPosition)
        {
            if(forceDisplayUpdate)
            {
                m_displayer.UpdateDisplayPos(m_position, m_targetPosition);
                forceDisplayUpdate = false;
            }

            m_soundManager.UpdateState(m_moving);
            return;
        }

        if(CanMove())
        {
            m_moving = !Utils.SpeedMove(dt, m_speed, m_position, m_targetPosition, out float newPos);
            m_position = newPos;
        }

        m_soundManager.UpdateState(m_moving);

        m_displayer.UpdateDisplayPos(m_position, m_targetPosition);
        forceDisplayUpdate = false;
    }

    private void ManageDoors(double dt)
    {
        if(m_targetPosition == m_position || IsBroken())
        {
            // We're where we want, open doors
            if(m_doorPos < 1.0f)
            {
                Utils.SpeedMove(dt, m_doorSpeed, m_doorPos, 1.0f, out float newPos);
                m_doorPos = newPos;
                m_displayer.UpdateDoorDisplay(m_doorPos);
            }
        }
        else
        {
            // We should close the doors as we want to move
            if(m_doorPos > 0.0f)
            {
                Utils.SpeedMove(dt, m_doorSpeed, m_doorPos, 0.0f, out float newPos);
                m_doorPos = newPos;
                m_displayer.UpdateDoorDisplay(m_doorPos);
            }
        }
    }

    private bool CanMove() { return m_doorPos <= 0.0f && IsBroken() == false; }
    public bool AreDoorsBlocking() { return m_doorPos < 0.7f; }
    public bool IsAvailable() { return AreDoorsBlocking() == false && IsBroken() == false; }

    public bool IsBroken() { return m_broken > 0.0f; }
    public void Break()
    {
        m_broken = 1.0;
        ClearFloorRequests();

        m_position = Mathf.RoundToInt(m_position);

        m_moving = false;
    }
    public void Repair(double repairAmount)
    {
        if(IsBroken() == false)
            return;

        m_broken -= repairAmount;

        if(IsBroken() == false)
        {
            m_broken = 0.0;
            ClearFloorRequests();
        }
        else
            m_displayer.UpdateFloorSelectionDisplay(0, m_broken);
    }

    public void RequestFloor(int floor)
    {
        m_requestedFloorFlags |= 1 << floor;
        m_displayer.UpdateFloorSelectionDisplay(m_requestedFloorFlags, m_broken);

        m_displayer.AnimateFloorSelection(floor);
    }
    public void ClearFloorRequest(int floor)
    {
        m_requestedFloorFlags &= ~(1 << floor);
        m_displayer.UpdateFloorSelectionDisplay(m_requestedFloorFlags, m_broken);
    }

    public void ClearFloorRequests()
    {
        m_requestedFloorFlags = 0;
        m_displayer.UpdateFloorSelectionDisplay(0, m_broken);
    }
}
