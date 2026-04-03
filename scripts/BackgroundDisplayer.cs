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
    [Export] private Node2D backgroundTextsHolder;

    private List<SceneryItem> items = new();
    private SceneryNode2D selectionDisplayer;

    public override void _Ready()
    {
        items.Add(CreateItem(backgroundColor, new(0.5f, 0.5f), new(1.0f, 1.0f)));

        for(int i = 0; i < 6; ++i)
        {
            float xPos = (i + 1) / 7.0f;
            items.Add(CreateItem(SampleColor(i), new(0.5f, xPos), new(1.0f, 0.01f)));
            items.Add(CreateFloorDisplay(5 - i, backgroundColor, new(0.05f, xPos), new(-1.0f, -1.0f)));
        }

        selectionDisplayer = CreateItem(selectionColor, new(1.0f / 4.0f, 0.5f), new(0.01f, 1.0f), true);
        items.Add(selectionDisplayer);

        UpdateScenery();
    }

    public void UpdateScenery()
    {
        items.ForEach((i) => i.Update());
    }

    public void MoveSelection(float x)
    {
        selectionDisplayer.center.X = x;
        selectionDisplayer.Update();
    }

    private SceneryNode2D CreateItem(Color c, Vector2 center, Vector2 size, bool centerGlow = false) // Last item added is on top
    {
        Sprite2D sprite = new();
        if(centerGlow)
            sprite.Texture = createTextureCenterGlow(c);
        else
            sprite.Texture = createTextureLowShadow(c);
        backgroundHolder.AddChild(sprite);

        SceneryNode2D item = new()
        {
            center = center,
            size = size,
            node = sprite
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

    private Color SampleColor(int step) { return colors.Colors[step]; }

    private SceneryControl CreateFloorDisplay(int floor, Color c, Vector2 center, Vector2 size)
    {
        RichTextLabel text = new()
        {
            FitContent = false,
            ScrollActive = false,
            AutowrapMode = TextServer.AutowrapMode.Off,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = floor.ToString(),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Size = new(100.0f, 100.0f)
        };

        text.AddThemeFontSizeOverride("normal_font_size", 50);
        text.AddThemeColorOverride("default_color", c);
        text.AddThemeConstantOverride("outline_size", 4);
        text.AddThemeColorOverride("font_outline_color", new(1, 1, 1, 0.8f));

        backgroundHolder.AddChild(text);

        return new()
        {
            control = text,
            center = center,
            size = size
        };
    }
}

public abstract class SceneryItem
{
    public Vector2 center; // as screen ratio
    public Vector2 size; // as screen ratio
    public abstract void Update();
}

public class SceneryNode2D : SceneryItem
{
    public Node2D node;
    public override void Update()
    {
        node.Scale = DisplayUtils.screenSize * size;
        node.Position = DisplayUtils.screenSize * center;
    }
}

public class SceneryControl : SceneryItem
{
    public Control control;
    public override void Update()
    {
        if(size.X >= 0.0f && size.Y >= 0.0f)
            control.Scale = DisplayUtils.screenSize * size;
        control.Position = DisplayUtils.screenSize * center - control.Size * 0.5f;
    }
}
