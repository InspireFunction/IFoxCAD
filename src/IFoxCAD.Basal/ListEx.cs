
namespace IFoxCAD.Basal;

public static class ListEx
{
    public static bool EqualsAll<T>(this IList<T> a, IList<T> b)
    {
        return EqualsAll(a, b, null); 
        // there is a slight performance gain in passing null here.
        // It is how it is done in other parts of the framework.
    }

    public static bool EqualsAll<T>(this IList<T> a!!, IList<T> b!!, IEqualityComparer<T>? comparer)
    {
        if (a.Count != b.Count)
            return false;

        comparer ??= EqualityComparer<T>.Default;

        for (int i = 0; i < a.Count; i++)
        {
            if (!comparer.Equals(a[i], b[i]))
                return false;
        }
        return true;
    }
}
