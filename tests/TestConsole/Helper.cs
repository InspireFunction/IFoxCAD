//namespace TestConsole;

//public static class Helper
//{
//    public static void ForEach<T>(this IEnumerable<T> ints, Action<T, CtrlState> action)
//    {
//        CtrlState state = new();
//        foreach (var item in ints)
//        {
//            action(item, state);
//            if (!state.IsRun)
//                break;
//        }

//        // int forNum = 5;
//        // var result = Parallel.For(0, forNum, (int i, ParallelCtrlState pls) => {
//        //     if (i > 2)
//        //         pls.Break();
//        //     Task.Delay(10).Wait();
//        // });
//    }
//}