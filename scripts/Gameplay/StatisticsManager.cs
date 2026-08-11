using Godot;
using System;

public static class StatisticsManager
{
    public static int numberOfTravels { get; private set; } = 0;

    public static void Reset()
    {
        numberOfTravels = 0;
    }

    public static void IncrNumberOfTravels() { numberOfTravels++; }
}
