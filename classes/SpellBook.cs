
using System.Collections.Generic;
using System.Linq;

public class SpellBook : List<Card>
{
    public IEnumerable<ICardEffect> Effects => this.Select(c => c.Effect);
}