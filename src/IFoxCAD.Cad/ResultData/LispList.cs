using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IFoxCAD.Cad
{
    /* 老代码，以防万一，不能删除
    /// <summary>
    /// lisp列表的类
    /// </summary>
    public class LispList : LispData, IEnumerable<LispData>
    {
        /// <summary>
        /// LispData 列表
        /// </summary>
        protected List<LispData> _lst =
            new List<LispData>();

        /// <summary>
        /// LispList 的父对象
        /// </summary>
        protected LispList _parent;

        /// <summary>
        /// 列表的结尾
        /// </summary>
        protected virtual TypedValue ListEnd
        {
            get { return new TypedValue((int)LispDataType.ListEnd); }
        }

        /// <summary>
        /// 是否为列表
        /// </summary>
        public override bool IsList
        {
            get { return true; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LispList()
            : base(LispDataType.ListBegin)
        { }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>LispData对象</returns>
        public LispData this[int index]
        {
            get { return _lst[index]; }
            set { _lst[index] = value; }
        }

        #region AddRemove

        /// <summary>
        /// 列表长度
        /// </summary>
        public int Count
        {
            get { return _lst.Count; }
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">LispData 对象</param>
        public void Add(LispData value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            _lst.Add(value);
            if (value.IsList)
                ((LispList)value)._parent = this;
        }

        private void Add(TypedValue value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">布尔值</param>
        public void Add(bool value)
        {
            Add(value ? T : Nil);
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">16位整型数值</param>
        public void Add(short value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">32位整型数值</param>
        public void Add(int value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">64位整型数值</param>
        public void Add(double value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">二维点</param>
        public void Add(Point2d value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">三维点</param>
        public void Add(Point3d value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">对象id</param>
        public void Add(ObjectId value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">字符串</param>
        public void Add(string value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="value">选择集</param>
        public void Add(SelectionSet value)
        {
            Add(new LispData(value));
        }

        /// <summary>
        /// 删除 index 索引处的值
        /// </summary>
        /// <param name="index">索引</param>
        public void RemoveAt(int index)
        {
            if (index > -1 && index < _lst.Count)
            {
                _lst.RemoveAt(index);
            }
        }

        /// <summary>
        /// 删除值
        /// </summary>
        /// <param name="value">LispData对象</param>
        public void Remove(LispData value)
        {
            _lst.Remove(value);
        }

        /// <summary>
        /// 是否存在值
        /// </summary>
        /// <param name="value">LispData对象</param>
        /// <returns><see langword="true"/>表示存在，<see langword="false"/>表示不存在</returns>
        public bool Contains(LispData value)
        {
            return _lst.Contains(value);
        }

        /// <summary>
        /// 返回值的索引
        /// </summary>
        /// <param name="value">LispData对象</param>
        /// <returns>索引</returns>
        public int IndexOf(LispData value)
        {
            return _lst.IndexOf(value);
        }

        #endregion AddRemove

        #region Convert

        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns>LispData迭代器</returns>
        public IEnumerator<LispData> GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal override void GetValues(ResultBuffer rb)
        {
            rb.Add(Value);
            _lst.ForEach(d => d.GetValues(rb));
            rb.Add(ListEnd);
        }

        /// <summary>
        /// 获取lisplist列表的值
        /// </summary>
        /// <returns>lisplist列表的值</returns>
        public override object GetValue()
        {
            return ToBuffer();
        }

        /// <summary>
        /// 设置lisplist列表的值
        /// </summary>
        /// <param name="value">lisplist列表的值</param>
        public override void SetValue(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 设置lisplist列表的值
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="value">值</param>
        public override void SetValue(int code, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 转换为 ResultBuffer
        /// </summary>
        /// <returns>ResultBuffer对象</returns>
        public ResultBuffer ToBuffer()
        {
            ResultBuffer rb = new ResultBuffer();
            GetValues(rb);
            return rb;
        }

        /// <summary>
        /// 从 ResultBuffer 转换为 list
        /// </summary>
        /// <param name="rb">ResultBuffer对象</param>
        /// <returns>LispList对象</returns>
        public static LispList FromBuffer(ResultBuffer rb)
        {
            LispList lst = new LispList();
            if (rb != null)
            {
                LispList clst = lst;
                foreach (TypedValue value in rb)
                {
                    switch ((LispDataType)value.TypeCode)
                    {
                        case LispDataType.ListBegin:
                            var slst = new LispList();
                            clst.Add(slst);
                            clst = slst;
                            break;

                        case LispDataType.ListEnd:
                            clst = clst._parent;
                            break;

                        case LispDataType.DottedPair:
                            var plst = clst._parent;
                            plst[plst.IndexOf(clst)] =
                                new LispDottedPair
                                {
                                    _lst = clst._lst,
                                    _parent = clst._parent
                                };
                            clst = plst;
                            break;

                        default:
                            clst.Add(value);
                            break;
                    }
                }
            }
            return lst;
        }

        #endregion Convert
    }

    /// <summary>
    /// lisp 点对表
    /// </summary>
    public class LispDottedPair : LispList
    {
        /// <summary>
        /// 列表结尾
        /// </summary>
        protected override TypedValue ListEnd
        {
            get { return new TypedValue((int)LispDataType.DottedPair); }
        }
    }
    */

    /// <summary>
    /// lisp数据封装类
    /// </summary>
    public class LispList : TypedValueList
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public LispList() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="values">TypedValue 迭代器</param>
        public LispList(IEnumerable<TypedValue> values) : base(values) { }
        
        /// <summary>
        /// lisp 列表的值
        /// </summary>
        public virtual List<TypedValue> Value
        {
            get
            {
                var value = new List<TypedValue>
                {
                    new TypedValue((int)LispDataType.ListBegin,-1),
                    new TypedValue((int)LispDataType.ListEnd,-1)
                };
                value.InsertRange(1, this);
                return value;
            }
        }

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="obj">组码值</param>
        public override void Add(int code, object obj)
        {
            if (code < 5000)
            {
                throw new System.Exception("传入的组码值不是 lisp数据 有效范围！");
            }
            Add(new TypedValue(code, obj));
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="code">dxfcode枚举值</param>
        /// <param name="obj">组码值</param>
        public void Add(LispDataType code, object obj)
        {
            Add((int)code, obj);
        }
        /// <summary>
        /// 添加数据，参数为true时添加 lisp 中的 T，false时添加 lisp 中的 nil
        /// </summary>
        /// <param name="value">bool 型的数据</param>
        public void Add(bool value)
        {
            if (value)
            {
                Add(LispDataType.T_atom, true);
            }
            else
            {
                Add(LispDataType.Nil, null);
            }
        }
        /// <summary>
        /// 添加字符串
        /// </summary>
        /// <param name="value">字符串</param>
        public void Add(string value)
        {
            Add(LispDataType.Text, value);
        }
        /// <summary>
        /// 添加短整型数
        /// </summary>
        /// <param name="value">短整型数</param>
        public void Add(short value)
        {
            Add(LispDataType.Int16, value);
        }
        /// <summary>
        /// 添加整型数
        /// </summary>
        /// <param name="value">整型数</param>
        public void Add(int value)
        {
            Add(LispDataType.Int32, value);
        }
        /// <summary>
        /// 添加浮点数
        /// </summary>
        /// <param name="value">浮点数</param>
        public void Add(double value)
        {
            Add(LispDataType.Double, value);
        }
        /// <summary>
        /// 添加对象id
        /// </summary>
        /// <param name="value">对象id</param>
        public void Add(ObjectId value)
        {
            Add(LispDataType.ObjectId, value);
        }
        /// <summary>
        /// 添加选择集
        /// </summary>
        /// <param name="value">选择集</param>
        public void Add(SelectionSet value)
        {
            Add(LispDataType.SelectionSet, value);
        }
        /// <summary>
        /// 添加二维点
        /// </summary>
        /// <param name="value">二维点</param>
        public void Add(Point2d value)
        {
            Add(LispDataType.Point2d, value);
        }
        /// <summary>
        /// 添加三维点
        /// </summary>
        /// <param name="value">三维点</param>
        public void Add(Point3d value)
        {
            Add(LispDataType.Point3d, value);
        }
        /// <summary>
        /// 添加二维点
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void Add(double x, double y)
        {
            Add(LispDataType.Point2d, new Point2d(x, y));
        }
        /// <summary>
        /// 添加三维点
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public void Add(double x, double y, double z)
        {
            Add(LispDataType.Point3d, new Point3d(x, y, z));
        }
        /// <summary>
        /// 添加列表
        /// </summary>
        /// <param name="value">lisp 列表</param>
        public void Add(LispList value)
        {
            this.AddRange(value.Value);
        }

        #endregion

        #region 转换器
        /// <summary>
        /// ResultBuffer 隐式转换到 LispDataList
        /// </summary>
        /// <param name="buffer">ResultBuffer 实例</param>
        public static implicit operator LispList(ResultBuffer buffer) => new LispList(buffer.AsArray());
        /// <summary>
        /// LispDataList 隐式转换到 TypedValue 数组
        /// </summary>
        /// <param name="values">TypedValueList 实例</param>
        public static implicit operator TypedValue[](LispList values) => values.Value.ToArray();
        /// <summary>
        /// TypedValueList 隐式转换到 ResultBuffer
        /// </summary>
        /// <param name="values">TypedValueList 实例</param>
        public static implicit operator ResultBuffer(LispList values) => new ResultBuffer(values.Value.ToArray());
        /// <summary>
        /// TypedValue 数组隐式转换到 TypedValueList
        /// </summary>
        /// <param name="values">TypedValue 数组</param>
        public static implicit operator LispList(TypedValue[] values) => new LispList(values);
        #endregion
    }
}