using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardDrafting : Node2D
{
    private const int CardPositionRandomOffsetPx = 5;
    private const float CardRotationRandomRangeRad = 0;
    private const float CardRotationRandomOffsetRad = -CardRotationRandomRangeRad / 2;
    private const double CardDealAnimationDurationSec = 0.5;
    private const double CardDealAnimationOverlap = 4.0;

    private const int NumCardsToDeal = 8;
    private const int NumRowsOfCards = 2;
    private const int NumColumnsOfCards = NumCardsToDeal / NumRowsOfCards;

    private const float FractionOfManaUseFuzzyness = 1.0f / NumCardsToDeal;
    private const float FractionOfManaUseBias = 0.1f / NumCardsToDeal;

    private const double ShowManaCatchUpRate = 100.0;
    private double ShownManaReal = 0;
    private int ShownMana = 0;
    public int CurrentMana = 100;

    private Random rng = new();

    [Export]
    private PackedScene cardScene;

    [Export]
    private Label manaDisplay;

    [Export]
    private Node2D dealerSource;
    [Export]
    private Node2D dealerSink;

    private Node cardNode;

    private Vector2 cardPosition00;
    private Vector2 cardPosition10;

    private Vector2 cardPositionZeroOffset;
    private Vector2 cardPositionOneOffset;

    public override void _Ready()
    {
        Card card = cardScene.Instantiate<Card>();
        ReferenceRect referenceRect = card.GetNode<ReferenceRect>("CardOutline");

        cardNode = GetNode<Node>("Cards");
        cardPosition00 = GetNode<Node2D>("Cards/CardPosition00").Position;
        Vector2 cardPosition01 = GetNode<Node2D>("Cards/CardPosition01").Position;
        cardPosition10 = GetNode<Node2D>("Cards/CardPosition10").Position;
        Vector2 cardPosition11 = GetNode<Node2D>("Cards/CardPosition11").Position;

        cardPosition01.X -= referenceRect.Size.X;
        cardPosition10.Y -= referenceRect.Size.Y;
        cardPosition11 -= referenceRect.Size;

        cardPositionZeroOffset = cardPosition01 - cardPosition00;
        cardPositionOneOffset = cardPosition11 - cardPosition10;

        dealerSource.Position -= referenceRect.Size / 2;
        dealerSink.Position -= referenceRect.Size / 2;

        manaDisplay.Text = ShownMana.ToString();
    }

    public override void _Process(double delta)
    {
        // we juggle with ShownManaReal to avoid situations 
        // where (ShowManaCatchUpRate * delta < 1)
        if (ShownMana != CurrentMana)
        {
            if (ShownManaReal < CurrentMana)
            {
                ShownManaReal += ShowManaCatchUpRate * delta;
                if (ShownManaReal > CurrentMana)
                {
                    ShownManaReal = CurrentMana;
                }
            }
            else if (ShownManaReal > CurrentMana)
            {
                ShownManaReal -= ShowManaCatchUpRate * delta;
                if (ShownManaReal < CurrentMana)
                {
                    ShownManaReal = CurrentMana;
                }
            }

            ShownMana = (int)Math.Round(ShownManaReal);
            manaDisplay.Text = ShownMana.ToString();
        }
    }

    public void Deal()
    {
        RemoveCards();

        for (int currentCardIdx = 0; currentCardIdx < NumCardsToDeal; currentCardIdx++)
        {
            if (CurrentMana == 0) break;

            float baseFractionToUse = 1.0f / (NumCardsToDeal - currentCardIdx);
            float rawFractionToUse = baseFractionToUse + GetRandomOffset(FractionOfManaUseFuzzyness) + FractionOfManaUseBias;
            int manaToUse = (int)(CurrentMana * (float)Math.Clamp(rawFractionToUse, 0, 1));
            if (manaToUse == 0) manaToUse = CurrentMana;

            ICardEffect effectSource = Select(manaToUse);
            CurrentMana -= effectSource.GetManaCost();
            SpawnCard(currentCardIdx, effectSource);
        }
    }

    public void RemoveCards()
    {
        int currentCardIdx = 0;
        foreach (Node child in cardNode.GetChildren())
        {
            if (child is Card card)
            {
                RemoveCard(card, currentCardIdx);
                currentCardIdx++;
            }
        };
    }

    private void RemoveCard(Card card, int delayIndex = 0)
    {
        double duration = CardDealAnimationDurationSec * (CardDealAnimationOverlap / NumCardsToDeal);
        double startTime = (CardDealAnimationDurationSec - duration) * ((1.0 + delayIndex) / NumCardsToDeal);

        CurrentMana += card.ManaCost;

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", dealerSink.Position, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetDelay(startTime);

        tween.TweenCallback(Callable.From(card.QueueFree));
    }

    private void CheckFailureCard(Card card)
    {
        if (card.ManaCost == 0) RemoveCard(card);
    }

    private void SpawnCard(int currentCardIdx, ICardEffect effectSource)
    {
        Card card = cardScene.Instantiate<Card>();
        card.SetCardEffect(effectSource);
        card.Position = dealerSource.Position;
        card.Rotation = CardRotationRandomOffsetRad + rng.NextSingle() * CardRotationRandomRangeRad;

        Vector2 targetPosition = GetCardPosition(currentCardIdx);
        targetPosition += new Vector2(rng.NextSingle(), rng.NextSingle()) * CardPositionRandomOffsetPx;

        double duration = CardDealAnimationDurationSec * (CardDealAnimationOverlap / NumCardsToDeal);
        double startTime = (CardDealAnimationDurationSec - duration) * ((1.0 + currentCardIdx) / NumCardsToDeal);

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", targetPosition, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetDelay(startTime);

        tween.TweenCallback(Callable.From(() => CheckFailureCard(card)))
            .SetDelay(1.0f);

        cardNode.AddChild(card);
    }

    private Vector2 GetCardPosition(int aCurrentCardIdx)
    {
        int row = aCurrentCardIdx % NumRowsOfCards;
        int column = aCurrentCardIdx / NumRowsOfCards;

        float rowFraction = row / (NumRowsOfCards - 1.0f);
        float columnFraction = column / (NumColumnsOfCards - 1.0f);

        Vector2 zeroXOffset = cardPosition00 + cardPositionZeroOffset * columnFraction;
        Vector2 oneXOffset = cardPosition10 + cardPositionOneOffset * columnFraction;

        Vector2 vector2 = zeroXOffset + (oneXOffset - zeroXOffset) * rowFraction;

        return vector2;
    }


    private float GetRandomOffset(float magnitude) => (rng.NextSingle() * magnitude * 2.0f) - magnitude;

    public ICardEffect Select(int manaToUse)
    {
        return BoltStatEffect.CreateWithCost(manaToUse);
    }
}
