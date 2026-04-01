using Godot;
using System;
using System.Collections.Generic;

public partial class BackgroundDisplayer : Node
{
    [Export] private Color color;
    [Export] private Color color2;
    [Export] private Node2D backgroundHolder;

    private List<SceneryRectangle> items = new();

    public override void _Ready()
    {

        items.Add(CreateItem(color, new(0.5f, 0.5f), new(0.1f, 1.0f)));
        items.Add(CreateItem(color2, new(0.5f, 0.5f), new(1.0f, 0.1f)));

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

    private SceneryRectangle CreateItem(Color c, Vector2 center, Vector2 size) // Last item added is on top
    {
        Sprite2D sprite = new();
        Image img = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        img.SetPixel(0, 0, c);
        sprite.Texture = ImageTexture.CreateFromImage(img);

        backgroundHolder.AddChild(sprite);

        SceneryRectangle item = new()
        {
            center = center,
            size = size,
            sprite = sprite
        };

        return item;
    }
}

public class SceneryRectangle
{
    public Vector2 center; // as screen ratio
    public Vector2 size; // as screen ratio
    public Sprite2D sprite;
}
