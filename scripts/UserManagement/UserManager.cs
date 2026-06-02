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
            u.Update(dt);

            if(u.elevatorState == ElevatorUser.UserElevatorState.Outside && u.m_walking == false)
                u.elevatorState = ElevatorUser.UserElevatorState.Waiting;

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
            users.Add(new(6 - 1 - i, usersWalkSpeed * Mathf.Lerp(0.7f, 1.0f, GD.Randf())));
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
            if(user.elevatorState == ElevatorUser.UserElevatorState.Leaving)
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
                    user.elevatorState = ElevatorUser.UserElevatorState.Leaving;
                }
            }
            else if(user.m_destination != user.m_position.Y)
            {
                int userElevatorLocalID = pos.IndexOf(Mathf.RoundToInt(user.m_position.Y));
                if(userElevatorLocalID == -1)
                {
                    if(user.elevatorState == ElevatorUser.UserElevatorState.GoingIn)
                    {
                        // Elevator just left right in front of this user's face --> todo get angry
                        user.elevatorState = ElevatorUser.UserElevatorState.Waiting;
                        user.SetHorizontalTargetNearest();
                    }
                    continue; // No elevator on user's floor
                }

                int elevatorIndex = ids[userElevatorLocalID];

                if(user.elevatorState == ElevatorUser.UserElevatorState.Waiting)
                {
                    user.elevatorState = ElevatorUser.UserElevatorState.GoingIn;
                    float randomXOffset = 0.06f * (0.5f - GD.Randf());
                    user.SetWalkTarget(elevators[elevatorIndex].GetHorizontalPos() + randomXOffset);
                }
                else if(user.elevatorState == ElevatorUser.UserElevatorState.GoingIn)
                {
                    float distanceToElevator = Mathf.Abs(elevators[elevatorIndex].GetHorizontalPos() - user.m_position.X);
                    if(distanceToElevator < 0.05f) // bit a flexibility
                    {
                        // Caught the elevator !
                        GD.Print("User " + i + " jumped in at floor " + user.m_position);
                        elevators[elevatorIndex].RequestFloor(user.m_destination);
                        user.elevatorIndex = elevatorIndex;
                        user.elevatorState = ElevatorUser.UserElevatorState.Elevating;
                    }
                }
            }
        }
    }
}
