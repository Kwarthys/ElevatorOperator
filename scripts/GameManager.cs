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
    [Export] private Button startGameButton;

    public float currentGameDuration { get; private set; } = 0.0f;

    private List<Elevator> elevators = [];

    private int selectedElevator = 0;
    private bool gameStarted = false;

    public override void _Ready()
    {
        int elevatorCount = 3;
        for(int i = 0; i < elevatorCount; ++i)
        {
            ElevatorDisplayer elevatorDisplayer = elevatorDisplayerScene.Instantiate<ElevatorDisplayer>();
            elevatorDisplayer.soundScaleIndex = 2 * i;
            sceneryNode.AddChild(elevatorDisplayer);
            elevators.Add(new(0.0f, elevatorSpeed, elevatorDoorSpeed, elevatorDisplayer));

            elevatorDisplayer.horizontalRatio = (i + 1.0f) / (elevatorCount + 1.0f);
        }

        usersManager.HideGameOverScreen();

        GetViewport().SizeChanged += OnScreenResize;
        OnScreenResize();
    }

    public void StartGame()
    {
        StatisticsManager.Reset();
        currentGameDuration = 0.0f;
        gameStarted = true;

        usersManager.InitUsers();
        usersManager.HideGameOverScreen();
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
        elevators.ForEach((e) => e.Update(dt)); // Let player move elevator even before the start

        if(gameStarted == false)
            return;

        usersManager.UpdateUsers(dt, elevators);

        gameClockManager.AdvanceClock(dt);

        if(usersManager.gameLost == false)
            currentGameDuration += (float)dt;
    }

    public void OnStartGameButtonClick()
    {
        startGameButton.Visible = false;
        StartGame();
    }

    private void UpdateSelectionDisplay()
    {
        backgroundDisplayer.MoveSelection(1.0f * (selectedElevator + 1) / (elevators.Count + 1));
    }

    private void OnScreenResize()
    {
        DisplayUtils.screenSize = GetViewport().GetVisibleRect().Size;
        elevators.ForEach((e) => e.forceDisplayUpdate = true);
        backgroundDisplayer.UpdateScenery();
        usersManager.OnScreenResize();

        startGameButton.Position = (DisplayUtils.screenSize - startGameButton.Size) * 0.5f;
    }
}