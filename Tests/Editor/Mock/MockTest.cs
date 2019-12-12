using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using RunState = UnityEditor.TestTools.TestRunner.Api.RunState;

namespace Unity.PackageManagerUI.Develop.Editor.Tests {
    class MockTest : ITestAdaptor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public int TestCaseCount => Children.Count();
        public bool HasChildren { get; set; }
        public bool IsSuite { get; set; }
        public IEnumerable<ITestAdaptor> Children { get; set; }
        public int TestCaseTimeout { get; set; }
        public ITypeInfo TypeInfo { get; set; }
        public IMethodInfo Method { get; set; }
        public string[] Categories { get; set; }
        public bool IsTestAssembly { get; set; }
        public RunState RunState { get; set; }
        public string Description { get; set; }
        public string SkipReason { get; set; }
        public string ParentId { get; set; }
        public string UniqueName { get; set; }
        public string ParentUniqueName { get; set; }

        public ITestAdaptor Parent { get; set; }

        public string ParentFullName { get; set; }

        public int ChildIndex { get; set; }

        public TestMode TestMode { get; set; }

        public MockTest()
        {
            Name = "MockTest";
            Children = new List<ITestAdaptor>();
            Categories = new string[0];
            IsTestAssembly = true;
            RunState = RunState.Runnable;
        }
        
        static public MockTest CreateSimple()
        {
            return new MockTest();
        }
    }
}