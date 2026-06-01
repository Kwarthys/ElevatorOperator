using Godot;
using System;

public partial class GameClockManager : Node
{
    [Export] private double gameHourDuration_sec = 60.0;
    [Export] private double gameHourDurationNightTime_sec = 60.0; // Todo accelerate time during night

    public static GameClock clock = new(); // static for easy (and dirty) access accross the project

    public void AdvanceClock(double _dt)
    {
        clock.seconds += _dt * 3600.0 / gameHourDuration_sec;
        clock.Trim();
    }
}

// Clock used to drive characters behavior and user display
public class GameClock
{
    public int days = 0;
    public int hours = 0;
    public int minutes = 0;
    public double seconds = 0.0;

    public void Trim()
    {
        while(seconds > 60.0)
        {
            minutes++;
            seconds -= 60.0;

            GD.Print(this); // Todo proper UI display
        }
        while(minutes >= 60)
        {
            hours++;
            minutes -= 60;
        }
        while(hours >= 24)
        {
            days++;
            hours -= 24;
        }
    }

    public override string ToString()
    {
        return "Day " + days + ", " + hours + ":" + minutes + ":" + seconds;
    }
}
