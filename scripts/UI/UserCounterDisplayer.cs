using Godot;
using System;

public partial class UserCounterDisplayer : RichTextLabel
{
    [Export] private UserManager userManager;
    public override void _Process(double _)
    {
        Text = userManager.GetUserCount().ToString();

    }

}
