using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UserManager : Node
{
    [Export] private UsersDisplayer usersDisplayer;
    [Export] public float usersWalkSpeed = 0.5f;
    [Export] private int startingUserCount = 5;
    [Export] private float addUserPeriod = 10.0f;
    [Export] private Control gameOverScreen;
    private List<ElevatorUser> users = [];

    private double addUserDTCounter = 0.0f;

    public bool gameLost { get; private set; } = false;

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

            if(gameLost == false && u.GetPatience() == 0.0f)
            {
                gameLost = true;
                gameOverScreen.Visible = true;
            }
        });

        if(gameLost)
            return; // stop adding users when game is already lost

        addUserDTCounter += dt;
        while(addUserDTCounter > addUserPeriod)
        {
            ElevatorUser user = GenerateUser();
            addUserDTCounter -= addUserPeriod;
        }
    }

    public void OnScreenResize()
    {
        usersDisplayer.OnScreenResize();
    }

    public void InitUsers()
    {
        for(int i = 0; i < startingUserCount; ++i)
        {
            GenerateUser();
        }
    }

    public int GetUserCount() { return users.Count; }

    private ElevatorUser GenerateUser()
    {
        users.Add(new(GD.RandRange(1, 5), usersWalkSpeed * Mathf.Lerp(0.7f, 1.0f, GD.Randf())));
        return users.Last();
    }

    private void ManageUserBoardOrLeaveElevators(List<Elevator> elevators)
    {
        List<int> pos = [];
        List<int> ids = [];
        for(int i = 0; i < elevators.Count; ++i)
        {
            if(elevators[i].moving || elevators[i].AreDoorsBlocking())
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

                    //GD.Print("User " + i + " left at floor " + user.m_destination);
                    user.m_position.Y = Mathf.RoundToInt(user.m_destination);
                    user.elevatorIndex = -1;
                    user.elevatorState = ElevatorUser.UserElevatorState.Leaving;
                }
            }
            else if(user.m_destination != user.m_position.Y)
            {
                if(user.targetElevatorIndex != -1)
                {
                    // user is already running toward an elevator, is it still available ?
                    if(elevators[user.targetElevatorIndex].AreDoorsBlocking())
                    {
                        // Elevator just left right in front of this user's face --> todo get angry
                        user.elevatorState = ElevatorUser.UserElevatorState.Waiting;
                        user.SetHorizontalTargetNearestInside();
                        user.targetElevatorIndex = -1;
                    }
                    else
                    {
                        int elevatorIndex = user.targetElevatorIndex;
                        float distanceToElevator = Mathf.Abs(elevators[elevatorIndex].GetHorizontalPos() - user.m_position.X);
                        if(distanceToElevator < 0.05f) // bit of flexibility
                        {
                            // Caught the elevator !
                            //GD.Print("User " + i + " jumped in at floor " + user.m_position);
                            elevators[elevatorIndex].RequestFloor(user.m_destination);
                            user.elevatorIndex = elevatorIndex;
                            user.targetElevatorIndex = -1;
                            user.elevatorState = ElevatorUser.UserElevatorState.Elevating;
                        }
                    }
                }
                else
                {
                    int usersFloor = Mathf.RoundToInt(user.m_position.Y);
                    int userElevatorLocalID = pos.IndexOf(usersFloor);
                    if(userElevatorLocalID == -1)
                        continue; // No elevator on user's floor

                    int elevatorIndex = ids[userElevatorLocalID];
                    user.elevatorState = ElevatorUser.UserElevatorState.GoingIn;
                    user.targetElevatorIndex = elevatorIndex;
                    float randomXOffset = 0.06f * (0.5f - GD.Randf());
                    user.SetWalkTarget(elevators[elevatorIndex].GetHorizontalPos() + randomXOffset);
                }
            }
        }
    }
}
