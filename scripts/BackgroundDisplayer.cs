using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BackgroundDisplayer : Node
{
    [Export] private Color backgroundColor;
    [Export] private Color selectionColor;
    [Export] private Gradient colors;
    [Export] private Node2D backgroundHolder;

    private List<SceneryRectangle> items = new();
    private SceneryRectangle selectionDisplayer;

    public override void _Ready()
    {
        items.Add(CreateItem(backgroundColor, new(0.5f, 0.5f), new(1.0f, 1.0f)));

        for(int i = 0; i < 6; ++i)
        {
            items.Add(CreateItem(SampleColor(i), new(0.5f, (i + 1) / 7.0f), new(1.0f, 0.01f)));
        }

        items.Add(CreateItem(selectionColor, new(1.0f / 4.0f, 0.5f), new(0.01f, 1.0f), true));
        selectionDisplayer = items.Last();

        UpdateScenery();
    }

    public void UpdateScenery()
    {
        foreach(SceneryRectangle r in items)
        {
            r.sprite.Scale = DisplayUtils.screenSize * r.size;
            r.sprite.Position = DisplayUtils.screenSize * r.center;
        }
    }

    public void MoveSelection(float x)
    {
        selectionDisplayer.center.X = x;
        UpdateScenery();
    }

    private SceneryRectangle CreateItem(Color c, Vector2 center, Vector2 size, bool centerGlow = false) // Last item added is on top
    {
        Sprite2D sprite = new();
        if(centerGlow)
            sprite.Texture = createTextureCenterGlow(c);
        else
            sprite.Texture = createTextureLowShadow(c);
        backgroundHolder.AddChild(sprite);

        SceneryRectangle item = new()
        {
            center = center,
            size = size,
            sprite = sprite
        };

        return item;
    }

    private Texture2D createTextureLowShadow(Color c)
    {
        Image img = Image.CreateEmpty(1, 10, false, Image.Format.Rgba8);

        for(int i = 0; i < 10; ++i)
            img.SetPixel(0, i, c * (1.0f - i / 20.0f));
        return ImageTexture.CreateFromImage(img);
    }

    private Texture2D createTextureCenterGlow(Color c)
    {
        Image img = Image.CreateEmpty(10, 1, false, Image.Format.Rgba8);

        for(int i = 0; i < 10; ++i)
        {
            float centerDist = Mathf.Abs(4.5f - i); // [0-5]
            float strength = centerDist / 8.0f; // [1-0.5]
            img.SetPixel(i, 0, c * (1.0f - strength * strength));
        }
        return ImageTexture.CreateFromImage(img);
    }

    private Color SampleColor(int step)
    {
        return colors.Colors[step];
    }
}

public class SceneryRectangle
{
    public Vector2 center; // as screen ratio
    public Vector2 size; // as screen ratio
    public Sprite2D sprite;
}
