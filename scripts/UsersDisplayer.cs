using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

public partial class UsersDisplayer : Node
{
    [Export] private Node2D sceneryHolder;
    private List<RichTextLabel> displays = new();

    public void DisplayUsers(List<ElevatorUser> users)
    {
        while(displays.Count < users.Count)
            CreateNewDisplay();

        int i = 0;
        while(i < users.Count)
        {
            RichTextLabel text = displays[i];
            ElevatorUser user = users[i];

            text.Position = DisplayUtils.ComputeScreenPosFromPos(user.m_position);
            text.Text = user.m_destination.ToString();

            i++;
        }

        while(i < displays.Count)
        {
            displays[i++].Position = new(-10000.0f, -10000.0f);
        }
    }

    private void CreateNewDisplay()
    {
        displays.Add(new());
        sceneryHolder.AddChild(displays.Last());

        RichTextLabel text = displays.Last();
        text.FitContent = true;
        text.ScrollActive = false;
        text.AutowrapMode = TextServer.AutowrapMode.Off;
        text.HorizontalAlignment = HorizontalAlignment.Center;
        text.AddThemeFontSizeOverride("normal_font_size", 40);
        text.AddThemeColorOverride("default_color", new(0, 0, 0, 1));
    }
}
