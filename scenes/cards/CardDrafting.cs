using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardDrafting : Node2D
{
    private const int CardPositionOffsetPx = 20;
    
    private const int NumCardsToDeal = 8;
    private const int NumRowsOfCards = 2;
    private const int NumColumnsOfCards = NumCardsToDeal / NumRowsOfCards;

    private const float FractionOfManaUseFuzzyness = 1.0f / NumCardsToDeal;

    private const float SpeedAdditiveMax = 10;
    private const float SpeedAdditiveCost = 1;
    private const float SpeedMultiplicativeMax = 1;
    private const float SpeedMultiplicativeCost = 10;
    private const float DamageAdditiveMax = 5;
    private const float DamageAdditiveCost = 10;
    private const float DamageMultiplicativeMax = 1;
    private const float DamageMultiplicativeCost = 20;
    private const float HomingDegPerSecondMax = 50;
    private const float HomingDegPerSecondCost = 5;


    public int CurrentMana = 100;

    private Random rng = new();

    [Export]
    private PackedScene cardScene;

    [Export]
    private Label manaDisplay;

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

        manaDisplay.Text = CurrentMana.ToString();
    }

    public void Deal()
    {
        foreach(Node child in cardNode.GetChildren()) child.QueueFree();

        for (int currentCardIdx = 0; currentCardIdx < NumCardsToDeal; currentCardIdx++)
        {
            if (CurrentMana == 0) break;

            float baseFractionToUse = 1.0f / (NumCardsToDeal - currentCardIdx);
            float randomOffset = GetRandomOffset(FractionOfManaUseFuzzyness);
            float rawFractionToUse = baseFractionToUse + randomOffset;
            int manaToUse = (int)(CurrentMana * (float)Math.Clamp(rawFractionToUse, 0, 1));
            if (manaToUse == 0) manaToUse = CurrentMana;

            ICardEffect effectSource = Select(manaToUse);
            CurrentMana -= manaToUse;

            Card card = cardScene.Instantiate<Card>();
            card.SetCardEffect(effectSource, manaToUse);
            card.Position = GetCardPosition(currentCardIdx);
            card.Position += new Vector2(rng.NextSingle(), rng.NextSingle()) * CardPositionOffsetPx;
            cardNode.AddChild(card);
        }
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


    private float GetRandomOffset(float magnitude)
    {
        return (rng.NextSingle() * magnitude * 2.0f) - magnitude;
    }

    public ICardEffect Select(int manaToUse)
    {
        BoltStatEffect effect = new();

        int scaleMultpilier = 1 + (manaToUse / 20);

        int[] table = {
            5,  // 0: OnlyOnPlayerFire
            5,  // 1: SpeedAdditive
            30, // 2: SpeedMultiplicative
            30, // 3: DamageAdditive
            10, // 4: DamageMultiplicative
            10, // 5: HomingDegPerSecond
            1,  // 6: Anti SpeedMultiplicative
            1,  // 7: Anti DamageMultiplicative
        };

        int manaLeftToUse = manaToUse;
        while (manaLeftToUse > 0)
        {
            int effectIndex = -1;
            int number = rng.Next() % table.Sum();
            while (number >= 0) number -= table[++effectIndex];

            float offsetModifier = scaleMultpilier * ((rng.Next() % 8) + 3) / 10.0f;

            switch (effectIndex)
            {
                case 0:
                    if (!effect.OnlyOnPlayerFire)
                    {
                        manaLeftToUse += manaToUse; // double the total mana worth
                        effect.OnlyOnPlayerFire = true;
                    }
                    break;
                case 1:
                    manaLeftToUse -= (int) (offsetModifier * SpeedAdditiveCost);
                    if (manaLeftToUse > 0) effect.SpeedAdditive += offsetModifier * SpeedAdditiveMax;
                    break;
                case 2:
                    manaLeftToUse -= (int) (offsetModifier * SpeedMultiplicativeCost);
                    if (manaLeftToUse > 0) effect.SpeedMultiplicative += offsetModifier * SpeedMultiplicativeMax;
                    break;
                case 3:
                    manaLeftToUse -= (int) (offsetModifier * DamageAdditiveCost);
                    if (manaLeftToUse > 0) effect.DamageAdditive += (int)(offsetModifier * DamageAdditiveMax);
                    break;
                case 4:
                    manaLeftToUse -= (int) (offsetModifier * DamageMultiplicativeCost);
                    if (manaLeftToUse > 0) effect.DamageMultiplicative += offsetModifier * DamageMultiplicativeMax;
                    break;
                case 5:
                    manaLeftToUse -= (int) (offsetModifier * HomingDegPerSecondCost);
                    if (manaLeftToUse > 0) effect.HomingDegPerSecond += offsetModifier * HomingDegPerSecondMax;
                    break;
                case 6:
                    if (effect.SpeedMultiplicative == 0)
                    {
                        manaLeftToUse += (int) (offsetModifier * 2 * SpeedMultiplicativeCost);
                        effect.SpeedMultiplicative -= offsetModifier * (SpeedMultiplicativeMax / 2);
                        table[2] = 0;
                    }
                    break;
                case 7:
                    if (effect.DamageMultiplicative == 0)
                    {
                        manaLeftToUse += (int) (offsetModifier * 2 * DamageMultiplicativeCost);
                        effect.DamageMultiplicative -= offsetModifier * (DamageMultiplicativeMax / 2);
                        table[4] = 0;
                    }
                    break;
                default:
                    throw new Exception("Table index out of bounds");
            }
        }

        return effect;
    }
}
