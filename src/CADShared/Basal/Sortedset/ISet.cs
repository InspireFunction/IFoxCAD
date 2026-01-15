#if NET35
// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Interface:  ISet
**
** <OWNER>kimhamil</OWNER>
**
**
** Purpose: Base interface for all generic sets.
**
**
===========================================================*/
namespace System.Collections.Generic
{
    using System;
    using System.Runtime.CompilerServices;


    /// <summary>
    /// Generic collection that guarantees the uniqueness of its elements, as defined
    /// by some comparer. It also supports basic set operations such as Union, Intersection,
    /// Complement and Exclusive Complement.
    /// </summary>
    /// <summary>
    /// 泛型集合接口，保证元素的唯一性，并支持基本的集合操作如并集、交集、补集和异或补集。
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    public interface ISet<T> : ICollection<T>
    {
        /// <summary>
        /// 向集合中添加元素，如果已存在则返回false，否则返回true
        /// </summary>
        /// <param name="item">要添加的元素</param>
        /// <returns>是否成功添加</returns>
        new bool Add(T item);

        /// <summary>
        /// 将当前集合与指定集合执行并集操作
        /// </summary>
        /// <param name="other">另一个集合</param>
        void UnionWith(IEnumerable<T> other);

        /// <summary>
        /// 将当前集合与指定集合执行交集操作
        /// </summary>
        /// <param name="other">另一个集合</param>
        void IntersectWith(IEnumerable<T> other);

        /// <summary>
        /// 从当前集合中移除与指定集合相同的元素
        /// </summary>
        /// <param name="other">另一个集合</param>
        void ExceptWith(IEnumerable<T> other);

        /// <summary>
        /// 将当前集合转换为与指定集合的异或集合（存在于其中一个集合但不同时存在于两个集合的元素）
        /// </summary>
        /// <param name="other">另一个集合</param>
        void SymmetricExceptWith(IEnumerable<T> other);

        /// <summary>
        /// 判断当前集合是否为指定集合的子集
        /// </summary>
        /// <param name="other">另一个集合</param>
        /// <returns>是否为子集</returns>
        bool IsSubsetOf(IEnumerable<T> other);

        /// <summary>
        /// 判断当前集合是否为指定集合的超集
        /// </summary>
        /// <param name="other">另一个集合</param>
        /// <returns>是否为超集</returns>
        bool IsSupersetOf(IEnumerable<T> other);

        /// <summary>
        /// 判断当前集合是否为指定集合的真超集（当前集合包含指定集合的所有元素且至少多一个元素）
        /// </summary>
        /// <param name="other">另一个集合</param>
        /// <returns>是否为真超集</returns>
        bool IsProperSupersetOf(IEnumerable<T> other);

        /// <summary>
        /// 判断当前集合是否为指定集合的真子集（当前集合的所有元素都存在于指定集合且指定集合至少多一个元素）
        /// </summary>
        /// <param name="other">另一个集合</param>
        /// <returns>是否为真子集</returns>
        bool IsProperSubsetOf(IEnumerable<T> other);

        /// <summary>
        /// 判断当前集合是否与指定集合有重叠（至少有一个共同元素）
        /// </summary>
        /// <param name="other">另一个集合</param>
        /// <returns>是否有重叠</returns>
        bool Overlaps(IEnumerable<T> other);

        /// <summary>
        /// 判断当前集合是否与指定集合相等（包含相同的元素）
        /// </summary>
        /// <param name="other">另一个集合</param>
        /// <returns>是否相等</returns>
        bool SetEquals(IEnumerable<T> other);
    }
}
#endif