using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ElevatorDisplayer : Node2D
{
    [Export] private Sprite2D elevatorModel;
    [Export] private Sprite2D targetModel;

    public static Vector2 screenSize = Vector2.Zero;

    public override void _Ready()
    {
        Position = Vector2.Zero; // make sure nothing is offset
    }

    public void DrawElevator(float pos, float targetPos)
    {
        elevatorModel.Position = compute2DFromHeight(pos);
        targetModel.Position = compute2DFromHeight(targetPos);
    }

    private Vector2 compute2DFromHeight(float pos)
    {
        float max = 6.0f;
        float step = screenSize.Y / (max + 1.0f);
        return new(screenSize.X * 0.5f, (max - pos) * step);
    }

}
