using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
    [Export] public float elevatorSpeed = 1.0f;
    [Export] public float elevatorDoorSpeed = 1.0f;
    [Export] private Node sceneryNode;
    [Export] private PackedScene elevatorDisplayerScene;
    [Export] private UserManager usersManager;
    [Export] private BackgroundDisplayer backgroundDisplayer;
    [Export] private GameClockManager gameClockManager;

    private List<Elevator> elevators = [];

    private int selectedElevator = 0;

    public override void _Ready()
    {
        usersManager.InitUsers();

        int elevatorCount = 2;
        for(int i = 0; i < elevatorCount; ++i)
        {
            ElevatorDisplayer elevatorDisplayer = elevatorDisplayerScene.Instantiate<ElevatorDisplayer>();
            sceneryNode.AddChild(elevatorDisplayer);
            elevators.Add(new(0.0f, elevatorSpeed, elevatorDoorSpeed, elevatorDisplayer));

            elevatorDisplayer.horizontalRatio = (i + 1.0f) / (elevatorCount + 1.0f);
        }

        GetViewport().SizeChanged += OnScreenResize;
        OnScreenResize();
    }

    public void OnInputUp()
    {
        if(elevators[selectedElevator].m_targetPosition < 5.0f)
        {
            elevators[selectedElevator].m_targetPosition += 1.0f;
            elevators[selectedElevator].forceDisplayUpdate = true;
        }
    }
    public void OnInputDown()
    {
        if(elevators[selectedElevator].m_targetPosition > 0)
        {
            elevators[selectedElevator].m_targetPosition -= 1.0f;
            elevators[selectedElevator].forceDisplayUpdate = true;
        }
    }
    public void OnInputLeft()
    {
        if(selectedElevator - 1 >= 0)
            selectedElevator--;
        UpdateSelectionDisplay();
    }
    public void OnInputRight()
    {
        if(selectedElevator + 1 < elevators.Count)
            selectedElevator++;
        UpdateSelectionDisplay();
    }

    public override void _Process(double dt)
    {
        elevators.ForEach((e) => e.Update(dt));
        usersManager.UpdateUsers(dt, elevators);

        gameClockManager.AdvanceClock(dt);
    }

    private void UpdateSelectionDisplay()
    {
        backgroundDisplayer.MoveSelection((selectedElevator + 1) / 3.0f);
    }

    private void OnScreenResize()
    {
        DisplayUtils.screenSize = GetViewport().GetVisibleRect().Size;
        elevators.ForEach((e) => e.forceDisplayUpdate = true);
        backgroundDisplayer.UpdateScenery();
        usersManager.OnScreenResize();
    }
}