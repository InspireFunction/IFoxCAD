namespace Test.wpf
{
    public class Class1
    {
        [CommandMethod("testwpf")]
        public void TestWPf()
        {

            var test = new TestView();
            Application.ShowModalWindow(test);
        }
    }
}
