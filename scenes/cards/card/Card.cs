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


    public void SetCardEffect(ICardEffect aEffect)
    {
        ManaCost = aEffect.GetManaCost();
        if (ManaCost > 0)
        {
            Effect = aEffect;
            cardTitle.Text = Effect.GetCardTitle();
            cardText.Text = Effect.GetCardText();
        }
        else
        {
            cardTitle.Text = "Failure";
            cardText.Text = "This card has no effect";
            // ManaCost = 0;
        }
        cardManaCost.Text = ManaCost.ToString();
    }
}

