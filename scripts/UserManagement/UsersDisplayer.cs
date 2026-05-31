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

    private BodyGenerator bodyGenerator = new();

    private Dictionary<ElevatorUser, UserSprite> userToSpriteMap = [];

    public void DisplayUsers(List<ElevatorUser> users, double dt)
    {
        foreach(ElevatorUser user in users)
        {
            UserSprite sprite;
            if(userToSpriteMap.ContainsKey(user) == false)
                sprite = CreateNewDisplay(user);
            else
                sprite = userToSpriteMap[user];

            sprite.Position = DisplayUtils.ComputeScreenPosFromPos(user.m_position);
            sprite.Update(dt);
        }
    }

    private UserSprite CreateNewDisplay(ElevatorUser user)
    {
        UserSprite sprite = new(bodyGenerator.Generate(textureSize, outlineSize, circleRadii));
        userToSpriteMap.Add(user, sprite);
        sceneryHolder.AddChild(sprite);
        return sprite;
    }

    public void OnScreenResize()
    {
        foreach(UserSprite sprite in userToSpriteMap.Values)
        {
            sprite.OnScreenResize();
            sprite.Update(0.0f);
        }
    }
}
