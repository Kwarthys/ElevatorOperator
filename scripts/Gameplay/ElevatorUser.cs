using Godot;
using System;

public class ElevatorUser
{
    public Vector2 m_position;
    public int m_destination { get; private set; }
    public int elevatorIndex = -1;

    public ElevatorUser(Vector2 position, int destination)
    {
        m_position = position;
        m_destination = destination;
    }

}
