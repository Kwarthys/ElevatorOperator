using Godot;
using System;

public class Elevator
{
    public float m_position { get; private set; }
    public float m_speed { get; private set; }
    public float m_doorSpeed { get; private set; }
    public float m_targetPosition;
    public bool moving { get; private set; } = false;
    public float m_doorPos { get; private set; } = 1.0f;

    private ElevatorDisplayer m_displayer;

    public bool forceDisplayUpdate = false;

    public float GetHorizontalPos() { return m_displayer.horizontalRatio; }

    public Elevator(float position, float speed, float doorSpeed, ElevatorDisplayer displayer)
    {
        m_position = position;
        m_speed = speed;
        m_targetPosition = position;
        m_displayer = displayer;
        m_doorSpeed = doorSpeed;

        m_displayer.Ready += () => m_displayer.UpdateDisplayPos(m_position, m_targetPosition);
    }

    public void Update(double dt)
    {
        ManageDoors(dt);

        if(m_position == m_targetPosition)
        {
            if(forceDisplayUpdate)
            {
                m_displayer.UpdateDisplayPos(m_position, m_targetPosition);
                forceDisplayUpdate = false;
            }
            return;
        }

        if(CanMove())
        {
            moving = !SpeedMove(dt, m_speed, m_position, m_targetPosition, out float newPos);
            m_position = newPos;
        }

        m_displayer.UpdateDisplayPos(m_position, m_targetPosition);
        forceDisplayUpdate = false;
    }

    private void ManageDoors(double dt)
    {

        if(m_targetPosition == m_position)
        {
            // We're where we want, open doors
            if(m_doorPos < 1.0f)
            {
                SpeedMove(dt, m_doorSpeed, m_doorPos, 1.0f, out float newPos);
                m_doorPos = newPos;
                m_displayer.UpdateDoorDisplay(m_doorPos);
            }
        }
        else
        {
            // We should close the doors as we want to move
            if(m_doorPos > 0.0f)
            {
                SpeedMove(dt, m_doorSpeed, m_doorPos, 0.0f, out float newPos);
                m_doorPos = newPos;
                m_displayer.UpdateDoorDisplay(m_doorPos);
            }
        }
    }

    private bool CanMove() { return m_doorPos <= 0.0f; }
    public bool AreDoorsBlocking() { return m_doorPos > 0.7f; }

    private bool SpeedMove(double dt, float speed, float pos, float target, out float newPos)
    {
        float delta = speed * (float)dt;

        if(target < pos)
            delta *= -1.0f;

        if(Mathf.Abs(delta) > Mathf.Abs(target - pos))
        {
            // Overshoot, snap and be done
            newPos = target;
            return true;
        }
        else
        {
            newPos = pos + delta;
            return false; // move not done
        }
    }
}
