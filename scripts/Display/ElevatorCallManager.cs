using Godot;
using System;
using System.Collections.Generic;

public partial class ElevatorCallManager : Node
{
    [Export] private Color offColor = new(0.1f, 0.1f, 0.1f);
    [Export] private Color litColor = new(1.0f, 1.0f, 0.0f);
    [Export] private float litTime = 1.0f;

    private List<Sprite2D> callButtons = [];
    private List<float> litTimers = [];
    private static ElevatorCallManager Instance;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double dt)
    {
        for(int i = 0; i < callButtons.Count; ++i)
        {
            if(litTimers[i] < 0.0f)
                continue;

            litTimers[i] -= (float)dt;

            if(litTimers[i] < 0.0f)
                callButtons[i].SelfModulate = offColor;
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
