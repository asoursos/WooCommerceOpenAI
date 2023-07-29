namespace WebApplication2.Helpers;

public static class Hasher
{
    /// <summary>
    /// Deterministic hash function.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>A long number (max string length 20).</returns>
    public static ulong CalculateDeterministicHash(string input)
    {
        ulong hashedValue = 3074457345617258791ul;
        for (int i = 0; i < input.Length; i++)
        {
            hashedValue += input[i];
            hashedValue *= 3074457345617258799ul;
        }

        return hashedValue;
    }
}
