using Godot;
using System;

public class Elevator
{
    public float m_position { get; private set; }
    public float m_speed { get; private set; }
    public float m_targetPosition;

    public bool moving { get; private set; } = false;

    public Elevator(float position, float speed)
    {
        m_position = position;
        m_speed = speed;
        m_targetPosition = position;
    }

    public void Update(double dt)
    {
        if(m_position == m_targetPosition)
            return;

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
    }
}
