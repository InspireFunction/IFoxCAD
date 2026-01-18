using System;
using System.Collections.Generic;
using System.Text;

namespace TestShared;

public class TestParallel
{
    public class TestAddEntity
    {
        [CommandMethod(nameof(Test_Parallel))]
        public void Test_Parallel()
        {
            using DBTrans tr = new(); // 开启事务

            // todo 测试并行遍历所有块表记录
            foreach (var id in tr.BlockTable)
            {
                var ent = tr.GetObject(id);

            }


            //tr.BlockTable.AsParallel().ForAll(block => {
            //    Env.Printl($"块名: {block.Name}");
            //});
        }
    }
}
