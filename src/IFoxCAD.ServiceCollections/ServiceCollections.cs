using System;
using System.Collections.Generic;
using System.Reflection;

namespace IFoxCAD.Services
{
    #region 类的例子
    /// <summary>
    /// IOC过滤-class-的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IOCService : Attribute
    {
        public IOCService()
        {

        }
    }

    /// <summary>
    /// 学生类
    /// </summary>
    [IOCService]
    class Student
    {
        [IOCInject]
        public Teacher Teacher { set; get; }
        public void Study()
        {
            Console.WriteLine("学生开始学习");
        }
    }

    /// <summary>
    /// 老师类
    /// </summary>
    [IOCService]
    class Teacher
    {
        [IOCInject]
        public Student Student { set; get; }//制造了环形成员引用
        public void Classes()
        {
            Console.WriteLine("我是老师Classes方法");
        }
    }

    /// <summary>
    /// IOC过滤-method-的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    class IOCInject : Attribute
    {
        public IOCInject()
        {

        }
    }

    #endregion

    #region IOC容器服务集合
    class ServiceCollections
    {
        /// <summary>
        /// IOC容器(存储对象)
        /// </summary>
        private Dictionary<string, object> _iocContainer = new();

        /// <summary>
        /// type容器(为了效率)
        /// </summary>
        private Dictionary<string, Type> _typeContainer = new();

        /// <summary>
        /// 创建过的对象集合(防止环形引用成员)
        /// </summary> 
        //private Dictionary<string, object> _objList = new();


        /*
           当前程序集支持net35,
           net40可以用以下1.0的,
           最新的是5.0,cad并不支持net50,所以手写一个IOC容器         
           <PackageReference Include = "Microsoft.Extensions.DependencyInjection" />
           <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
        */
        public ServiceCollections()
        {
            //当前项目名
            string file = Assembly.GetExecutingAssembly().GetName().Name;
            //加载项目到内存上
            var assembly = Assembly.Load(file);
            //获取所有类型
            Type[] types = assembly.GetTypes();
            //遍历类型,存储Type用哈希做索引
            //为了尽可能避免内存溢出,过滤掉无特性的内容
            foreach (var type in types)
            {
#if NET35 || NET40
                var atts = type.GetCustomAttributes(true);
                foreach (var item in atts)
                {
                    if (item is IOCService)
                    {
                        _typeContainer.Add(type.FullName, type);
                    }
                }
#else
                var iOCService = type.GetCustomAttribute<IOCService>();
                if (iOCService != null)
                {
                    _typeContainer.Add(type.FullName, type);
                }
#endif
            }
        }


        /// <summary>
        /// 创建方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object CreateClassAndSetAttributes(Type type)
        {
            // 处理环形成员引用,直接返回已经创建了的成员
            foreach (var item in _iocContainer)
            {
                if (item.Key == type.FullName)
                {
                    return _iocContainer[type.FullName];
                }
            }
            // 创建对象
            object obj = Activator.CreateInstance(type);
            // 存储对象
            _iocContainer.Add(obj.GetType().FullName, obj);
            // 属性赋值(依赖注入)
            PropertyInfo[] infos = type.GetProperties();
            foreach (var pr in infos)
            {
#if NET35 || NET40
                var atts = pr.GetCustomAttributes(true);
                foreach (var item in atts)
                {
                    if (item is IOCInject)
                    {
                        Type type1 = _typeContainer[pr.PropertyType.FullName];
                        object objInfo = CreateClassAndSetAttributes(type1);
                        //属性赋值
                        pr.SetValue(obj, objInfo, null);
                    }
                }
#else
                // 属性过滤,目的: 指定的属性进行依赖注入
                var iOCInject = pr.GetCustomAttribute<IOCInject>();
                if (iOCInject != null)
                {
                    Type type1 = _typeContainer[pr.PropertyType.FullName];
                    object objInfo = CreateClassAndSetAttributes(type1);
                    //属性赋值
                    pr.SetValue(obj, objInfo);
                }
#endif 
            }
            return obj;
        }


        //防止在构造的时候直接创建对象,所以这里为了客户端需要对象的时候才创建
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public object GetService(string typeFullName)
        {
            Type type = _typeContainer[typeFullName];
            return GetService(type);
        }
         
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public T? GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public object GetService(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("GetService");
            }
            //递归创建对象和属性赋值 
            return CreateClassAndSetAttributes(type);
        }
    }
    #endregion
}