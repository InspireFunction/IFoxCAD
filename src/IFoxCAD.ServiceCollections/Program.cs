using System;

namespace IFoxCAD.Services
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Go");

            //这里是例子:客户端
            //创建IOC容器
            {
                var de = new ServiceCollections();

                //创建这个类,以及它的所有成员
                Student st;
                //两种用法例子
#if true2
                st = (Student)de.GetService("Student");
#else
                st = de.GetService<Student>(); //这样怎么拿到形参来实例化....
#endif
                var te = st.Teacher;
                te.Classes();
                var aaa = te.Student;//环形成员引用
                aaa.Study();
                st.Study();
            } 

            //主机->托管服务->IOC容器<-再依赖注入容器内

            {//这是客户端
 
                //发起请求
                IFoxContext context = new();
                  
                //创建中间件 
                IApplicationBuilder ab = new();
                ab.UserMiddlewareAuthentication();
                ab.UserMiddlewareException();
                ab.UserMiddlewareAuthorization();

                //获取管道
                var application = ab.Build();
                //从管道创建中间件链条
                var middleware = application.GetMiddleware();

                //4、处理客户端网络请求
                middleware.HandlerRequest(context); 
            }

            Console.WriteLine("End");
            Console.ReadKey();
        }
    }
}
