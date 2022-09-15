﻿// #if NET35
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.CompilerServices;

// 编译提示多个程序集中定义,屏蔽不了,但是不影响编译
// #pragma warning disable CS1685 // 类型与导入类型冲突
public static class RuntimeHelpers
// #pragma warning restore CS1685 // 类型与导入类型冲突
{
    /// <summary>
    /// Slices the specified array using the specified range.
    /// </summary>
    public static T[] GetSubArray<T>(T[] array, Range range)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        (int offset, int length) = range.GetOffsetAndLength(array.Length);

        if (default(T)! != null || typeof(T[]) == array.GetType()) // NULLABLE: default(T) == null warning (https://github.com/dotnet/roslyn/issues/34757)
        {
            // We know the type of the array to be exactly T[].
            if (length == 0)
            {
                // return Array.Empty<T>();
                return new T[0];
            }

            var dest = new T[length];
            Array.Copy(array, offset, dest, 0, length);
            return dest;
        }
        else
        {
            // The array is actually a U[] where U:T.
            T[] dest = (T[])Array.CreateInstance(array.GetType().GetElementType()!, length);
            Array.Copy(array, offset, dest, 0, length);
            return dest;
        }
    }
}
// #endif