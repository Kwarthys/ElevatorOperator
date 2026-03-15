using Godot;
using System;

public class Elevator
{
    public float m_position { get; private set; }
    public float m_speed { get; private set; }
    public float m_targetPosition;
    public bool moving { get; private set; } = false;

    private ElevatorDisplayer m_displayer;

    public bool forceDisplayUpdate = false;

    public float GetHorizontalPos() { return m_displayer.horizontalRatio; }

    public Elevator(float position, float speed, ElevatorDisplayer displayer)
    {
        m_position = position;
        m_speed = speed;
        m_targetPosition = position;
        m_displayer = displayer;

        m_displayer.Ready += () => m_displayer.UpdateDisplay(m_position, m_targetPosition);
    }

    public void Update(double dt)
    {
        if(m_position == m_targetPosition && forceDisplayUpdate == false)
        {
            if(forceDisplayUpdate)
            {
                m_displayer.UpdateDisplay(m_position, m_targetPosition);
                forceDisplayUpdate = false;
            }
            return;
        }

        float delta = m_speed * (float)dt;

        if(m_targetPosition < m_position)
            delta *= -1.0f;

        if(delta > Mathf.Abs(m_targetPosition - m_position))
        {
            // Overshoot, snap and be done
            moving = false;
            m_position = m_targetPosition;
        }
        else
        {
            moving = true;
            m_position += delta;
        }

        m_displayer.UpdateDisplay(m_position, m_targetPosition);
        forceDisplayUpdate = false;
    }
}
