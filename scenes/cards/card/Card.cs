using Godot;
using System;

public partial class Card : Node2D
{
	public BoltStatEffect Effect { get; private set; }

	[Export]
	private Label cardTitle;
	[Export]
	private RichTextLabel cardText;

	public override void _Ready()
	{
		cardTitle.Text = Effect.GetCardTitle();
		cardText.Text = Effect.GetCardText();
	}
}
 
