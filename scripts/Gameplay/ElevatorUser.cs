using Godot;
using System;

public class ElevatorUser
{
    public int m_position;
    public int m_destination { get; private set; }
    public int elevatorIndex = -1;

    public ElevatorUser(int position, int destination)
    {
        m_position = position;
        m_destination = destination;
    }

}
