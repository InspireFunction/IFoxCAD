using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 集合扩展类
    /// </summary>
    public static class CollectionEx
    {
        /// <summary>
        /// 对象id迭代器转换为集合
        /// </summary>
        /// <param name="ids">对象id的迭代器</param>
        /// <returns>对象id集合</returns>
        public static ObjectIdCollection ToCollection(this IEnumerable<ObjectId> ids)
        {
            return new ObjectIdCollection(ids.ToArray());
        }

        /// <summary>
        /// 实体迭代器转换为集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="objs">实体对象的迭代器</param>
        /// <returns>实体集合</returns>
        public static DBObjectCollection ToCollection<T>(this IEnumerable<T> objs) where T : DBObject
        {
            DBObjectCollection objCol = new();
            foreach (T obj in objs)
                objCol.Add(obj);
            return objCol;
        }

        /// <summary>
        /// double 数值迭代器转换为 double 数值集合
        /// </summary>
        /// <param name="doubles">double 数值迭代器</param>
        /// <returns>double 数值集合</returns>
        public static DoubleCollection ToCollection(this IEnumerable<double> doubles)
        {
            return new DoubleCollection(doubles.ToArray());
        }

        /// <summary>
        /// 二维点迭代器转换为二维点集合
        /// </summary>
        /// <param name="pts">二维点迭代器</param>
        /// <returns>二维点集合</returns>
        public static Point2dCollection ToCollection(this IEnumerable<Point2d> pts)
        {
            return new Point2dCollection(pts.ToArray());
        }

        /// <summary>
        /// 三维点迭代器转换为三维点集合
        /// </summary>
        /// <param name="pts">三维点迭代器</param>
        /// <returns>三维点集合</returns>
        public static Point3dCollection ToCollection(this IEnumerable<Point3d> pts)
        {
            return new Point3dCollection(pts.ToArray());
        }

        /// <summary>
        /// 对象id集合转换为对象id列表
        /// </summary>
        /// <param name="ids">对象id集合</param>
        /// <returns>对象id列表</returns>
        public static List<ObjectId> ToList(this ObjectIdCollection ids)
        {
            return ids.Cast<ObjectId>().ToList();
        }

        public static List<string> ToList(this StringCollection strs)
        {
            return strs.Cast<string>().ToList();
        }

        /* Cast不进行过滤,而是直接强转
              var s = new System.Collections.ArrayList();
              s.Add("1");
              s.Add("a");
              s.Add(5666);

              var aaa = s.Cast<int>().ToList();
                  System.InvalidCastException: 指定的转换无效。
                  + List<T>..ctor(IEnumerable<T>)
                  + System.Linq.Enumerable.ToList<TSource>(IEnumerable<TSource>)

              var aaa = s.Cast<string>().ToList();
                  System.InvalidCastException: 无法将类型为“System.Int32”的对象强制转换为类型“System.String”。
                  + List<T>..ctor(IEnumerable<T>)
                  + System.Linq.Enumerable.ToList<TSource>(IEnumerable<TSource>)
        */

        /// <summary>
        /// 遍历集合的迭代器，执行action委托
        /// </summary>
        /// <typeparam name="T">集合值的类型</typeparam>
        /// <param name="source">集合</param>
        /// <param name="action">要运行的委托</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            foreach (var element in source)
                action.Invoke(element);
        }


        /// <summary>
        /// 同时遍历集合索引和值的迭代器，执行action委托
        /// </summary>
        /// <typeparam name="T">集合值的类型</typeparam>
        /// <param name="source">集合</param>
        /// <param name="action">要运行的委托</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            int i = 0;
            foreach (var item in source)
            {
                action.Invoke(i, item);
                i++;
            }
        }

    }
}
