using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IFoxCAD.Services
{
    abstract class Middleware
    {
        public Middleware NextMiddleware { get; set; }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context"></param>
        public abstract void HandlerRequest(IFoxContext context); 
    }
} 