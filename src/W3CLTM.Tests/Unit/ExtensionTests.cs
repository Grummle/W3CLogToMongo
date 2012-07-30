using IILogReader;
using NUnit.Framework;
using Shouldly;

namespace IISLogReader.Tests.Unit
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void Frmat_DoesntGiveAFUCK()
        {
            string value = "A string with {0} then {1} replacement.";

            value.Frmat("{more}", 1).ShouldBe("A string with {more} then 1 replacement.");
        }

        [Test]
        public void Frmat_Replaces_Values()
        {
            string value = "A string with {0} then {1} replacement.";

            value.Frmat("more", 1).ShouldBe("A string with more then 1 replacement.");
        }
    }
}