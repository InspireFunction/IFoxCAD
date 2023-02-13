
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using IFox.Basal;

namespace System.Runtime.CompilerServices;

/*
 * 1.编译提示多个程序集中定义,屏蔽不了,但是不影响编译
 * 2.发现可以通过定义一个条件编译常量来进行屏蔽。
 * 3.普通包会提示编译器缺少GetSubArray函数，但是源码包不会。所以解决方案就是使用普通包的时候安装TA.System.Runtime.CompilerServices.RuntimeHelpers.GetSubArray nuget包来解决，此包为一个符号包，不会生成多余的dll
 */


#if !NORuntimeHelpers
/// <summary>
/// 
/// </summary>
internal static class RuntimeHelpers

{
    /// <summary>
    /// Slices the specified array using the specified range.
    /// </summary>
    public static T[] GetSubArray<T>(T[] array, Range range)
    {
        //if (array == null)
        //    throw new ArgumentNullException(nameof(array));
        array.NotNull(nameof(array));

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
#endif
