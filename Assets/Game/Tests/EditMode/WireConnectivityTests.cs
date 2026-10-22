using NUnit.Framework;
using ShorelineDefense.Presentation;

namespace ShorelineDefense.Tests
{
    public sealed class WireConnectivityTests
    {
        [TestCase(false,false,false,false,0)]
        [TestCase(true,false,false,false,1)]
        [TestCase(false,true,false,false,2)]
        [TestCase(false,false,true,false,4)]
        [TestCase(false,false,false,true,8)]
        [TestCase(true,false,true,false,5)]
        [TestCase(false,true,false,true,10)]
        [TestCase(true,true,false,false,3)]
        [TestCase(false,true,true,false,6)]
        [TestCase(false,false,true,true,12)]
        [TestCase(true,false,false,true,9)]
        [TestCase(true,true,true,false,7)]
        [TestCase(true,true,false,true,11)]
        [TestCase(true,false,true,true,13)]
        [TestCase(false,true,true,true,14)]
        [TestCase(true,true,true,true,15)]
        public void CardinalBitsCoverEveryTopology(bool north,bool east,bool south,bool west,int expected)
        {
            Assert.That(WireConnectivity.Mask(north,east,south,west),Is.EqualTo(expected));
        }
    }
}
