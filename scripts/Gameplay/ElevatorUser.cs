using Godot;
using System;

public class ElevatorUser
{
    public enum UserState { Spawning, Waiting, GoingIn, Elevating, Leaving }
    public UserState state = UserState.Spawning;
    public Vector2 m_position;
    public int m_destination { get; private set; }
    public int elevatorIndex = -1;

    public float m_horizontalTarget { get; private set; }
    public bool m_walking { get; private set; } = false;
    private float m_walkSpeed;

    public ElevatorUser(Vector2 position, int destination, float walkSpeed)
    {
        m_position = position;
        m_destination = destination;
        m_walkSpeed = walkSpeed;

        m_horizontalTarget = position.X;
    }

    public void UpdateWalk(double dt)
    {
        if(m_horizontalTarget != m_position.X)
        {
            m_walking = !Utils.SpeedMove(dt, m_walkSpeed, m_position.X, m_horizontalTarget, out float newPos);
            m_position.X = newPos;
        }
    }

    public void SetWalkTarget(float target) { m_horizontalTarget = target; }

}
