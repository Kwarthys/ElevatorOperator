using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UserManager : Node
{
    [Export] private UsersDisplayer usersDisplayer;
    [Export] public float usersWalkSpeed = 0.5f;
    private List<ElevatorUser> users = [];

    public void UpdateUsers(double dt, List<Elevator> elevators)
    {
        usersDisplayer.DisplayUsers(users, dt);
        ManageUserBoardOrLeaveElevators(elevators);

        users.ForEach((u) =>
        {
            u.UpdateWalk(dt);

            if(u.state == ElevatorUser.UserState.Spawning && u.m_walking == false)
                u.state = ElevatorUser.UserState.Waiting;

            if(u.elevatorIndex != -1)
                u.m_position.Y = elevators[u.elevatorIndex].m_position;
        });
    }

    public void OnScreenResize()
    {
        usersDisplayer.OnScreenResize();
    }

    public void InitUsers()
    {
        for(int i = 0; i < 6; ++i)
        {
            users.Add(new(new(-0.1f, i), 6 - 1 - i, usersWalkSpeed));
            users.Last().SetWalkTarget(0.1f);
        }
    }

    private void ManageUserBoardOrLeaveElevators(List<Elevator> elevators)
    {
        List<int> pos = [];
        List<int> ids = [];
        for(int i = 0; i < elevators.Count; ++i)
        {
            if(elevators[i].moving || elevators[i].AreDoorsBlocking() == false)
                continue;

            pos.Add(Mathf.RoundToInt(elevators[i].m_position));
            ids.Add(i);
        }

        if(pos.Count == 0)
            return; // all elevator moving, no user movement possible

        for(int i = 0; i < users.Count; ++i)
        {
            ElevatorUser user = users[i];
            if(user.state == ElevatorUser.UserState.Leaving)
                continue; // User is not interacting with elevators

            if(user.elevatorIndex != -1)
            {
                // User is in elevator
                int userElevatorLocalID = ids.IndexOf(user.elevatorIndex);
                if(userElevatorLocalID == -1)
                    continue; // user's elevator is still moving

                if(pos[userElevatorLocalID] == user.m_destination)
                {
                    elevators[user.elevatorIndex].ClearFloorRequest(user.m_destination);

                    GD.Print("User " + i + " left at floor " + user.m_destination);
                    user.m_position.Y = Mathf.RoundToInt(user.m_destination);
                    user.elevatorIndex = -1;
                    user.state = ElevatorUser.UserState.Leaving;
                    user.SetWalkTarget(GD.Randf() > 0.5f ? -1.5f : 1.5f);
                }
            }
            else if(user.m_destination != user.m_position.Y)
            {
                int userElevatorLocalID = pos.IndexOf(Mathf.RoundToInt(user.m_position.Y));
                if(userElevatorLocalID == -1)
                {
                    if(user.state == ElevatorUser.UserState.GoingIn)
                    {
                        // Elevator just left right in front of this user's face --> todo get angry
                        user.state = ElevatorUser.UserState.Waiting;
                        user.SetWalkTarget(0.1f);
                    }
                    continue; // No elevator on user's floor
                }

                int elevatorIndex = ids[userElevatorLocalID];

                if(user.state == ElevatorUser.UserState.Waiting)
                {
                    user.state = ElevatorUser.UserState.GoingIn;
                    float randomXOffset = 0.06f * (0.5f - GD.Randf());
                    user.SetWalkTarget(elevators[elevatorIndex].GetHorizontalPos() + randomXOffset);
                }
                else if(user.state == ElevatorUser.UserState.GoingIn)
                {
                    float distanceToElevator = Mathf.Abs(elevators[elevatorIndex].GetHorizontalPos() - user.m_position.X);
                    if(distanceToElevator < 0.05f) // bit a flexibility
                    {
                        // Caught the elevator !
                        GD.Print("User " + i + " jumped in at floor " + user.m_position);
                        elevators[elevatorIndex].RequestFloor(user.m_destination);
                        user.elevatorIndex = elevatorIndex;
                        user.state = ElevatorUser.UserState.Elevating;
                    }
                }
            }
        }
    }
}
