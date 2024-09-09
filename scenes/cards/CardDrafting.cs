using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardDrafting : Node2D
{
    [Signal]
    public delegate void OnNextLevelEventHandler();

    private const int CardPositionRandomOffsetPx = 5;
    private const float CardRotationRandomRangeRad = 0;
    private const float CardRotationRandomOffsetRad = -CardRotationRandomRangeRad / 2;
    private const double CardDealAnimationDurationSec = 0.5;
    private const double CardDealAnimationOverlap = 4.0;
    private const double CardChooseAnimationDurationSec = 0.5;

    private const int NumCardsToDeal = 8;
    private const int NumRowsOfCards = 2;
    private const int NumColumnsOfCards = NumCardsToDeal / NumRowsOfCards;

    private const float FractionOfManaUseFuzzyness = 1.0f / NumCardsToDeal;
    private const float FractionOfManaUseBias = 0.1f / NumCardsToDeal;

    private const double ShowManaCatchUpRate = 100.0;

    public int CurrentMana = 100;
    private double shownManaReal = 0;
    private int shownMana = 0;

    public SpellBook Spellbook = new();

    [Export]
    private PackedScene cardScene;

    [Export]
    private Label manaDisplay;

    [Export]
    private Node2D dealerSource;
    [Export]
    private Node2D dealerSink;
    [Export]
    private Node2D spellbookNode;

    private Random rng = new();

    private Node dealtCardsNode;

    private Vector2 cardPosition00;
    private Vector2 cardPosition10;

    private Vector2 cardPositionZeroOffset;
    private Vector2 cardPositionOneOffset;

    public override void _Ready()
    {
        Card card = cardScene.Instantiate<Card>();
        Control outline = card.GetNode<Control>("Background");

        dealtCardsNode = GetNode<Node>("Cards");
        cardPosition00 = GetNode<Node2D>("CardPosition00").Position;
        Vector2 cardPosition01 = GetNode<Node2D>("CardPosition01").Position;
        cardPosition10 = GetNode<Node2D>("CardPosition10").Position;
        Vector2 cardPosition11 = GetNode<Node2D>("CardPosition11").Position;

        cardPosition01.X -= outline.Size.X;
        cardPosition10.Y -= outline.Size.Y;
        cardPosition11 -= outline.Size;

        cardPositionZeroOffset = cardPosition01 - cardPosition00;
        cardPositionOneOffset = cardPosition11 - cardPosition10;

        dealerSource.Position -= outline.Size / 2;
        dealerSink.Position -= outline.Size / 2;

        manaDisplay.Text = shownMana.ToString();
    }

    public override void _Process(double aDelta)
    {
        // we juggle with ShownManaReal to avoid situations 
        // where (ShowManaCatchUpRate * delta < 1)
        if (shownMana != CurrentMana)
        {
            if (shownManaReal < CurrentMana)
            {
                shownManaReal += ShowManaCatchUpRate * aDelta;
                if (shownManaReal > CurrentMana)
                {
                    shownManaReal = CurrentMana;
                }
            }
            else if (shownManaReal > CurrentMana)
            {
                shownManaReal -= ShowManaCatchUpRate * aDelta;
                if (shownManaReal < CurrentMana)
                {
                    shownManaReal = CurrentMana;
                }
            }

            shownMana = (int)Math.Round(shownManaReal);
            manaDisplay.Text = shownMana.ToString();
        }
    }

    public void Deal()
    {
        DiscardAll();

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

    private void SpawnCard(int aCurrentCardIdx, ICardEffect aEffectSource)
    {
        Card card = cardScene.Instantiate<Card>();
        card.SetCardEffect(aEffectSource);
        card.Position = dealerSource.Position;
        card.Rotation = CardRotationRandomOffsetRad + rng.NextSingle() * CardRotationRandomRangeRad;
        card.OnCardGetsChosen += ChooseCard;

        Vector2 targetPosition = GetCardPosition(aCurrentCardIdx);
        targetPosition += new Vector2(rng.NextSingle(), rng.NextSingle()) * CardPositionRandomOffsetPx;

        double duration = CardDealAnimationDurationSec * (CardDealAnimationOverlap / NumCardsToDeal);
        double startTime = (CardDealAnimationDurationSec - duration) * ((1.0 + aCurrentCardIdx) / NumCardsToDeal);

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", targetPosition, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetDelay(startTime);

        tween.TweenCallback(Callable.From(() => CheckFailureCard(card)))
            .SetDelay(1.0f);

        dealtCardsNode.AddChild(card);
    }

    private void ChooseCard(Card aCard)
    {
        Spellbook.Add(aCard);
        
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(aCard, "position", spellbookNode.Position, CardChooseAnimationDurationSec)
            .SetTrans(Tween.TransitionType.Cubic);
    }

    private void CheckFailureCard(Card aCard)
    {
        // maybe this is already disposed
        if (!IsInstanceValid(aCard)) return;

        // no need for additional delay
        if (aCard.ManaCost == 0) DiscardCard(aCard);
    }

    public void DiscardAll()
    {
        int currentCardIdx = 0;
        foreach (Node child in dealtCardsNode.GetChildren())
        {
            if (child is Card card)
            {
                DiscardCard(card, currentCardIdx);
                currentCardIdx++;
            }
        };
    }

    private void DiscardCard(Card aCard, int aDelayIndex = 0)
    {
        double duration = CardDealAnimationDurationSec * (CardDealAnimationOverlap / NumCardsToDeal);
        double startTime = (CardDealAnimationDurationSec - duration) * ((1.0 + aDelayIndex) / NumCardsToDeal);

        CurrentMana += aCard.ManaCost;

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(aCard, "position", dealerSink.Position, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetDelay(startTime);

        tween.TweenCallback(Callable.From(aCard.QueueFree));
    }

    private void StartNextLevel()
    {
        DiscardAll();
        
        Tween tween = GetTree().CreateTween();
        tween.TweenCallback(Callable.From(() => EmitSignal(SignalName.OnNextLevel)))
            .SetDelay(CardDealAnimationDurationSec);
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
