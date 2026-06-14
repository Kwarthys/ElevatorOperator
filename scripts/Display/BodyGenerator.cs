using Godot;

public class BodyGenerator
{
	public Texture2D Generate(Vector2I size, int outlineSize, Vector2I circleRadii)
	{
		Image img = Image.CreateEmpty(size.X, size.Y, false, Image.Format.Rgba8);

		int circleARadius = GetRandomRadius(circleRadii);
		int circleBRadius = GetRandomRadius(circleRadii);

		Vector2I circleCenterA = new(size.X / 2, circleARadius);
		Vector2I circleCenterB = new(size.X / 2, size.Y - circleBRadius);

		float interpStyle = GD.Randf();

		Color c = new(GD.Randf(), GD.Randf(), GD.Randf());
		for(int y = 0; y < size.Y; ++y)
		{
			for(int x = 0; x < size.X; ++x)
			{
				Vector2I vPos = new(x, y);
				Vector2I center;
				int radius;

				if(y >= circleCenterA.Y && y <= circleCenterB.Y)
				{
					// Interpolate radius
					center = new(size.X / 2, y);
					float ratio = (y - circleCenterA.Y) * 1.0f / (circleCenterB.Y - circleCenterA.Y);

					if(interpStyle < 0.33f)
						ratio *= ratio;
					else if(interpStyle > 0.66f)
						ratio = Mathf.Sqrt(ratio);
					// else stay linear

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

		return ImageTexture.CreateFromImage(img);
	}

	private int GetRandomRadius(Vector2I circleRadii)
	{
		return Mathf.RoundToInt(Mathf.Lerp(circleRadii.X, circleRadii.Y, GD.Randf()));
	}
}
