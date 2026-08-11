using Godot;
using System;

public partial class CruiseSoundManager : Node2D
{
    [Export] private AudioStream cruiseSound;
    [Export] private float maxVolumeDB = 2.0f;
    [Export] Vector2 fadeInFadeOutTimers = new(1.0f, 1.0f);

    private float targetVolume = 0.0f;
    private float slopeVolumePerSec = 0.0f;
    private float minVolume = 0.0f;
    private float maxVolume = 0.0f;
    private bool lerping = false;

    private AudioStreamPlayer2D soundPlayer;
    private bool elevatorWasMoving = false;
    private bool playing = false;

    public void Init(int scaleIndex)
    {
        soundPlayer = ScaleGenerator.GeneratePlayerForPitchedSound(cruiseSound, scaleIndex, "FX");
        soundPlayer.VolumeDb = Mathf.LinearToDb(minVolume);
        AddChild(soundPlayer);

        maxVolume = Mathf.DbToLinear(maxVolumeDB);

        soundPlayer.Finished += LoopSound;
        soundPlayer.Play();
    }

    public override void _Process(double dt)
    {
        if(lerping == false)
            return;

        float volumeDelta = slopeVolumePerSec * (float)dt;
        float nextVolume = Mathf.DbToLinear(soundPlayer.VolumeDb) + volumeDelta;

        if(slopeVolumePerSec < 0.0f && nextVolume < targetVolume)
        {
            // Reached end of fade out
            lerping = false;
            soundPlayer.VolumeDb = Mathf.LinearToDb(0);
        }
        else if(slopeVolumePerSec > 0.0f && nextVolume > targetVolume)
        {
            // Reached end of fade in
            lerping = false;
            soundPlayer.VolumeDb = maxVolumeDB;
        }
        else
        {
            soundPlayer.VolumeDb = Mathf.LinearToDb(nextVolume);
        }
    }

    public void UpdateState(bool moving)
    {
        if(moving == elevatorWasMoving)
            return;

        if(moving)
        {
            // Start sound
            StartLerp(maxVolume);
        }
        else
        {
            // end sound
            StartLerp(minVolume);
        }

        elevatorWasMoving = moving;
    }

    private void StartLerp(float target)
    {
        float time = target > minVolume ? fadeInFadeOutTimers.X : fadeInFadeOutTimers.Y;
        slopeVolumePerSec = (target - Mathf.DbToLinear(soundPlayer.VolumeDb)) / time;
        targetVolume = target;

        lerping = true;
    }

    private void LoopSound()
    {
        soundPlayer.Play();
    }
}
