using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IFoxCAD.Services
{
    /// <summary>
    /// 中间件建造者
    /// 所有新建的中间件都在这里维护
    /// </summary>
    class IApplicationBuilder
    {
        IApplication _app = new();

        public IApplicationBuilder UserMiddlewareAuthentication()
        {
            var mi = new MiddlewareAuthentication();
            _app.AddMiddleware(mi);
            return this;
        }
          
        public IApplicationBuilder UserMiddlewareException()
        {
            var mi = new MiddlewareException();
            _app.AddMiddleware(mi);
            return this;
        }

        public IApplicationBuilder UserMiddlewareAuthorization()
        {
            var mi = new MiddlewareAuthorization();
            _app.AddMiddleware(mi);
            return this;
        }

        public IApplicationBuilder UserMiddlewareDefault404()
        {
            var mi = new MiddlewareDefault404();
            _app.AddMiddleware(mi);
            return this;
        }

        /// <summary>
        /// 获取中间件管道
        /// </summary>
        /// <returns></returns>
        public IApplication Build()
        {
            return _app;
        } 
    }
}
