using Godot;
using System;

public class UserSchedule
{
    public int leaveHour = 0;
    public int leaveMinute = 0;
    public int backHour = 0;
    public int backMinute = 0;

    private int leaveInMinutes; // hours and minutes encoded as minutes for comparisons
    private int backInMinutes;

    public bool ShouldLeave()
    {
        int timeInMinutes = GameClockManager.clock.TimeOfDayInMinutes();
        if(leaveHour < backHour)
        {
            return timeInMinutes > leaveInMinutes && timeInMinutes < backInMinutes;
        }
        else
        {
            return timeInMinutes < backInMinutes || timeInMinutes > leaveInMinutes;
        }
    }

    public bool ShouldBack() { return !ShouldLeave(); }

    public static UserSchedule Generate()
    {
        float scheduleType = GD.Randf();
        UserSchedule sc = new();

        // | --- FullRandom --- | --- Classic --- | --- Reversed --- |
        // 0 ----------------- 0.1 ------------- 0.7 --------------- 1
        //         10%                  60%               30%

        if(scheduleType < 0.1f) // 10% full random
        {
            sc.leaveHour = (int)(GD.Randi() % 24);
            sc.leaveMinute = (int)(GD.Randi() % 60);
            sc.backMinute = (int)(GD.Randi() % 60);

            int outTime = GD.RandRange(3, 12);
            sc.backHour = (sc.leaveHour + outTime) % 24;
        }
        else
        {
            sc.leaveHour = GD.RandRange(5, 12);
            sc.leaveMinute = (int)(GD.Randi() % 60);
            sc.backHour = GD.RandRange(16, 26) % 24;
            sc.backMinute = (int)(GD.Randi() % 60);

            if(scheduleType > 0.7f) // 30% flipped schedule
            {
                (sc.backHour, sc.leaveHour) = (sc.leaveHour, sc.backHour);
            }
        }

        sc.backInMinutes = sc.backHour * 60 + sc.backMinute;
        sc.leaveInMinutes = sc.leaveHour * 60 + sc.leaveMinute;

        return sc;
    }
}
