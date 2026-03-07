using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class GameManager : Node
{
    [Export] private ElevatorDisplayer elevatorDisplayer;

    private int elevatorPos = 0;
    private List<ElevatorUser> users = [];

    public override void _Ready()
    {
        for(int i = 0; i < 5; ++i)
        {
            users.Add(new(i, 5 - i));
        }

        UpdateUsers();

        GetViewport().SizeChanged += OnScreenResize;
        OnScreenResize();
    }

    public void OnInputUp()
    {
        if(elevatorPos < 5)
            MoveElevator(1);
    }
    public void OnInputDown()
    {
        if(elevatorPos > 0)
            MoveElevator(-1);
    }
    public void OnInputLeft()
    {
        GD.Print("LEFT");
    }
    public void OnInputRight()
    {
        GD.Print("RIGHT");
    }

    private void MoveElevator(int offset)
    {
        elevatorPos += offset;
        UpdateUsers();

        elevatorDisplayer.Move(elevatorPos);
    }

    private void UpdateUsers()
    {
        for(int i = 0; i < users.Count; ++i)
        {
            ElevatorUser user = users[i];
            if(user.elevatorIndex != -1)
            {
                // User is in elevator
                if(elevatorPos == user.m_destination)
                {
                    GD.Print("User " + i + " left at floor " + elevatorPos);
                    user.m_position = elevatorPos;
                    user.elevatorIndex = -1;
                }
            }
            else if(user.m_destination != user.m_position)
            {
                if(elevatorPos == user.m_position)
                {
                    user.elevatorIndex = 0;
                    GD.Print("User " + i + " jumped in at floor " + elevatorPos);
                }
            }
        }
    }

    private void OnScreenResize()
    {
        ElevatorDisplayer.screenSize = GetViewport().GetVisibleRect().Size;
        elevatorDisplayer.Redraw();
    }


}