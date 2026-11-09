using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UserManager : Node
{
    [Export] private UsersDisplayer usersDisplayer;
    [Export] public float usersWalkSpeed = 0.5f;
    [Export] private int startingUserCount = 20;
    [Export] private float addUserPeriod = 5.0f;
    [Export] private double technicianRepairTime = 10.0;
    [Export] private EndGameAnimationDriver endGameAnimation;
    [Export] private Vector2 commuteMinMax = new(2.0f, 10.0f);
    [Export] private int dayForMinCommute = 4;
    private int minutesForMinCommute;

    public static float[] impatienceThresholds = [0.5f, 0.25f, 0.1f];
    private List<Inhabitant> inhabitants = [];
    private List<ElevatorTechos> technicians = [];

    private double addUserDTCounter = 0.0;

    public bool gameLost { get; private set; } = false;

    [Export] private int chaosMeterUserCountMax = 40;
    public float chaosMeter { get; private set; } = 0.0f;

    public void UpdateUsers(double dt, List<Elevator> elevators)
    {
        List<ElevatorUser> baseUsers = [];
        inhabitants.ForEach(baseUsers.Add); // convert list of child elements to list of base elements
        technicians.ForEach(baseUsers.Add); // Regroup both children
        usersDisplayer.DisplayUsers(baseUsers, dt);

        int usersToManage = 0;

        for(int i = technicians.Count - 1; i >= 0; --i)
        {
            if(gameLost == false)
            {
                technicians[i].UpdateBehavior(dt, elevators);

                if(technicians[i].m_movementState == ElevatorUser.UserMovementState.Outside && technicians[i].IsJobDone() && technicians[i].m_walking == false)
                {
                    usersDisplayer.DestroySprite(technicians[i]);
                    technicians.RemoveAt(i);
                    continue;
                }
            }
            technicians[i].UpdateWalk(dt);
        }

        inhabitants.ForEach((u) =>
        {
            if(gameLost == false)
                u.UpdateBehavior(dt, elevators);
            u.UpdateWalk(dt);

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
            GenerateUser();
            addUserDTCounter -= addUserPeriod;
        }
    }

    public void SpawnTechnician(Elevator _target)
    {
        technicians.Add(new(technicianRepairTime, usersWalkSpeed, _target));
    }

    public void OnScreenResize()
    {
        usersDisplayer.OnScreenResize();
    }

    public void InitUsers()
    {
        minutesForMinCommute = dayForMinCommute * 24 * 60;

        usersDisplayer.Reset();
        inhabitants.Clear();
        gameLost = false;
        for(int i = 0; i < startingUserCount; ++i)
        {
            GenerateUser();
        }
    }

    public int GetUserCount() { return inhabitants.Count; }

    private Inhabitant GenerateUser()
    {
        inhabitants.Add(new(GD.RandRange(1, 5), usersWalkSpeed * Mathf.Lerp(0.7f, 1.0f, GD.Randf()), ComputeCommuteDifficulty()));
        return inhabitants.Last();
    }

    private float ComputeCommuteDifficulty()
    {
        int minutes = GameClockManager.clock.TimeOfDayInMinutes() + GameClockManager.clock.days * 24 * 60;
        float lerpTime = 1.0f * (minutesForMinCommute - minutes) / minutesForMinCommute;
        return Mathf.Lerp(commuteMinMax.X, commuteMinMax.Y, lerpTime) * 60.0f;
    }
}
