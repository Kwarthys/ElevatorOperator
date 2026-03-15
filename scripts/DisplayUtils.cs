using Godot;
using System;

public static class DisplayUtils
{
    public static Vector2 screenSize = Vector2.One;
    public static float maxFloors = 6.0f;
    public static Vector2 ComputeScreenPosFromPos(Vector2 _pos)
    {
        float step = screenSize.Y / (maxFloors + 1.0f);
        return new(screenSize.X * _pos.X, (maxFloors - _pos.Y) * step);
    }
}
