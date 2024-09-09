using Godot;
using System;

public partial class Game : Control
{
    private const double FadeDuration = 1.0f;

    [Export]
    private PackedScene[] sceneLevels;

    [Export]
    private PackedScene sceneCardDrafting;

    int currentLevelIdx = 0;
    private Level currentLevelNode;
    private CardDrafting cardDraftingNode;

    private ColorRect fader;

    public override void _Ready()
    {
        fader = GetNode<ColorRect>("FadeToBlack");

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
        currentLevelNode = sceneLevels[currentLevelIdx].Instantiate<Level>();
        currentLevelNode.Spellbook = cardDraftingNode.Spellbook;
        currentLevelNode.OnLevelFinish += HandleStartDraft;

        FadeTransition(() =>
        {
            cardDraftingNode.Visible = false;
            AddChild(currentLevelNode);
        });
    }

    public void HandleStartDraft()
    {
        cardDraftingNode.Spellbook = currentLevelNode.Spellbook;
        cardDraftingNode.CurrentMana = currentLevelNode.CurrentMana;
        currentLevelIdx++;

        FadeTransition(() =>
        {
            cardDraftingNode.Visible = true;
            currentLevelNode.QueueFree();
        });
    }
}
