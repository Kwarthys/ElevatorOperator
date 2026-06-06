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

    [Export] private bool displayDebugTexts = false;
    private bool displayDebugTextsMemory = false;

    private BodyGenerator bodyGenerator = new();

    private Dictionary<ElevatorUser, UserSprite> userToSpriteMap = [];

    private List<RichTextLabel> debugTexts = [];

    public void DisplayUsers(List<ElevatorUser> users, double dt)
    {
        while(debugTexts.Count < users.Count)
            InstantiateNewDebugText();

        int i;
        for(i = 0; i < users.Count; ++i)
        {
            UserSprite sprite;
            if(userToSpriteMap.ContainsKey(users[i]) == false)
                sprite = CreateNewDisplay(users[i]);
            else
                sprite = userToSpriteMap[users[i]];

            sprite.Position = DisplayUtils.ComputeScreenPosFromPos(users[i].m_position);
            sprite.Update(dt);

            if(displayDebugTexts)
            {
                debugTexts[i].Text = users[i].GetScheduleDebugText();
                debugTexts[i].Position = sprite.Position;
            }
        }

        if(displayDebugTexts)
        {
            for(int j = i; j < debugTexts.Count; ++j)
            {
                debugTexts[j].Text = "";
            }
        }

        if(displayDebugTextsMemory != displayDebugTexts)
        {
            if(displayDebugTextsMemory)
            {
                for(int ti = 0; ti < debugTexts.Count; ++ti)
                {
                    debugTexts[ti].Text = "";
                }
            }

            displayDebugTextsMemory = displayDebugTexts;
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

    private void InstantiateNewDebugText()
    {
        RichTextLabel label = Utils.GenerateTextLabel("debug", new Color(0.0f, 0.0f, 0.0f), 19);
        sceneryHolder.AddChild(label);
        debugTexts.Add(label);
    }
}
