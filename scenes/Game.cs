using Godot;
using System;

public partial class Game : Control
{
    private const double FadeDuration = 1.0f;

    [Export]
    private PackedScene[] levelScenes;

    private PackedScene sceneCardDrafting;

    int currentLevelIdx = 0;
    private Level currentLevelNode;
    private CardDrafting cardDraftingNode;

    private ColorRect fader;

    public override void _Ready()
    {
        fader = GetNode<ColorRect>("FadeToBlack");

        sceneCardDrafting = ResourceLoader.Load<PackedScene>("res://scenes/cards/card_draft.tscn");
        cardDraftingNode = sceneCardDrafting.Instantiate<CardDrafting>();
        cardDraftingNode.OnNextLevel += HandleNextLevel;
        AddChild(cardDraftingNode);
    }

    public void FadeTransition(Action action)
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(fader, "modulate:a", 1.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);

        tween.TweenCallback(Callable.From(action));
        
        tween.TweenProperty(fader, "modulate:a", 0.0, FadeDuration)
            .SetTrans(Tween.TransitionType.Cubic);
    }

    public void HandleNextLevel()
    {
        FadeTransition(() =>
        {
            currentLevelNode = levelScenes[currentLevelIdx].Instantiate<Level>();
            Deck.TransitionToLevel(cardDraftingNode, currentLevelNode);
            currentLevelNode.OnLevelFinish += HandleStartDraft;

            cardDraftingNode.Visible = false;
            AddChild(currentLevelNode);
        });
    }

    public void HandleStartDraft()
    {
        FadeTransition(() =>
        {
            Deck.TransitionToDraft(currentLevelNode, cardDraftingNode);
            cardDraftingNode.CurrentMana += currentLevelNode.EarnedMana;
            currentLevelIdx++;

            // repeat last level if we run out of levels
            if (currentLevelIdx == levelScenes.Length) currentLevelIdx--;

            cardDraftingNode.Visible = true;
            currentLevelNode.QueueFree();
        });
    }
}
