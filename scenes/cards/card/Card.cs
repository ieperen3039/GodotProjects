using Godot;
using System;

public partial class Card : Node2D
{
    [Signal]
    public delegate void OnCardGetsChosenEventHandler(Card aCard);

    public ICardEffect Effect { get; private set; }

    public int ManaCost { get; private set; }

    [Export]
    private Label cardTitle;
    [Export]
    private Label cardManaCost;
    [Export]
    private RichTextLabel cardText;
    [Export]
    private BaseButton clickListener;

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
            clickListener.Disabled = true;
        }
        cardManaCost.Text = ManaCost.ToString();
    }
    
    private void OnClick()
    {
        EmitSignal(SignalName.OnCardGetsChosen, this);
    }
}
