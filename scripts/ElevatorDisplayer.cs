using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ElevatorDisplayer : Node2D
{
    [Export] private Sprite2D elevatorModel;
    [Export] private Sprite2D targetModel;

    public float horizontalRatio = 0.5f;

    public override void _Ready()
    {
        Position = Vector2.Zero; // make sure nothing is offset
    }

    public void UpdateDisplay(float pos, float targetPos)
    {
        elevatorModel.Position = DisplayUtils.ComputeScreenPosFromPos(new(horizontalRatio, pos));
        targetModel.Position = DisplayUtils.ComputeScreenPosFromPos(new(horizontalRatio, targetPos));
    }

}
