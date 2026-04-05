using Godot;
using System;

public partial class UserSprite : Sprite2D
{
	private float dtAccumulator = 0.0f;
	private float animSpeed = 1.0f;
	private float animAmplitude = 0.2f;

	private float baseSize = GD.Randf() * 0.5f + 0.5f;
	private float texYSize;
	private float phaseOffset = GD.Randf() * Mathf.Tau;

	public UserSprite(Texture2D tex)
	{
		Texture = tex;
		texYSize = tex.GetSize().Y;
	}

	public override void _Ready()
	{
		OnScreenResize();
	}


	public void Update(double dt)
	{
		dtAccumulator += (float)dt;
		Skew = animAmplitude * Mathf.Sin(phaseOffset + dtAccumulator * animSpeed);
	}

	public void OnScreenResize()
	{
		float scaleRatio = 0.1f * DisplayUtils.screenSize.Y * baseSize / texYSize;
		Scale = new(scaleRatio, scaleRatio);
	}
}
