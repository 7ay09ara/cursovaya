namespace UniTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void SortArrayTest()
    {
        int[] ints = [99,0,13,5];
        Services.HeapSort(ints);
        Assert.IsTrue(ints[0] == 0 && ints[1] == 5 && ints[2]==13 && ints[3]==99);
    }
}