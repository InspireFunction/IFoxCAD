#if NET35
#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8618
#pragma warning disable CS8625


using System;
using System.Collections;
using System.Text;

namespace System.Collections.Generic
{
    unsafe internal class BitHelper
    {   // should not be serialized
        private const byte MarkedBitFlag = 1;
        private const byte IntSize = 32;

        // m_length of underlying int array (not logical bit array)
        private int m_length;

        // ptr to stack alloc'd array of ints
        [System.Security.SecurityCritical]
        private int* m_arrayPtr;

        // array of ints
        private int[] m_array;

        // whether to operate on stack alloc'd or heap alloc'd array
        private bool useStackAlloc;

        /// <summary>
        /// Instantiates a BitHelper with a heap alloc'd array of ints
        /// </summary>
        /// <param name="bitArrayPtr">int array to hold bits</param>
        /// <param name="length">length of int array</param>
        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Field: m_arrayPtr" />
        // <UsesUnsafeCode Name="Parameter bitArrayPtr of type: Int32*" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal BitHelper(int* bitArrayPtr, int length)
        {
            this.m_arrayPtr = bitArrayPtr;
            this.m_length = length;
            useStackAlloc = true;
        }

        /// <summary>
        /// Instantiates a BitHelper with a heap alloc'd array of ints
        /// </summary>
        /// <param name="bitArray">int array to hold bits</param>
        /// <param name="length">length of int array</param>
        internal BitHelper(int[] bitArray, int length)
        {
            this.m_array = bitArray;
            this.m_length = length;
        }

        /// <summary>
        /// Mark bit at specified position
        /// </summary>
        /// <param name="bitPosition"></param>
        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Field: m_arrayPtr" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal unsafe void MarkBit(int bitPosition)
        {
            if (useStackAlloc)
            {
                int bitArrayIndex = bitPosition / IntSize;
                if (bitArrayIndex < m_length && bitArrayIndex >= 0)
                {
                    m_arrayPtr[bitArrayIndex] |= (MarkedBitFlag << (bitPosition % IntSize));
                }
            }
            else
            {
                int bitArrayIndex = bitPosition / IntSize;
                if (bitArrayIndex < m_length && bitArrayIndex >= 0)
                {
                    m_array[bitArrayIndex] |= (MarkedBitFlag << (bitPosition % IntSize));
                }
            }
        }

        /// <summary>
        /// Is bit at specified position marked?
        /// </summary>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        // <SecurityKernel Critical="True" Ring="0">
        // <UsesUnsafeCode Name="Field: m_arrayPtr" />
        // </SecurityKernel>
        [System.Security.SecurityCritical]
        internal unsafe bool IsMarked(int bitPosition)
        {
            if (useStackAlloc)
            {
                int bitArrayIndex = bitPosition / IntSize;
                if (bitArrayIndex < m_length && bitArrayIndex >= 0)
                {
                    return ((m_arrayPtr[bitArrayIndex] & (MarkedBitFlag << (bitPosition % IntSize))) != 0);
                }
                return false;
            }
            else
            {
                int bitArrayIndex = bitPosition / IntSize;
                if (bitArrayIndex < m_length && bitArrayIndex >= 0)
                {
                    return ((m_array[bitArrayIndex] & (MarkedBitFlag << (bitPosition % IntSize))) != 0);
                }
                return false;
            }
        }

        /// <summary>
        /// How many ints must be allocated to represent n bits. Returns (n+31)/32, but
        /// avoids overflow
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static int ToIntArrayLength(int n)
        {
            return n > 0 ? ((n - 1) / IntSize + 1) : 0;
        }
    }
}

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
#pragma warning restore CS8603 // 可能返回 null 引用。

#endif