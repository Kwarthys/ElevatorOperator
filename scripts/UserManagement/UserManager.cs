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
    [Export] private EndGameAnimationDriver endGameAnimation;

    public static float[] impatienceThresholds = [0.5f, 0.25f, 0.1f];
    private List<Inhabitant> inhabitants = [];

    private double addUserDTCounter = 0.0f;

    public bool gameLost { get; private set; } = false;

    [Export] private int chaosMeterUserCountMax = 30;
    public float chaosMeter { get; private set; } = 0.0f;

    public void UpdateUsers(double dt, List<Elevator> elevators)
    {
        List<ElevatorUser> baseUsers = [];
        inhabitants.ForEach(baseUsers.Add); // convert list of child elements to list of base elements
        usersDisplayer.DisplayUsers(baseUsers, dt);

        int usersToManage = 0;

        inhabitants.ForEach((u) =>
        {
            if(gameLost == false)
                u.UpdateBehavior(dt, elevators);
            u.UpdateWalk(dt);

            if(u.elevatorIndex != -1)
            {
                u.m_position.Y = elevators[u.elevatorIndex].m_position;

                // Manage floor button impatience press
                if(u.ShouldReCall())
                {
                    elevators[u.elevatorIndex].RequestFloor(u.m_destination);
                }
            }

            if(u.NeedsALift())
                usersToManage++;

            if(gameLost == false && u.GetPatience() == 0.0f)
            {
                gameLost = true;
                StatisticsManager.userLostSchedule = u.GetSchedule();
                StatisticsManager.userLostID = inhabitants.IndexOf(u) + 1;
                GameManager.OnGameLost();
                endGameAnimation.Start();
            }
        });

        chaosMeter = Mathf.Min(1.0f, 1.0f * usersToManage / chaosMeterUserCountMax);

        if(gameLost)
            return; // stop adding users when game is lost

        StatisticsManager.registerFrameUsers(usersToManage);

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
        usersDisplayer.Reset();
        inhabitants.Clear();
        gameLost = false;
        for(int i = 0; i < startingUserCount; ++i)
        {
            GenerateUser();
        }
    }

    public int GetUserCount() { return inhabitants.Count; }

    private ElevatorUser GenerateUser()
    {
        inhabitants.Add(new(GD.RandRange(1, 5), usersWalkSpeed * Mathf.Lerp(0.7f, 1.0f, GD.Randf())));
        return inhabitants.Last();
    }
}
