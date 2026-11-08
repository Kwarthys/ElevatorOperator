using Godot;
using System;

public partial class ClockDrawer : Node2D
{
    [Export] private Color m_backgroundColor = new(1, 1, 1);
    [Export] private Color m_edgeColor = new(0, 0, 0);
    [Export] private Color m_ticksColor = new(0, 0, 0);
    [Export] private Color m_backgroundNightColor = new(1, 1, 1);
    [Export] private Color m_edgeNightColor = new(0, 0, 0);
    [Export] private Color m_ticksNightColor = new(0, 0, 0);
    [Export] private Vector2I m_textureResolution = new(100, 100);
    [Export] private ColorRect m_needle;

    private TextureRect m_clockdDisplay;

    private Texture2D m_dayTexture;
    private Texture2D m_nightTexture;
    private bool m_isDarkMode = true;

    public override void _Process(double delta)
    {
        float minutes = GameClockManager.clock.hours * 60.0f + GameClockManager.clock.minutes;
        float angleDegrees = 180.0f + minutes / (12.0f * 60.0f) * 360.0f;
        m_needle.RotationDegrees = angleDegrees % 360.0f;

        if(GameClockManager.clock.hours >= 8 && GameClockManager.clock.hours < 22)
        {
            // Light Mode
            if(m_isDarkMode)
            {
                m_clockdDisplay.Texture = m_dayTexture;
                m_needle.Color = m_ticksColor;
                m_isDarkMode = false;
            }
        }
        else
        {
            if(m_isDarkMode == false)
            {
                m_clockdDisplay.Texture = m_nightTexture;
                m_needle.Color = m_ticksNightColor;
                m_isDarkMode = true;
            }
        }
    }

    public override void _Ready()
    {
        m_clockdDisplay = new();
        AddChild(m_clockdDisplay);

        m_needle = new();
        m_needle.Color = m_ticksNightColor;
        AddChild(m_needle);

        BuildTextures();
        m_clockdDisplay.Texture = m_nightTexture;

        GetViewport().SizeChanged += OnScreenResize;
        OnScreenResize();
    }

    private void BuildTextures()
    {
        m_dayTexture = BuildClockTexture(m_edgeColor, m_ticksColor, m_backgroundColor);
        m_nightTexture = BuildClockTexture(m_edgeNightColor, m_ticksNightColor, m_backgroundNightColor);
    }

    private Texture2D BuildClockTexture(Color edge, Color ticks, Color back)
    {
        Image image = Image.CreateEmpty(m_textureResolution.X, m_textureResolution.Y, false, Image.Format.Rgba8);

        float clockRadius = Mathf.Min(m_textureResolution.X, m_textureResolution.Y) * 0.5f;
        float clockInnerRadius = clockRadius * 0.9f;
        Vector2 clockCenter = new(clockRadius, clockRadius);

        Color paintColor;
        for(int y = 0; y < m_textureResolution.Y; ++y)
        {
            for(int x = 0; x < m_textureResolution.X; ++x)
            {
                float distToCenter = (new Vector2(x, y) - clockCenter).Length();

                if(distToCenter > clockRadius)
                    paintColor = new(0, 0, 0, 0);
                else if(distToCenter > clockInnerRadius || distToCenter < 7.0)
                    paintColor = edge;
                else
                    paintColor = back;

                image.SetPixel(x, y, paintColor);
            }
        }

        // Add Tickmarks
        const int TICK_NUMBER = 12;
        float tickCenterRadius = clockInnerRadius * 0.8f;
        float angleOffset = Mathf.Tau / TICK_NUMBER;
        for(int i = 0; i < TICK_NUMBER + 1; ++i)
        {
            float angle = angleOffset * i;
            Vector2 tickCenterFloat = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * tickCenterRadius + clockCenter;
            Vector2I tickCenter = new(Mathf.RoundToInt(tickCenterFloat.X), Mathf.RoundToInt(tickCenterFloat.Y));

            int tickRadius = i % 3 == 0 ? 10 : 4;
            DrawCircle(ref image, tickCenter, tickRadius, ticks);
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static void DrawCircle(ref Image image, Vector2I pos, int radius, Color color)
    {
        for(int y = -radius - 1; y < radius + 1; ++y)
        {
            for(int x = -radius - 1; x < radius + 1; ++x)
            {
                Vector2I pixelPos = pos - new Vector2I(x, y);

                if(pixelPos.DistanceTo(pos) < radius)
                    image.SetPixelv(pixelPos, color);
            }
        }
    }

    private void OnScreenResize()
    {
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;

        float clockWantedSize = Mathf.Min(screenSize.X, screenSize.Y) * 0.15f;
        float clockScaler = clockWantedSize / Mathf.Max(m_textureResolution.X, m_textureResolution.Y);

        m_clockdDisplay.SetScale(new(clockScaler, clockScaler));
        m_clockdDisplay.Position = new Vector2(screenSize.X, 0.0f) + new Vector2(-m_textureResolution.X * clockScaler - 10.0f, 10.0f);

        m_needle.SetSize(new(7.0f * clockScaler, m_textureResolution.Y * 0.4f * clockScaler));
        m_needle.PivotOffset = new(m_needle.Size.X * 0.5f, m_needle.Size.Y * 0.3f);
        m_needle.Position = m_clockdDisplay.Position + m_clockdDisplay.Scale * m_clockdDisplay.Texture.GetSize() * 0.5f - m_needle.PivotOffset;
    }
}
