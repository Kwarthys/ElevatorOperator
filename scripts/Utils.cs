using Godot;
using System;

public static class Utils
{
    public static bool SpeedMove(double dt, float speed, float pos, float target, out float newPos)
    {
        float delta = speed * (float)dt;

        if(target < pos)
            delta *= -1.0f;

        if(Mathf.Abs(delta) > Mathf.Abs(target - pos))
        {
            // Overshoot, snap and be done
            newPos = target;
            return true;
        }
        else
        {
            newPos = pos + delta;
            return false; // move not done
        }
    }

    public static RichTextLabel GenerateTextLabel(string text, Color c, int fontSize)
    {
        RichTextLabel label = new()
        {
            FitContent = true,
            ScrollActive = false,
            AutowrapMode = TextServer.AutowrapMode.Off,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = text,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Size = new(100.0f, 100.0f)
        };

        label.AddThemeFontSizeOverride("normal_font_size", fontSize);
        label.AddThemeColorOverride("default_color", c);
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeColorOverride("font_outline_color", new(1, 1, 1, 0.8f));

        return label;
    }
}
