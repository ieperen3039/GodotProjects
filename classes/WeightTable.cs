using System;
using System.Collections.Generic;
using System.Linq;

class WeightTable<T>
{
    private List<int> weights = new();
    private List<T> elements = new();

    public WeightTable<T> Add(T element, int weight)
    {
        elements.Add(element);
        weights.Add(weight);
        return this;
    }

    public T Get(Random rng)
    {
        int effectIndex = -1;
        int number = rng.Next() % weights.Sum();
        while (number >= 0) number -= weights[++effectIndex];

        return elements[effectIndex];
    }
}