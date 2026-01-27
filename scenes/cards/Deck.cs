
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Deck : Node
{
    private List<Card> cards = new();

    public static void TransitionToLevel(CardDrafting from, Level to)
    {
        Deck spellBook = from.Deck;
        from.RemoveChild(spellBook);
        to.SpellBook = spellBook;
        to.AddChild(spellBook);
    }

    public static void TransitionToDraft(Level from, CardDrafting to)
    {
        Deck spellBook = from.SpellBook;
        from.RemoveChild(spellBook);
        to.Deck = spellBook;
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
