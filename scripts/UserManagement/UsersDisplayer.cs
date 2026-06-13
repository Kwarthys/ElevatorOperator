using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

public partial class UsersDisplayer : Node
{
    [Export] private Node2D sceneryHolder;
    [Export] private Vector2I textureSize;
    [Export] private Vector2I circleRadii = new(10, 50);
    [Export] private int outlineSize = 5;
    [Export] private Vector2 swaySpeed_MinMax = new(1.0f, 5.0f);
    [Export] private float swayAmplitude = 0.2f;
    [Export] private StyleBox patienceProgressBar_Fill;
    [Export] private StyleBox patienceProgressBar_Background;

    [Export] private bool displayDebugTexts = false;
    [Export] private bool displayPatienceBars = true;
    private bool displayDebugTextsMemory = false;
    private bool displayPatienceBarsMemory = true;

    private BodyGenerator bodyGenerator = new();

    private Dictionary<ElevatorUser, UserSprite> userToSpriteMap = [];

    private List<RichTextLabel> debugTexts = [];
    private List<ProgressBar> patienceBars = [];

    public void DisplayUsers(List<ElevatorUser> users, double dt)
    {
        if(displayDebugTexts)
        {
            while(debugTexts.Count < users.Count)
                InstantiateNewDebugText();
        }

        if(displayPatienceBars)
        {
            while(patienceBars.Count < users.Count)
                InstantiateNewPatienceBar();
        }

        int i;
        for(i = 0; i < users.Count; ++i)
        {
            UserSprite sprite;
            if(userToSpriteMap.ContainsKey(users[i]) == false)
                sprite = CreateNewDisplay(users[i]);
            else
                sprite = userToSpriteMap[users[i]];

            sprite.Position = DisplayUtils.ComputeScreenPosFromPos(users[i].m_position);

            float userImpatience = Mathf.Clamp(1.0f - users[i].GetPatience(), 0.0f, 1.0f);
            float swaySpeed = Mathf.Lerp(swaySpeed_MinMax.X, swaySpeed_MinMax.Y, userImpatience * userImpatience * userImpatience);
            sprite.Update(dt, swaySpeed);

            if(displayDebugTexts)
            {
                debugTexts[i].Text = users[i].GetScheduleDebugText();
                debugTexts[i].Position = sprite.Position;
            }

            if(displayPatienceBars)
            {
                patienceBars[i].Value = users[i].GetPatience();
                patienceBars[i].Position = sprite.Position;
            }
        }

        if(displayDebugTexts)
        {
            for(int j = i; j < debugTexts.Count; ++j)
            {
                debugTexts[j].Text = "";
            }
        }

        bool hideAllBars = displayPatienceBarsMemory && displayPatienceBarsMemory != displayPatienceBars;
        int hideBarsStartIndex = hideAllBars ? 0 : i;
        for(int j = hideBarsStartIndex; j < patienceBars.Count; ++j)
        {
            patienceBars[j].Position = new(-1000, -1000);
        }
        displayPatienceBarsMemory = displayPatienceBars;

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
        UserSprite sprite = new(bodyGenerator.Generate(textureSize, outlineSize, circleRadii), swayAmplitude);
        userToSpriteMap.Add(user, sprite);
        sceneryHolder.AddChild(sprite);
        return sprite;
    }

    public void OnScreenResize()
    {
        foreach(UserSprite sprite in userToSpriteMap.Values)
        {
            sprite.OnScreenResize();
        }
    }

    private void InstantiateNewPatienceBar()
    {
        patienceBars.Add(new());
        ProgressBar bar = patienceBars.Last();

        bar.AddThemeStyleboxOverride("fill", patienceProgressBar_Fill);
        bar.AddThemeStyleboxOverride("background", patienceProgressBar_Background);

        bar.MaxValue = 1.0f;
        bar.CustomMinimumSize = new(70, 20);
        bar.ShowPercentage = false;

        sceneryHolder.AddChild(bar);
    }

    private void InstantiateNewDebugText()
    {
        RichTextLabel label = Utils.GenerateTextLabel("", new Color(0.0f, 0.0f, 0.0f), 19);
        sceneryHolder.AddChild(label);
        debugTexts.Add(label);
    }
}
