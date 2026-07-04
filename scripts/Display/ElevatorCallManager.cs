using Godot;
using System;
using System.Collections.Generic;

public partial class ElevatorCallManager : Node
{
    [Export] private Color offColor = new(0.1f, 0.1f, 0.1f);
    [Export] private Color litColor = new(1.0f, 1.0f, 0.0f);
    [Export] private float litTime = 1.0f;

    [Export] private bool testButton = false;

    private List<Sprite2D> callButtons = [];
    private List<float> litTimers = [];
    private static ElevatorCallManager Instance;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double dt)
    {
        if(testButton)
        {
            testButton = false;
            int randIndex = GD.RandRange(0, litTimers.Count / 2 - 1);
            CallElevator(randIndex);
        }

        for(int i = 0; i < callButtons.Count; ++i)
        {
            if(litTimers[i] < 0.0f)
                continue;

            litTimers[i] -= (float)dt;

            callButtons[i].Skew = Mathf.Cos(litTimers[i]);

            if(litTimers[i] < 0.0f)
            {
                callButtons[i].SelfModulate = offColor;
                callButtons[i].Skew = 0;
                callButtons[i].Rotation = 0.0f;
            }
            else
            {
                callButtons[i].Skew = litTimers[i] * litTimers[i] * 0.8f * Mathf.Sin(litTimers[i] * 30.0f); // Fast decrease fast frequency
                callButtons[i].Rotation = litTimers[i] * 0.5f * Mathf.Sin(litTimers[i] * 2.0f * Mathf.Tau / litTime); // low decrease low frequency

                if(i % 2 == 0)
                    callButtons[i].Rotation *= -1.0f; // Reverse every other floor animation orientation for variety
            }
        }
    }


    public void RegisterCallButton(Sprite2D sprite)
    {
        callButtons.Add(sprite);
        litTimers.Add(-1.0f);
        sprite.SelfModulate = offColor;
    }

    public static void CallElevator(int floor) { Instance?.CallElevator_Private(floor); }

    private void CallElevator_Private(int floor)
    {
        int id = FloorToIndex(floor);
        litTimers[id] = litTime;
        litTimers[id + 1] = litTime;

        callButtons[id].SelfModulate = litColor;
        callButtons[id + 1].SelfModulate = litColor;
    }

    private int FloorToIndex(int floor) { return callButtons.Count - 2 * (floor + 1); }
}
