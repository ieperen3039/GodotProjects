using Godot;
using System;

public partial class Healthbar : Node2D
{
	private Polygon2D fill;
	public bool Fading = false;

	public override void _Ready()
	{
		fill = GetNode<Polygon2D>("GreenBar");
	}

	public override void _Process(double aDelta)
	{
		if (Fading)
		{
			float newTransparency = Modulate.A - (float)aDelta;
			if (newTransparency > 0)
			{
				Modulate = new Color(Modulate, newTransparency);
			}
			else
			{
				Visible = false;
				Fading = false;
			}
		}
	}

	public void SetHealth(float aFraction)
	{
		fill.Scale = new Vector2(aFraction, 1);
	}
}
