using Godot;
using System;

public partial class Card : Node2D
{
    public ICardEffect Effect { get; private set; }

    public int ManaCost { get; private set; }

    [Export]
    private Label cardTitle;
    [Export]
    private Label cardManaCost;
    [Export]
    private RichTextLabel cardText;


    public void SetCardEffect(ICardEffect aEffect, int aManaCost)
    {
        Effect = aEffect;
        cardTitle.Text = Effect.GetCardTitle();
        cardText.Text = Effect.GetCardText();
        ManaCost = aManaCost;
        cardManaCost.Text = aManaCost.ToString();
    }
}

