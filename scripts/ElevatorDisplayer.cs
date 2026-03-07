using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ElevatorDisplayer : Node2D
{
    public static Vector2 screenSize = Vector2.Zero;
    private float m_pos = 0.0f;

    public void Move(float pos)
    {
        float max = 6.0f;
        float step = screenSize.Y / (max + 1.0f);
        Position = new(screenSize.X * 0.5f, (max - pos) * step);
        m_pos = pos;
    }

    public void Redraw()
    {
        Move(m_pos);
    }

}
