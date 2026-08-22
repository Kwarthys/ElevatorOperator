using Godot;
using System;

public partial class EndGameScreenManager : Control
{
    private GameManager gameManager;

    public void RegisterManager(GameManager manager) { gameManager = manager; }

    public void OnRestartButtonPressed() { gameManager.OnRestartGameButtonClick(); }
}
