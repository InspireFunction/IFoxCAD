using IFoxCAD.Basal;

using System.Diagnostics.CodeAnalysis;
namespace Test
{
    public class TestLoop
    {
        



        [CommandMethod("testloop")]
        public void Testloop()
        {
            for (int i = 0; i < 10000000; i++)
            {
                var loop = new LoopList<int>();
                for (int j = 0; j < 100000; j++)
                {
                    loop.Add(j);
                }
                //loop.Add(1);
                //loop.Add(2);
                //loop.Add(3);
                //loop.Add(4);

                //loop.Add(5);
               

                loop.Clear();
                
            }
            

        }
    }
}
