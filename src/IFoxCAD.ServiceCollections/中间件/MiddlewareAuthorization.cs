using System;

namespace IFoxCAD.Services
{
    /// <summary>
    /// 授权中间件
    /// </summary>
    class MiddlewareAuthorization : Middleware
    {
        /// <summary>
        /// 中间件是否终止条件
        /// </summary>
        private const string _token = "授权令牌";
        /// <summary>
        /// 处理客户端请求
        /// </summary>
        /// <param name="context"></param>
        public override void HandlerRequest(IFoxContext context)
        {
            Console.WriteLine($"执行中间件{_token}");
            //判断请求url是否包含token，包含则终止
            if (context.Request.RequestUrl.Contains(_token))
            {
                Console.WriteLine($"中间件终止了{_token}");
                return;
            }
            else
            {
                NextMiddleware.HandlerRequest(context);
            }
        }
    }
}