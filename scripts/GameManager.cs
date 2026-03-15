using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class GameManager : Node
{
    [Export] public float elevatorSpeed = 1.0f;
    [Export] private Node sceneryNode;
    [Export] private PackedScene elevatorDisplayerScene;
    [Export] private UsersDisplayer usersDisplayer;

    private List<Elevator> elevators = [];
    private List<ElevatorUser> users = [];

    private int selectedElevator = 0;

    public override void _Ready()
    {
        for(int i = 0; i < 6; ++i)
        {
            users.Add(new(new(0.1f, i), 6 - 1 - i));
        }

        for(int i = 0; i < 3; ++i)
        {
            ElevatorDisplayer elevatorDisplayer = elevatorDisplayerScene.Instantiate<ElevatorDisplayer>();
            sceneryNode.AddChild(elevatorDisplayer);
            elevators.Add(new(0.0f, elevatorSpeed, elevatorDisplayer));

            elevatorDisplayer.horizontalRatio = (i + 1) / 4.0f;
        }

        GetViewport().SizeChanged += OnScreenResize;
        OnScreenResize();
    }

    public void OnInputUp()
    {
        if(elevators[selectedElevator].m_targetPosition < 5.0f)
            elevators[selectedElevator].m_targetPosition += 1.0f;
    }
    public void OnInputDown()
    {
        if(elevators[selectedElevator].m_targetPosition > 0)
            elevators[selectedElevator].m_targetPosition -= 1.0f;
    }
    public void OnInputLeft()
    {
        if(selectedElevator - 1 >= 0)
            selectedElevator--;
    }
    public void OnInputRight()
    {
        if(selectedElevator + 1 < elevators.Count)
            selectedElevator++;
    }

    public override void _Process(double delta)
    {
        elevators.ForEach((e) => e.Update(delta));
        UpdateUsers();
        usersDisplayer.DisplayUsers(users);
    }

    private void UpdateUsers()
    {
        ManageUserBoardOrLeaveElevators();

        users.ForEach((u) =>
        {
            if(u.elevatorIndex != -1)
                u.m_position.Y = elevators[u.elevatorIndex].m_position;
        });
    }

    private void ManageUserBoardOrLeaveElevators()
    {
        List<int> pos = [];
        List<int> ids = [];
        for(int i = 0; i < elevators.Count; ++i)
        {
            if(elevators[i].moving)
                continue;

            pos.Add(Mathf.RoundToInt(elevators[i].m_position));
            ids.Add(i);
        }

        if(pos.Count == 0)
            return; // all elevator moving, no user movement possible

        for(int i = 0; i < users.Count; ++i)
        {
            ElevatorUser user = users[i];
            if(user.elevatorIndex != -1)
            {
                // User is in elevator
                int userElevatorLocalID = ids.IndexOf(user.elevatorIndex);
                if(userElevatorLocalID == -1)
                    continue; // user's elevator is still moving

                if(pos[userElevatorLocalID] == user.m_destination)
                {
                    GD.Print("User " + i + " left at floor " + user.m_destination);
                    user.m_position.Y = Mathf.RoundToInt(user.m_destination);
                    user.elevatorIndex = -1;
                    user.m_position.X = 0.9f;
                }
            }
            else if(user.m_destination != user.m_position.Y)
            {
                int userElevatorLocalID = pos.IndexOf(Mathf.RoundToInt(user.m_position.Y));
                if(userElevatorLocalID == -1)
                    continue; // No elevator on user's floor

                user.elevatorIndex = ids[userElevatorLocalID];
                float randomXOffset = 0.1f * (0.5f - GD.Randf());
                user.m_position.X = elevators[user.elevatorIndex].GetHorizontalPos() + randomXOffset;
                GD.Print("User " + i + " jumped in at floor " + user.m_position);
            }
        }
    }

    private void OnScreenResize()
    {
        DisplayUtils.screenSize = GetViewport().GetVisibleRect().Size;
        elevators.ForEach((e) => e.forceDisplayUpdate = true);
    }
}