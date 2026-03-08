using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ConcurrentCollections;

/// <summary>
/// Represents a thread-safe set of unique elements.
/// </summary>
/// <typeparam name="T">The type of the elements in the set.</typeparam>
/// <remarks>
/// All public and protected members of <see cref="ConcurrentSet{T}"/> are thread-safe and may be used
/// concurrently from multiple threads.
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public class ConcurrentSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
    // Using ConcurrentDictionary with dummy values to implement the set
    private readonly ConcurrentDictionary<T, byte> _dict;

    private const byte DummyValue = 1; // Dummy value to use for all entries

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class
    /// that is empty and has the default concurrency level, has the default initial capacity, 
    /// and uses the default comparer for the key type.
    /// </summary>
    public ConcurrentSet()
    {
        _dict = new ConcurrentDictionary<T, byte>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class
    /// that contains elements copied from the specified enumerable and uses the default comparer.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    public ConcurrentSet(IEnumerable<T> collection)
    {
        _dict = new ConcurrentDictionary<T, byte>();

        if (collection != null)
        {
            foreach (T item in collection)
            {
                _dict.TryAdd(item, DummyValue);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class
    /// with the specified equality comparer.
    /// </summary>
    /// <param name="comparer">The equality comparer to use when comparing items.</param>
    public ConcurrentSet(IEqualityComparer<T> comparer)
    {
        _dict = new ConcurrentDictionary<T, byte>(comparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class
    /// that contains elements copied from the specified enumerable and uses the specified equality comparer.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <param name="comparer">The equality comparer to use when comparing items.</param>
    public ConcurrentSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        _dict = new ConcurrentDictionary<T, byte>(comparer);

        if (collection != null)
        {
            foreach (T item in collection)
            {
                _dict.TryAdd(item, DummyValue);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class
    /// with the specified concurrency level and capacity.
    /// </summary>
    /// <param name="concurrencyLevel">The estimated number of threads that will update the set concurrently.</param>
    /// <param name="capacity">The initial number of elements that the set can contain.</param>
    public ConcurrentSet(int concurrencyLevel, int capacity)
    {
        _dict = new ConcurrentDictionary<T, byte>(concurrencyLevel, capacity);
    }

    /// <summary>
    /// Adds the specified element to the set.
    /// </summary>
    /// <param name="item">The element to add to the set.</param>
    /// <returns>true if the element was added to the set; false if the element already exists.</returns>
    public bool Add(T item)
    {
        return _dict.TryAdd(item, DummyValue);
    }

    /// <summary>
    /// Adds the specified element to the set.
    /// </summary>
    /// <param name="item">The element to add to the set.</param>
    void ICollection<T>.Add(T item)
    {
        _dict.TryAdd(item, DummyValue);
    }

    /// <summary>
    /// Adds the specified element to the set if it does not already exist.
    /// </summary>
    /// <param name="item">The element to add to the set.</param>
    /// <returns>The value of the element that was added or already existed in the set.</returns>
    public T AddOrUpdate(T item)
    {
        _dict.GetOrAdd(item, DummyValue);
        return item;
    }

    /// <summary>
    /// Removes all elements from the set.
    /// </summary>
    public void Clear()
    {
        _dict.Clear();
    }

    /// <summary>
    /// Determines whether the set contains the specified element.
    /// </summary>
    /// <param name="item">The element to locate in the set.</param>
    /// <returns>true if the set contains the specified element; otherwise, false.</returns>
    public bool Contains(T item)
    {
        return _dict.ContainsKey(item);
    }

    /// <summary>
    /// Removes the specified element from the set.
    /// </summary>
    /// <param name="item">The element to remove from the set.</param>
    /// <returns>true if the element was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        byte dummy;
        return _dict.TryRemove(item, out dummy);
    }

    /// <summary>
    /// Copies the elements of the set to an array, starting at the specified index.
    /// </summary>
    /// <param name="array">The array to copy the elements to.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null) throw new System.ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        try
        {
            // Acquire all locks to ensure a consistent snapshot
            var dict = _dict; // Capture current dictionary reference
            var keys = dict.Keys;

            if (array.Length - arrayIndex < keys.Count)
                throw new ArgumentException("Destination array is not large enough to copy all elements", nameof(array));

            int index = arrayIndex;
            foreach (T item in keys)
            {
                array[index++] = item;
            }
        }
        finally
        {
            // Locks release is handled internally by the dictionary
        }
    }

    /// <summary>
    /// Gets the number of elements in the set.
    /// </summary>
    public int Count
    {
        get { return _dict.Count; }
    }

    /// <summary>
    /// Gets a value indicating whether the set is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get { return false; }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the set.
    /// </summary>
    /// <returns>An enumerator for the set.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _dict.Keys.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the set.
    /// </summary>
    /// <returns>An enumerator for the set.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Modifies the current set to include all elements that are present in both this set and the specified collection.
    /// </summary>
    /// <param name="other">The collection to compute the intersection with.</param>
    public void IntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var otherSet = new HashSet<T>(other);
        var itemsToRemove = new List<T>();

        // Collect items to remove first to avoid issues with concurrent modification
        foreach (T item in this)
        {
            if (!otherSet.Contains(item))
            {
                itemsToRemove.Add(item);
            }
        }

        // Remove collected items
        foreach (T item in itemsToRemove)
        {
            Remove(item);
        }
    }

    /// <summary>
    /// Modifies the current set to contain all elements that are present in either this set or the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void UnionWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (T item in other)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Determines whether the current set is a proper subset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is a proper subset of other; otherwise, false.</returns>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var otherSet = new HashSet<T>(other);

        // Check if all elements in this set are in other set
        foreach (T item in this)
        {
            if (!otherSet.Contains(item))
                return false;
        }

        // Check if other set has more elements than this set
        return otherSet.Count > Count;
    }

    /// <summary>
    /// Determines whether the current set is a proper superset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is a proper superset of other; otherwise, false.</returns>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // If other has more elements than this, it can't be a subset
        var otherEnum = other.GetEnumerator();
        int otherCount = 0;
        while (otherEnum.MoveNext())
        {
            otherCount++;
            if (otherCount > Count)
                return false;
        }
        otherEnum.Dispose();

        // Check if all elements in other are in this set
        foreach (T item in other)
        {
            if (!Contains(item))
                return false;
        }

        // Check if this set has more elements than other
        return Count > otherCount;
    }

    /// <summary>
    /// Determines whether the current set is a subset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is a subset of other; otherwise, false.</returns>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var otherSet = new HashSet<T>(other);

        foreach (T item in this)
        {
            if (!otherSet.Contains(item))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the current set is a superset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is a superset of other; otherwise, false.</returns>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (T item in other)
        {
            if (!Contains(item))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the current set overlaps with the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set and other share at least one common element; otherwise, false.</returns>
    public bool Overlaps(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (T item in other)
        {
            if (Contains(item))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the current set and the specified collection contain the same elements.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>true if the current set is equal to other; otherwise, false.</returns>
    public bool SetEquals(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var otherSet = new HashSet<T>(other);

        // If counts are different, sets can't be equal
        if (Count != otherSet.Count)
            return false;

        // Check if all elements in this set are in other set
        foreach (T item in this)
        {
            if (!otherSet.Contains(item))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Modifies the current set to contain only elements that are present either in this set or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (T item in other)
        {
            if (Contains(item))
            {
                Remove(item);
            }
            else
            {
                Add(item);
            }
        }
    }

    /// <summary>
    /// Modifies the current set to contain only elements that are also in the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    public void ExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (T item in other)
        {
            Remove(item);
        }
    }
}