using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IILogReader;
using NUnit.Framework;
using Shouldly;

namespace IISLogReader.Tests.Unit
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void Frmat_Replaces_Values()
        {
            var value = "A string with {0} then {1} replacement.";

            value.Frmat("more", 1).ShouldBe("A string with more then 1 replacement.");
        }

        [Test]
        public void Frmat_DoesntGiveAFUCK()
        {
            var value = "A string with {0} then {1} replacement.";

            value.Frmat("{more}", 1).ShouldBe("A string with {more} then 1 replacement.");
        }
    }
}
