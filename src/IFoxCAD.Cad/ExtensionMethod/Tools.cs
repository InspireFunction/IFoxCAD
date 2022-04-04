using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IFoxCAD.Cad
{
    public static class Tools
    {
        public static void TestTimes(int count, string message, Action action)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();  //开始监视代码运行时间
            for (int i = 0; i < count; i++)
            {
                action.Invoke();          //需要测试的代码
            }
            watch.Stop();  //停止监视
            TimeSpan timespan = watch.Elapsed;  //获取当前实例测量得出的总时间
            double time = timespan.TotalMilliseconds;
            string name = "毫秒";
            if (timespan.TotalMilliseconds > 1000)
            {
                time = timespan.TotalSeconds;
                name = "秒";
            }
            Env.Print($"{message} 代码执行 {count} 次的时间：{time} ({name})");  //总毫秒数
        }
    }

}
