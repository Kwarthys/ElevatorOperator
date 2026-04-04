using Godot;

public partial class BodyGenerator : Node2D
{
	[Export] private Vector2I textureSize;
	[Export] private Vector2I circleRadii = new Vector2I(10, 50);
	[Export] private int outlineSize = 5;
	private Sprite2D sprite;
	private float dtAccumulator = 0.0f;
	[Export] private float animSpeed = 1.0f;
	[Export] private float animAmplitude = 0.2f;

	public override void _Ready()
	{
		sprite = new();
		AddChild(sprite);
		sprite.Position = DisplayUtils.screenSize * 0.5f;

		Generate();
	}

	public override void _Process(double dt)
	{
		if(Input.IsActionJustPressed("Left"))
			Generate();

		dtAccumulator += (float)dt;
		sprite.Skew = animAmplitude * Mathf.Sin(dtAccumulator * animSpeed);
	}

	public void Generate()
	{
		Image img = Image.CreateEmpty(textureSize.X, textureSize.Y, false, Image.Format.Rgba8);

		int circleARadius = GetRandomRadius();
		int circleBRadius = GetRandomRadius();

		Vector2I circleCenterA = new(textureSize.X / 2, circleARadius);
		Vector2I circleCenterB = new(textureSize.X / 2, textureSize.Y - circleBRadius);

		bool interpSide = GD.Randf() > 0.5f;

		Color c = new(GD.Randf(), GD.Randf(), GD.Randf());
		for(int y = 0; y < textureSize.Y; ++y)
		{
			for(int x = 0; x < textureSize.X; ++x)
			{
				Vector2I vPos = new(x, y);
				Vector2I center;
				int radius;

				if(y >= circleCenterA.Y && y <= circleCenterB.Y)
				{
					// Interpolate radius
					center = new(textureSize.X / 2, y);
					float ratio = (y - circleCenterA.Y) * 1.0f / (circleCenterB.Y - circleCenterA.Y);

					if(interpSide)
						ratio *= ratio;
					else
						ratio = Mathf.Sqrt(ratio);

					radius = Mathf.RoundToInt(Mathf.Lerp(circleARadius, circleBRadius, ratio));
				}
				else
				{
					if(y < circleCenterA.Y)
					{
						center = circleCenterA;
						radius = circleARadius;
					}
					else
					{
						center = circleCenterB;
						radius = circleBRadius;
					}
				}

				Vector2 dist2ToCenterA = vPos - center;
				if(dist2ToCenterA.LengthSquared() < radius * radius)
				{
					if(dist2ToCenterA.LengthSquared() < (radius - outlineSize) * (radius - outlineSize))
						img.SetPixelv(vPos, c);
					else
						img.SetPixelv(vPos, new(1, 1, 1, 1));
				}
				else
				{
					img.SetPixelv(vPos, new(0, 0, 0, 0));
				}
			}
		}

		sprite.Texture = ImageTexture.CreateFromImage(img);
		sprite.Scale = Vector2.One * (1.0f - GD.Randf() * 0.5f);
	}

	private int GetRandomRadius()
	{
		return Mathf.RoundToInt(Mathf.Lerp(circleRadii.X, circleRadii.Y, GD.Randf()));
	}
}
