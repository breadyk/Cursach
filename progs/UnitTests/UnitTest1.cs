using System.Security.Cryptography.Xml;

namespace UnitTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestEncrypt()
    {
        const string toEncrypt = "test text";
        const int rows = 3;
        const int cols = 3;
        const string expectEncrypted = "stte xtte";
        
        string result = Codec.Encrypt(toEncrypt, rows, cols);

        Assert.IsTrue(result == expectEncrypted);
        Assert.IsTrue(result != toEncrypt);
    }

    [TestMethod]
    public void TestDecrypt()
    {
        const string toDecrypt = "stte xtte";
        const string expectDecrypted = "test text";
        const int rows = 3;
        const int cols = 3;

        string result = Codec.Decrypt(toDecrypt, rows, cols);

        Assert.IsTrue(result == expectDecrypted);
        Assert.IsTrue(result != toDecrypt);
    }
}