using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TestConsole
{
    /// <summary>
    /// 表达式树
    /// <a href="https://docs.microsoft.com/zh-cn/dotnet/csharp/programming-guide/concepts/expression-trees/">MSDN链接</a>
    /// </summary>
    public class Test_Expression
    {
        public static void Demo1()
        {
            // 官方例子:表达式体内只有一个式子
            // 创建表达式树
            Expression<Func<int, bool>> exprTree = num => num < 5;

            // 分解表达式树
            ParameterExpression param = exprTree.Parameters[0];//num
            BinaryExpression operation = (BinaryExpression)exprTree.Body;//函数体    {(num < 5)}
            ParameterExpression left = (ParameterExpression)operation.Left;//左节点   num
            ConstantExpression right = (ConstantExpression)operation.Right;//右表达式 5

            Console.WriteLine("表达式树例子: {0} => {1} {2} {3}",
                              param.Name, left.Name, operation.NodeType, right.Value);
            Console.Read();
        }

        public static void Demo2()
        {
            // 这里是会报错的!! 原因就是体内有多个例子需要分解!!
            // Expression<Func<int, bool>> exprTree = x => x > 5 && x < 50;
            // 
            // // 分解表达式树
            // ParameterExpression param = exprTree.Parameters[0];// x
            // BinaryExpression operation = (BinaryExpression)exprTree.Body;//函数体 {((x > 5) AndAlso (x < 50))}
            // 
            // ParameterExpression left = (ParameterExpression)operation.Left;//左节点
            // ConstantExpression right = (ConstantExpression)operation.Right;//右表达式
            // 
            // Console.WriteLine("表达式树例子: {0} => {1} {2} {3}",
            //                   param.Name, left.Name, operation.NodeType, right.Value);
        }

        //博客园例子,表达式体内有多个式子
        public static void Demo3()
        {
            List<string> names = new() { "Cai", "Edward", "Beauty" };

            Expression<Func<string, bool>> lambda0 = item => item.Length > 2;
            Expression<Func<string, bool>> lambda1 = item => item.Length < 4;
            Expression<Func<string, bool>> lambda2 = name => name.Length > 2 && name.Length < 3;

            var method = ReBuildExpression(lambda0, lambda1);
            var query = names.Where(method);
            foreach (string n in query)
                Console.WriteLine(n);

            Console.Read();
        }

        /// <summary>
        /// 重构表达式
        /// </summary>
        /// <param name="lambd0"></param>
        /// <param name="lambd1"></param>
        /// <returns></returns>
        static Func<string, bool> ReBuildExpression(Expression<Func<string, bool>> lambd0,
                                                    Expression<Func<string, bool>> lambd1)
        {
            MyExpressionVisitor my = new()
            {
                Parameter = Expression.Parameter(typeof(string), "name")
            };

            Expression left = my.Modify(lambd0.Body);
            Expression right = my.Modify(lambd1.Body);
            BinaryExpression expression = Expression.AndAlso(left, right);//就是 &&

            //构造一个新的表达式
            var lambda = Expression.Lambda<Func<string, bool>>(expression, my.Parameter);
            return lambda.Compile();
        }
    }


    /// <summary>
    /// 表达式参数分解
    /// <a href="https://www.cnblogs.com/FlyEdward/archive/2010/12/06/Linq_ExpressionTree7.html">博客园链接</a>
    /// </summary>
    public class MyExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// 公共参数
        /// </summary>
        public ParameterExpression? Parameter;
        /// <summary>
        /// 返回替换后的参数表达式
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public Expression Modify(Expression exp)
        {
            return this.Visit(exp);
        }
        /// <summary>
        /// 重写参数
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override Expression? VisitParameter(ParameterExpression p)
        {
            return Parameter;
        }
    }
}
