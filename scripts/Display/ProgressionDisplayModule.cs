using Godot;
using System;
using System.Linq;

public partial class ProgressionDisplayModule : Node
{
    [Export] private Godot.Collections.Array<Control> progressionSprites;
    [Export] private Godot.Collections.Array<float> progressionThresholdsDays;
    [Export] private Slider progressBar;

    [Export] private float animationDelay = 1.0f; // Will wait this amount of time on start before animating
    [Export] private float animationDuration = 5.0f;

    private bool animating = false;
    private float progressionTarget = 0.0f;
    private double dtCounter = 0.0;

    public override void _Ready()
    {
        for(int i = 0; i < progressionSprites.Count; ++i)
            DimElement(i);
    }

    public void AnimateTo(float progression)
    {
        animating = true;
        progressionTarget = progression;
        dtCounter = 0.0;
    }

    public override void _Process(double dt)
    {
        if(animating == false)
            return;

        dtCounter += dt;
        float progression = ((float)dtCounter - animationDelay) / animationDuration;
        if(progression < 0.0f)
            return;

        if(progression > 1.0f)
        {
            animating = false;
            return;
        }

        float actualProgress = Smooth(progression);
        float scoreProgress = progressionTarget * actualProgress;

        progressBar.Value = scoreProgress / progressionThresholdsDays.Last();

        for(int i = 0; i < progressionSprites.Count; ++i)
        {
            if(progressionThresholdsDays[i] > scoreProgress)
            {
                DimElement(i);
            }
            else
            {
                ShowElement(i);
            }
        }
    }

    private float Smooth(float x)
    {
        return -x * x + 2.0f * x; // Starts at (0,0), peaks at (1,1)
    }

    private void DimElement(int index) { progressionSprites[index].Modulate = new(0.2f, 0.2f, 0.2f, 0.8f); }
    private void ShowElement(int index) { progressionSprites[index].Modulate = new(1.0f, 1.0f, 1.0f, 1.0f); }
}
