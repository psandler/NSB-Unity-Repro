using System.Linq;
using NSB.Unity.Repro.Services;
using NUnit.Framework;


namespace NSB.Unity.Repro.Test
{
    [TestFixture]
    public class TestUnityNamedInstances
    {
        [Test]
        public void TestNamedInstancesShouldNotBeReRegistered()
        {
            var container = Bootstrap.BootstrapContainer();
            var before = container.ResolveAll(typeof(INamedRegService)).Count();
            Bootstrap.StartBus(container);
            var after = container.ResolveAll(typeof(INamedRegService)).Count();
            Assert.AreEqual(before, after);
        }
    }
}
