using Godot;
using System;

public partial class MusicManager : Node
{
    [Export] UserManager userManager;
    [Export] private AudioStreamPlayer2D chillMusic;
    [Export] private AudioStreamPlayer2D tenseMusic;
    [Export] private AudioStreamPlayer2D chaosMusic;
    [Export] private float maxVolumeDb = 0.0f;

    [Export] private float chillToTense = 0.5f;
    [Export] private float tenseToChaos = 0.9f;

    [Export] private bool debugDisplay = true;

    private int previousDisplayDebug = 0;

    public override void _Ready()
    {
        chillMusic.Finished += LoopChill;
        tenseMusic.Finished += LoopTense;
        chaosMusic.Finished += LoopChaos;
    }

    public override void _Process(double delta)
    {
        chillMusic.VolumeDb = -80.0f;
        tenseMusic.VolumeDb = -80.0f;
        chaosMusic.VolumeDb = -80.0f;
        float chaos = userManager.chaosMeter;

        int displayDebug;

        if(chaos < chillToTense)
        {
            // should play chill
            chillMusic.VolumeDb = maxVolumeDb;
            displayDebug = 1;
        }
        else if(chaos > tenseToChaos)
        {
            // should play chaos
            chaosMusic.VolumeDb = maxVolumeDb;
            displayDebug = 3;
        }
        else
        {
            // should play tense
            tenseMusic.VolumeDb = maxVolumeDb;
            displayDebug = 2;
        }

        if(debugDisplay && displayDebug != previousDisplayDebug)
        {
            previousDisplayDebug = displayDebug;
            GD.Print("Music " + displayDebug + "/3");
        }

    }

    private void LoopChill() { chillMusic.Play(); }
    private void LoopTense() { tenseMusic.Play(); }
    private void LoopChaos() { chaosMusic.Play(); }
}
