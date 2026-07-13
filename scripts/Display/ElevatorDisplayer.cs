using Godot;
using System.Collections.Generic;

public partial class ElevatorDisplayer : Node2D
{
    [Export] private AnimatedSprite2D elevator;
    [Export] private Sprite2D targetModel;
    [Export] private Color restSelectionColor;
    [Export] private Color activeSelectionColor;
    [Export] private float sizeScreenRatio = 0.15f;
    [Export] private float signAnimationDuration = 1.0f;
    private List<RichTextLabel> floorSigns = [];
    private List<float> animationTimers = [];

    private Texture2D elevatorTexture;

    public float horizontalRatio = 0.5f;

    public override void _Ready()
    {
        Position = Vector2.Zero; // make sure nothing is offset
        SetFloorSelection(0);
        elevatorTexture = elevator.SpriteFrames.GetFrameTexture("doors", 0);

        for(int i = 0; i < 6; ++i)
        {
            RichTextLabel sign = Utils.GenerateTextLabel(i.ToString(), restSelectionColor, 60);
            AddChild(sign);
            floorSigns.Add(sign);
            sign.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            sign.Scale = new(0.5f, 0.5f);
            sign.PivotOffset = sign.Size * 0.5f;

            animationTimers.Add(-1.0f);
        }
    }

    public void UpdateDisplayPos(float pos, float targetPos)
    {
        elevator.Position = DisplayUtils.ComputeScreenPosFromPos(new(horizontalRatio, pos));
        targetModel.Position = DisplayUtils.ComputeScreenPosFromPos(new(horizontalRatio, targetPos));
    }

    public void UpdateScale()
    {
        float targetSize = sizeScreenRatio * DisplayUtils.screenSize.Y;
        float scaler = targetSize / elevatorTexture.GetHeight();

        elevator.Scale = new(scaler, scaler);
        targetModel.Scale = new(scaler * 0.9f, scaler * 0.9f);
    }

    public void UpdateSigns(double dt)
    {
        float availableXSpace = elevatorTexture.GetWidth() * elevator.Scale.X * 1.3f;
        float padding = 0.05f * availableXSpace;
        float signBaseSize = availableXSpace / 5.0f - padding;
        float startXPoint = availableXSpace * 0.5f;

        for(int i = 0; i < floorSigns.Count; ++i)
        {
            RichTextLabel sign = floorSigns[i];
            sign.Position = elevator.Position - sign.Size * 0.5f;
            sign.Position -= new Vector2(startXPoint - i * (padding + signBaseSize), elevatorTexture.GetHeight() * elevator.Scale.Y * 0.7f);

            float baseScale = signBaseSize / sign.Size.X;
            sign.Scale = new(baseScale, baseScale);

            if(animationTimers[i] > 0.0f)
            {
                sign.Scale *= 1.0f + 4.0f * animationTimers[i] * animationTimers[i] * animationTimers[i] * animationTimers[i];
                sign.Rotation = 1.2f * animationTimers[i] * animationTimers[i] * Mathf.Sin(animationTimers[i] * 2.0f * Mathf.Tau / signAnimationDuration);

                if(i % 2 == 0)
                    sign.Rotation *= -1.0f; // Reverse every other sign animation orientation for variety

                animationTimers[i] -= (float)dt;

                if(animationTimers[i] <= 0.0f)
                {
                    sign.Scale = new(baseScale, baseScale);
                    sign.Rotation = 0.0f;
                }
            }
        }
    }

    public void UpdateDoorDisplay(float status)
    {
        if(status < 0.05f)
            elevator.Frame = 0;
        else if(status < 0.5f)
            elevator.Frame = 1;
        else if(status < 0.95f)
            elevator.Frame = 2;
        else
            elevator.Frame = 3;
    }

    public void AnimateFloorSelection(int floor)
    {
        animationTimers[floor] = signAnimationDuration;
    }

    public void SetFloorSelection(int selectionFlags)
    {
        for(int i = 0; i < floorSigns.Count; ++i)
        {
            floorSigns[i].Clear();
            bool selected = (selectionFlags & (1 << i)) != 0;
            floorSigns[i].PushColor(selected ? activeSelectionColor : restSelectionColor);
            floorSigns[i].AddText(i.ToString());
            floorSigns[i].Pop();
        }
    }
}
