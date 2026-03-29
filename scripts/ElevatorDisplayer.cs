using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ElevatorDisplayer : Node2D
{
    [Export] private AnimatedSprite2D elevator;
    [Export] private Sprite2D targetModel;

    public float horizontalRatio = 0.5f;

    public override void _Ready()
    {
        Position = Vector2.Zero; // make sure nothing is offset
    }

    public void UpdateDisplayPos(float pos, float targetPos)
    {
        elevator.Position = DisplayUtils.ComputeScreenPosFromPos(new(horizontalRatio, pos));
        targetModel.Position = DisplayUtils.ComputeScreenPosFromPos(new(horizontalRatio, targetPos));
    }

    public void UpdateDoorDisplay(float status)
    {
        if(status < 0.05f)
            elevator.Frame = 0;
        else if(status < 0.5f)
            elevator.Frame = 1;
        else if(status < 0.95f)
            elevator.Frame = 2;
        else
            elevator.Frame = 3;
    }

}
