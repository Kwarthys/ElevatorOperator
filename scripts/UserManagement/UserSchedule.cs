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


    public int GetLeaveTimeInMinute() { return leaveInMinutes; }
    public int GetBackTimeInMinute() { return backInMinutes; }

    public bool ShouldLeave()
    {
        int timeInMinutes = GameClockManager.clock.TimeOfDayInMinutes();
        if(leaveHour < backHour)
        {
            return timeInMinutes >= leaveInMinutes && timeInMinutes < backInMinutes;
        }
        else
        {
            return timeInMinutes < backInMinutes || timeInMinutes >= leaveInMinutes;
        }
    }

    public bool ShouldBack() { return !ShouldLeave(); }

    public static UserSchedule GenerateForFirstHours()
    {
        UserSchedule sc = new();
        sc.leaveHour = GD.RandRange(0, 4);
        sc.leaveMinute = (int)(GD.Randi() % 60);
        sc.backMinute = (int)(GD.Randi() % 60);
        sc.backHour = sc.leaveHour + 12;

        if(GD.Randf() > 0.5f)
        {
            (sc.backHour, sc.leaveHour) = (sc.leaveHour, sc.backHour);
        }

        sc.backInMinutes = sc.backHour * 60 + sc.backMinute;
        sc.leaveInMinutes = sc.leaveHour * 60 + sc.leaveMinute;

        return sc;
    }

    public static UserSchedule Generate(float commute)
    {
        float scheduleType = GD.Randf();
        UserSchedule sc = new();

        // | --- FullRandom --- | --- Classic --- | --- Reversed --- |
        // 0 -----------------0.05 ------------ 0.85 --------------- 1
        //          5%                  80%               15%

        if(scheduleType < 0.05f) // 10% full random
        {
            sc.leaveHour = (int)(GD.Randi() % 24);
            sc.leaveMinute = (int)(GD.Randi() % 60);
            sc.backMinute = (int)(GD.Randi() % 60);

            int outHours = Mathf.RoundToInt(commute / 60.0f);
            sc.backHour = (sc.leaveHour + outHours) % 24;
        }
        else
        {
            sc.leaveHour = GD.RandRange(5, 12);
            sc.leaveMinute = (int)(GD.Randi() % 60);
            sc.backMinute = (int)(GD.Randi() % 60);

            int outHours = Mathf.RoundToInt(commute / 60.0f);
            sc.backHour = (sc.leaveHour + outHours) % 24;

            if(scheduleType > 0.85f) // 15% flipped schedule
            {
                (sc.backHour, sc.leaveHour) = (sc.leaveHour, sc.backHour);
            }
        }

        sc.backInMinutes = sc.backHour * 60 + sc.backMinute;
        sc.leaveInMinutes = sc.leaveHour * 60 + sc.leaveMinute;

        return sc;
    }

    public override string ToString()
    {
        return "Leaves: " + (leaveHour < 10 ? "0" : "") + leaveHour + ":" + (leaveMinute < 10 ? "0" : "") + leaveMinute
        + "\nBacks:   " + (backHour < 10 ? "0" : "") + backHour + ":" + (backMinute < 10 ? "0" : "") + backMinute;
    }

}
