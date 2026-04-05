using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

public partial class UsersDisplayer : Node
{
    [Export] private Node2D sceneryHolder;
    [Export] private Vector2I textureSize;
    [Export] private Vector2I circleRadii = new Vector2I(10, 50);
    [Export] private int outlineSize = 5;
    [Export] private float animSpeed = 1.0f;
    [Export] private float animAmplitude = 0.2f;
    private List<UserSprite> displays = new();

    private BodyGenerator bodyGenerator = new();

    public void DisplayUsers(List<ElevatorUser> users, double dt)
    {
        while(displays.Count < users.Count)
            CreateNewDisplay();

        int i = 0;
        while(i < users.Count)
        {
            UserSprite display = displays[i];
            ElevatorUser user = users[i];
            display.Position = DisplayUtils.ComputeScreenPosFromPos(user.m_position);

            display.Update(dt);

            i++;
        }

        displays.RemoveRange(i, displays.Count - i);
    }

    private void CreateNewDisplay()
    {
        displays.Add(new(bodyGenerator.Generate(textureSize, outlineSize, circleRadii)));
        sceneryHolder.AddChild(displays.Last());
    }

    public void OnScreenResize()
    {
        displays.ForEach((d) => { d.OnScreenResize(); d.Update(0.0f); });
    }
}
