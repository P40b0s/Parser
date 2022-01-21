using NUnit.Framework;
using System.Threading.Tasks;
namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async ValueTask LoadSettings()
    {
        var s = new Settings.Settings();
        var load = await s.Load();
        Assert.IsTrue(load);
    }

    [Test]
    public void SaveSettings()
    {
        var s = new Settings.Settings();
        s.RequisitesParserRules.DefaultRules.TypeSearchMaxDeep = 999;
        s.Save();
        
    }
}