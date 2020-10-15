using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.TestTools.TestRunner.Api;
using TestStatus = UnityEditor.TestTools.TestRunner.Api.TestStatus;

namespace Unity.PackageManagerUI.Develop.Editor.Tests
{
    class MockResult : ITestResultAdaptor
    {
        public ITestAdaptor Test { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string ResultState { get; set; }
        public TestStatus TestStatus { get; set; }
        public double Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public int AssertCount { get; set; }
        public int FailCount { get; set; }
        public int PassCount { get; set; }
        public int SkipCount { get; set; }
        public int InconclusiveCount { get; set; }
        public bool HasChildren { get; set; }
        public IEnumerable<ITestResultAdaptor> Children { get; set; }
        public string Output { get; set; }

        public TNode ToXml() {return null;}
    }
}