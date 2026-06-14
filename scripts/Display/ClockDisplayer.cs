using Godot;
using System;

public partial class ClockDisplayer : RichTextLabel
{
    [Export] private StyleBoxFlat clockBackGroundTheme;
    [Export] private Color lightColor;
    [Export] private Color darkColor;

    public override void _Process(double _)
    {
        int hours = GameClockManager.clock.hours;
        Text = (hours < 10 ? "0" : "") + hours + ":";

        int minutes = GameClockManager.clock.minutes;
        Text += (minutes < 10 ? "0" : "") + minutes;

        if(hours >= 8 && hours < 22)
        {
            // Light Mode
            AddThemeColorOverride("default_color", darkColor);
            clockBackGroundTheme.BgColor = lightColor;
        }
        else
        {
            AddThemeColorOverride("default_color", lightColor);
            clockBackGroundTheme.BgColor = darkColor;
        }
    }
}
