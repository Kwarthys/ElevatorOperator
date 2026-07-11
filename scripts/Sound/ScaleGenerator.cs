using Godot;
using System;

public class ScaleGenerator
{
    // 2 2 1 2 2 2 1 half tones between MAJOR scale notes
    private static int[] SCALE = [2, 2, 1, 2, 2, 2, 1];
    public static AudioStreamPlayer2D GeneratePlayerForPitchedSound(AudioStream source, int scaleStep, string audioBus)
    {
        int halfNotes = 0;
        for(int i = 0; i < scaleStep; ++i)
        {
            halfNotes += SCALE[i % SCALE.Length];
        }

        AudioStreamPlayer2D player = new()
        {
            Stream = source,
            Bus = audioBus,
            PitchScale = Mathf.Pow(2, halfNotes / 12.0f)
        };

        return player;
    }
}
