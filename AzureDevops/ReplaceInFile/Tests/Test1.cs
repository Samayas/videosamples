namespace Tests
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string fullAssemblyName = typeof(TestClass.Helper).Assembly.FullName;

            Assert.IsTrue(fullAssemblyName == "TestClass, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        }
    }
}
