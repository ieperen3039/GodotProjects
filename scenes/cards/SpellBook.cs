
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SpellBook : Node
{
    private List<Card> cards = new();

    public static void TransitionToLevel(CardDrafting from, Level to)
    {
        SpellBook spellBook = from.SpellBook;
        from.RemoveChild(spellBook);
        to.Spellbook = spellBook;
        to.AddChild(spellBook);
    }

    public static void TransitionToDraft(Level from, CardDrafting to)
    {
        SpellBook spellBook = from.Spellbook;
        from.RemoveChild(spellBook);
        to.SpellBook = spellBook;
        to.AddChild(spellBook);
    }

    public void Add(Card card)
    {
        cards.Add(card);
        AddChild(card);
    }

    public void Remove(Card card)
    {
        cards.Remove(card);
        RemoveChild(card);
    }

    public IEnumerable<Card> Cards => cards;
    public IEnumerable<ICardEffect> Effects => cards.Select(c => c.Effect);
}
