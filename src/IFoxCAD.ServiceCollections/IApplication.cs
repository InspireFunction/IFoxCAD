using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IFoxCAD.Services
{
    /// <summary>
    /// 中间件管道
    /// </summary>
    class IApplication
    {
        /// <summary>
        /// 中间件管道
        /// </summary>
        public List<Middleware> _middlewares = new();

        /// <summary>
        /// 添加中间件
        /// </summary>
        /// <param name="middleware"></param>
        public void AddMiddleware(Middleware middleware)
        {
            _middlewares.Add(middleware);
        }

        /// <summary>
        /// 创建中间链
        /// </summary>
        /// <returns></returns>
        public Middleware GetMiddleware()
        {
            if (_middlewares == null || _middlewares.Count == 0)
            {
                throw new ArgumentNullException("IApplication.middlewares");
            }
            // 遍历集合形成一条中间件链
            Middleware midFirst = new MiddlewareDefault404();
            Middleware midLast = midFirst;
            foreach (var mid in _middlewares)
            {
                midLast.NextMiddleware = mid;
                midLast = mid;
            }

            //再把链条取出来
            //List<Middleware> mid2 = new();
            //mid2.Add(midFirst);
            //for (int i = 0; i < _middlewares.Count(); i++)
            //{
            //    midFirst = midFirst.NextMiddleware;
            //    mid2.Add(midFirst);
            //}

            return midFirst;
        }
    }
}





