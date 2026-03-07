using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class GameManager : Node
{
    [Export] public float elevatorSpeed = 1.0f;

    [Export] private ElevatorDisplayer elevatorDisplayer;

    private Elevator elevator;
    private List<ElevatorUser> users = [];

    public override void _Ready()
    {
        for(int i = 0; i < 5; ++i)
        {
            users.Add(new(i, 5 - i));
        }

        elevator = new(0.0f, elevatorSpeed);

        GetViewport().SizeChanged += OnScreenResize;
        OnScreenResize();
    }

    public void OnInputUp()
    {
        if(elevator.m_targetPosition < 5.0f)
            MoveElevator(1.0f);
    }
    public void OnInputDown()
    {
        if(elevator.m_targetPosition > 0)
            MoveElevator(-1.0f);
    }
    public void OnInputLeft()
    {
        GD.Print("LEFT");
    }
    public void OnInputRight()
    {
        GD.Print("RIGHT");
    }

    public override void _Process(double delta)
    {
        elevator.Update(delta);
        UpdateUsers();
        elevatorDisplayer.DrawElevator(elevator.m_position, elevator.m_targetPosition);
    }


    private void MoveElevator(float offset)
    {
        elevator.m_targetPosition = Mathf.Round(elevator.m_targetPosition + offset);
    }

    private void UpdateUsers()
    {
        if(elevator.moving)
            return;

        for(int i = 0; i < users.Count; ++i)
        {
            ElevatorUser user = users[i];
            if(user.elevatorIndex != -1)
            {
                // User is in elevator
                if(elevator.m_position == user.m_destination)
                {
                    GD.Print("User " + i + " left at floor " + elevator.m_position);
                    user.m_position = Mathf.RoundToInt(elevator.m_position);
                    user.elevatorIndex = -1;
                }
            }
            else if(user.m_destination != user.m_position)
            {
                if(elevator.m_position == user.m_position)
                {
                    user.elevatorIndex = 0;
                    GD.Print("User " + i + " jumped in at floor " + elevator.m_position);
                }
            }
        }
    }

    private void OnScreenResize()
    {
        ElevatorDisplayer.screenSize = GetViewport().GetVisibleRect().Size;
    }
}