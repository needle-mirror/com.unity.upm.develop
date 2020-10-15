using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.TestTools;

namespace Unity.PackageManagerUI.Develop.Editor.Tests
{
    public class CreatePackageTests
    {
        private const string k_TestPackageName = "com.unity.my-package";
        private const string k_TestPackageNameCase = "Com.Unity.MyPackage";
        private const string k_TestPackageNameGeneric = "name";
        private const string k_TestPackageNameExisting = "com.unity.upm.develop";
        private readonly List<string> m_TestPackageNames = new List<string> {k_TestPackageName, k_TestPackageNameCase, k_TestPackageNameGeneric};

        private bool m_SkipCleanup;

        static string PackagePath(string packageName)
        {
            return $"Packages/{packageName}";
        }

        [UnitySetUp]
        public IEnumerator EnsureNoCompilationLeakedByPreviousTest()
        {
            while (EditorApplication.isCompiling)
                yield return new WaitForDomainReload();
        }

        [UnityTearDown]
        public IEnumerator EnsureNoCompilationLeakedByThisTest()
        {
            while (EditorApplication.isCompiling)
                yield return new WaitForDomainReload();
        }

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(PackagePath(k_TestPackageNameExisting)))
            {
                m_SkipCleanup = true;
                throw new Exception($"{k_TestPackageNameExisting} should exist in the current project for tests to work.");
            }

            foreach (var packageName in m_TestPackageNames)
            {
                var createdPackagePath = $"Packages/{k_TestPackageName}";
                if (Directory.Exists(createdPackagePath))
                {
                    m_SkipCleanup = true;
                    throw new Exception("This package name currently exists and should be reserved for tests");
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Don't cleanup packages as they might be actual user packages.
            if (m_SkipCleanup)
                return;

            foreach (var packageName in m_TestPackageNames)
            {
                var createdPackagePath = $"Packages/{k_TestPackageName}";
                if (Directory.Exists(createdPackagePath))
                    Directory.Delete(createdPackagePath, true);
            }
        }

        private static IEnumerable<TestCaseData> ParameterValidationSource()
        {
            var defaultTemplatePath = Path.Combine(EditorApplication.applicationContentsPath, "Resources/PackageManager/PackageTemplates/default");

            // Param order: name, displayName, rootNamespace, templateFolder, expected error message
            yield return new TestCaseData("", "display name", "rootnamespace", defaultTemplatePath, "name is required");
            yield return new TestCaseData("  ", "display name", "rootnamespace", defaultTemplatePath, "name is required");
            yield return new TestCaseData(k_TestPackageNameCase, "display name", "rootnamespace", defaultTemplatePath, "Package name [Com.Unity.MyPackage] is invalid");

            // Hard-coding "local.test.references" is brittle, but for some reason, if we use Linq in this method (to fetch "any" package name),
            // it somehow confuses the test runner when running the SerializationTests.TestResultWhenFetchedAfterSerialization test with a "Non-static method requires a target." error.
            yield return new TestCaseData(k_TestPackageNameExisting, "displayName", "rootnamespace", defaultTemplatePath, string.Format("The project already contains a package with the name [{0}].", k_TestPackageNameExisting));

            yield return new TestCaseData(k_TestPackageNameGeneric, "", "rootnamespace", defaultTemplatePath, "displayName is required");
            yield return new TestCaseData(k_TestPackageNameGeneric, "  ", "rootnamespace", defaultTemplatePath, "displayName is required");

            yield return new TestCaseData(k_TestPackageNameGeneric, "displayName", "1MyPackage.Tests", defaultTemplatePath, "[1MyPackage.Tests] is not a valid namespace");

            yield return new TestCaseData(k_TestPackageNameGeneric, "display name", "rootnamespace", "", "templateFolder is required");
            yield return new TestCaseData(k_TestPackageNameGeneric, "display name", "rootnamespace", "  ", "templateFolder is required");
            var nonExistentTemplatePath = Path.Combine(EditorApplication.applicationContentsPath, "Resources/PackageManager/PackageTemplates/non/existent/path");
            yield return new TestCaseData(k_TestPackageNameGeneric, "display name", "rootnamespace", nonExistentTemplatePath, string.Format("The template folder [{0}] does not exist", nonExistentTemplatePath));
        }

        [Test, TestCaseSource(nameof(ParameterValidationSource))]
        public void TestParametersAreValidated(string name, string displayName, string rootNamespace, string templateFolder, string expectedMessage)
        {
            var options = new PackageTemplateOptions()
            {
                name = name,
                displayName = displayName,
                rootNamespace = rootNamespace,
                templateFolder = templateFolder
            };

            try
            {
                PackageTemplate.CreatePackage(options);
                Assert.Fail("Expected PackageTemplate.CreatePackage to throw");
            }
            catch (Exception e)
            {
                Assert.AreEqual($"options parameter is invalid:{Environment.NewLine}{expectedMessage}", e.Message);
            }
        }

        [Test]
        public void TestValidationErrorsAreConcatenated()
        {
            var options = new PackageTemplateOptions();

            try
            {
                PackageTemplate.CreatePackage(options);
                Assert.Fail("Expected PackageTemplate.CreatePackage to throw");
            }
            catch (Exception e)
            {
                Assert.AreEqual($"options parameter is invalid:{Environment.NewLine}name is required{Environment.NewLine}displayName is required", e.Message);
            }
        }
    }
}
