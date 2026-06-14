using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class ElevatorDisplayer : Node2D
{
    [Export] private AnimatedSprite2D elevator;
    [Export] private Sprite2D targetModel;
    [Export] private RichTextLabel floorSelectionLabel;
    [Export] private Color restSelectionColor;
    [Export] private Color activeSelectionColor;

    public float horizontalRatio = 0.5f;

    public override void _Ready()
    {
        Position = Vector2.Zero; // make sure nothing is offset
        SetFloorSelection(0);
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

    public void SetFloorSelection(int selectionFlags)
    {
        floorSelectionLabel.Clear();

        for(int i = 0; i < DisplayUtils.maxFloors; ++i)
        {
            string text = i.ToString();
            if(i != 0)
                text = " " + text;

            bool selected = (selectionFlags & (1 << i)) != 0;

            floorSelectionLabel.PushColor(selected ? activeSelectionColor : restSelectionColor);
            floorSelectionLabel.AddText(text);
            floorSelectionLabel.Pop();
        }
    }

}
