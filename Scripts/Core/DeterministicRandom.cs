namespace RuneGrid.Tactics.Core;

public sealed class DeterministicRandom
{
    private uint _state;

    public DeterministicRandom(string seed) => _state = Hash(seed);

    public float NextFloat()
    {
        var value = _state += 0x6D2B79F5;
        value = unchecked((value ^ (value >> 15)) * (value | 1));
        value ^= value + unchecked((value ^ (value >> 7)) * (value | 61));
        return (value ^ (value >> 14)) / 4294967296f;
    }

    public int NextInt(int minimumInclusive, int maximumInclusive) => minimumInclusive + (int)(NextFloat() * (maximumInclusive - minimumInclusive + 1));
    public bool Chance(float probability) => NextFloat() < probability;
    public T Pick<T>(IReadOnlyList<T> items) => items[NextInt(0, items.Count - 1)];

    public IReadOnlyList<T> Shuffle<T>(IReadOnlyList<T> items)
    {
        var result = items.ToList();
        for (var index = result.Count - 1; index > 0; index--)
        {
            var other = NextInt(0, index);
            (result[index], result[other]) = (result[other], result[index]);
        }
        return result;
    }

    public static uint Hash(string input)
    {
        uint hash = 2166136261;
        foreach (var character in input)
        {
            hash ^= character;
            hash *= 16777619;
        }
        return hash;
    }
}
