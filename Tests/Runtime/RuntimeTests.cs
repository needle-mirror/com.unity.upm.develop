using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.PackageManagerUI.Develop.Runtime.Tests
{
    public class RuntimeTests
    {
        // Make sure there is at least one playmode test to make sure that all tests will be run.
        [Test]
        public void SupportsPlayModeTests()
        {
            Assert.IsTrue(true);
        }
    }
}
