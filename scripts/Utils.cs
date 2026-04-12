using Godot;
using System;

public static class Utils
{
    public static bool SpeedMove(double dt, float speed, float pos, float target, out float newPos)
    {
        float delta = speed * (float)dt;

        if(target < pos)
            delta *= -1.0f;

        if(Mathf.Abs(delta) > Mathf.Abs(target - pos))
        {
            // Overshoot, snap and be done
            newPos = target;
            return true;
        }
        else
        {
            newPos = pos + delta;
            return false; // move not done
        }
    }
}
