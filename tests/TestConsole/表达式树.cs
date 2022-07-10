using System;
using System.Linq.Expressions;

namespace TestConsole
{
    public class Expression
    {
        /// <summary>
        /// 表达式树
        /// <a href="https://docs.microsoft.com/zh-cn/dotnet/csharp/programming-guide/concepts/expression-trees/">MSDN链接</a>
        /// </summary>
        public static void Demo()
        {
            // 创建表达式树  
            Expression<Func<int, bool>> exprTree = num => num < 5;

            // 分解表达式树
            ParameterExpression param = exprTree.Parameters[0];
            BinaryExpression operation = (BinaryExpression)exprTree.Body;//函数体
            ParameterExpression left = (ParameterExpression)operation.Left;//左节点
            ConstantExpression right = (ConstantExpression)operation.Right;//右表达式

            Console.WriteLine("表达式树例子: {0} => {1} {2} {3}",
                              param.Name, left.Name, operation.NodeType, right.Value);
        }
    }
}
