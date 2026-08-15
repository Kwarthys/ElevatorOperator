using Godot;
using System;

public partial class EndGameAnimationDriver : Node
{
    [Export] private float upPeriod = 0.5f;
    [Export] private float upPeriodAccumulativeCoef = 0.1f;
    [Export] private float downPeriod = 0.25f;
    [Export] private float highPause = 1.0f;
    [Export] private bool debugPlay = false;

    private float dtAccumulator = 0.0f;
    private int maxIndex = 5;
    private int currentIndex = 0;
    private bool goingUp = true;
    private bool playing = false;

    public void Start()
    {
        dtAccumulator = upPeriod;
        playing = true;
        goingUp = true;
    }

    public override void _Process(double dt)
    {
        if(debugPlay)
        {
            Start();
            debugPlay = false;
        }

        if(playing == false)
            return;

        dtAccumulator += (float)dt;

        float timer;
        if(goingUp)
            timer = upPeriod + upPeriodAccumulativeCoef * currentIndex;
        else
        {
            if(currentIndex == maxIndex) // just made the turn
                timer = highPause;
            else
                timer = downPeriod;
        }

        if(dtAccumulator > timer)
        {
            ElevatorCallManager.CallElevator(currentIndex);

            dtAccumulator -= timer;

            if(goingUp)
            {
                if(currentIndex < maxIndex)
                    currentIndex++;
                else
                    goingUp = false;
            }
            else
            {
                if(currentIndex > 0)
                    currentIndex--;
                else
                    playing = false;
            }
        }
    }


}
