using System;

namespace IFoxCAD.Services
{
    /// <summary>
    /// 请求上下文
    /// </summary>
    class IFoxContext
    {
        public IFoxContext()
        {
            Request = new Request();
            Response = new Response();
        }

        public Request Request { set; get; }  //请求输入类
        public Response Response { set; get; }//请求响应类
         
    }
}

namespace IFoxCAD.Services
{
    /// <summary>
    /// 请求输入类
    /// </summary>
    class Request
    {
        /// <summary>
        /// 请求URL
        /// </summary>
        public string RequestUrl = "404";
    }
}

namespace IFoxCAD.Services
{
    /// <summary>
    /// 请求响应类
    /// </summary>
    class Response
    {
        /// <summary>
        /// 响应请求URL
        /// </summary>
        public string RequestUrl = "404"; 
    }
}