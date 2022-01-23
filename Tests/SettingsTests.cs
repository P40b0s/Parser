using NUnit.Framework;
using System.Threading.Tasks;
namespace Tests;

public class SettingsTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async ValueTask LoadSettings()
    {
        var s = new SettingsWorker.Settings();
        var load = await s.Load();
        var s1 = s.DefaultRules.RequisiteRule.ToString();
        //Assert.IsTrue(load);
    }

    [Test]
    public void SaveSettings()
    {
        var s = new SettingsWorker.Settings();
        s.DefaultRules.RequisiteRule.TypeSearchMaxDeep = 999;
        s.Save();
    }

     [Test]
    public async ValueTask CheckChangedSettings()
    {
        var s = new SettingsWorker.Settings();
        var load = await s.Load();
        Assert.IsTrue(load);
        Assert.Equals(999, s.DefaultRules.RequisiteRule.TypeSearchMaxDeep);
    }
}