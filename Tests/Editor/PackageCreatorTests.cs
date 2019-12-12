using System;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.PackageCreatorTests1
{
    // Empty namespace used for tests
    public static class EmptyClass
    {
        // Nothing
    }
}

/* This Test still needs to be ported
        [TestCase(false)]
        [TestCase(true)]
        public void Show_Or_Hide_Develop_Button_By_CanBeEmbedded(bool canBeEmbedded)
        {
            m_MockPackage.Setup(p => p.installedVersion).Returns(m_MockVersion.Object);
            m_MockVersion.Setup(v => v.isInstalled).Returns(true);
            m_MockVersion.Setup(v => v.canBeEmbedded).Returns(canBeEmbedded);
            m_PackageDetails.SetPackage(m_MockPackage.Object, m_MockVersion.Object);

            Assert.AreEqual(m_PackageDetails.developButton.visible, canBeEmbedded);
        }
 */
namespace Unity.PackageManagerUI.Develop.Editor.Tests
{
    public class PackageCreatorTests
    {
        static readonly string k_ValidSpecialChars = "'~!@#$%^&;+=(){}[]";
        static int s_NameLoopReplacement = 5;
        List<string> m_FoldersToDelete;
        string defaultPackage;            // Package expected to always exists. Used for naming tests.

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

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Make sure we are not logged in
            if (MockUnityConnect.loggedIn)
                MockUnityConnect.Logout();
            
            try
            {
                defaultPackage = CreateTestPackage();
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.Log("Package Creator Tests package already exists. Skipping creation. " + e.Message);
            }
        }
        
        [SetUp]
        public void Setup()
        {
            m_FoldersToDelete = new List<string>();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (!string.IsNullOrEmpty(defaultPackage))
            {
                DeleteFolder(defaultPackage);
                AssetDatabase.Refresh();
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            if (m_FoldersToDelete.Count == 0)
                return;

            MockAssetDatabase.CloseCachedFiles();

            foreach (var folder in m_FoldersToDelete)
            {
                DeleteFolder(folder);
            }

            AssetDatabase.Refresh();
        }

        static void DeleteFolder(string folder)
        {
            try
            {
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        private static IEnumerable<TestCaseData> CreateTemplateOptionsTestSource()
        {
            // Param order: tested displayName, organization, expected displayName, expected name, expected root namespace

            // Test whitespaces
            yield return new TestCaseData(string.Empty, string.Empty, "Undefined Package", "com.undefined.undefinedpackage", "Undefined.UndefinedPackage");
            yield return new TestCaseData(string.Empty, "My Organization", "Undefined Package", "com.myorganization.undefinedpackage", "MyOrganization.UndefinedPackage");
            yield return new TestCaseData(string.Empty, "My    Organization", "Undefined Package", "com.myorganization.undefinedpackage", "MyOrganization.UndefinedPackage");
            yield return new TestCaseData("My Package", string.Empty, "My Package", "com.undefined.mypackage", "Undefined.MyPackage");
            yield return new TestCaseData("My    Package", string.Empty, "My    Package", "com.undefined.mypackage", "Undefined.MyPackage");
            yield return new TestCaseData("New\tPackage", "New Organization", "New\tPackage", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New\rPackage", "New Organization", "New\rPackage", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "New\tOrganization", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "New\rOrganization", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");

            // Test invalid special characters
            foreach (var specialChar in MockEditorUtility.GetInvalidFilenameChars())
            {
                yield return new TestCaseData($"New{specialChar}Package", "New Organization", "New_Package", "com.neworganization.new_package", "NewOrganization.NewPackage");
            }

            // Test valid special characters: ' ~ ! @ # $ % ^ & ( ) + = { } [ ] ;
            foreach (var specialChar in k_ValidSpecialChars.ToCharArray())
            {
                yield return new TestCaseData($"New{specialChar}Package", "New Organization", $"New{specialChar}Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            }

            // Test different display names
            yield return new TestCaseData("New Package", "New Organization", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package $$$", "New Organization", "New Package $$$", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("新しいテストパッケーシ", "New Organization", "新しいテストパッケーシ", "com.neworganization.undefinedpackage", "NewOrganization.UndefinedPackage");
            yield return new TestCaseData("001", "New Organization", "001", "com.neworganization.undefinedpackage", "NewOrganization.UndefinedPackage");
            yield return new TestCaseData("0", "New Organization", "0", "com.neworganization.undefinedpackage", "NewOrganization.UndefinedPackage");
            yield return new TestCaseData("0001", "New Organization", "0001", "com.neworganization.undefinedpackage", "NewOrganization.UndefinedPackage");
            yield return new TestCaseData("0.0001", "New Organization", "0.0001", "com.neworganization.undefinedpackage", "NewOrganization.UndefinedPackage");
            yield return new TestCaseData("0,0001", "New Organization", "0,0001", "com.neworganization.undefinedpackage", "NewOrganization.UndefinedPackage");
            yield return new TestCaseData("0-00.1", "New Organization", "0-00.1", "com.neworganization.undefinedpackage", "NewOrganization.UndefinedPackage");
            yield return new TestCaseData("4.95E-10", "New Organization", "4.95E-10", "com.neworganization.e-10", "NewOrganization.E10");
            yield return new TestCaseData("-4.95E-10", "New Organization", "-4.95E-10", "com.neworganization.e-10", "NewOrganization.E10");

            // Test different organization names
            yield return new TestCaseData("New Package", "New Organization $$$", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "我的组织", "New Package", "com.undefined.newpackage", "Undefined.NewPackage");
            yield return new TestCaseData("New Package", "001", "New Package", "com.undefined.newpackage", "Undefined.NewPackage");
            yield return new TestCaseData("New Package", "0", "New Package", "com.undefined.newpackage", "Undefined.NewPackage");
            yield return new TestCaseData("New Package", "0.0001", "New Package", "com.undefined.newpackage", "Undefined.NewPackage");
            yield return new TestCaseData("New Package", "0,0001", "New Package", "com.undefined.newpackage", "Undefined.NewPackage");
            yield return new TestCaseData("New Package", "0-00.1", "New Package", "com.undefined.newpackage", "Undefined.NewPackage");
            yield return new TestCaseData("New Package", "0Test", "New Package", "com.test.newpackage", "Test.NewPackage");
            yield return new TestCaseData("New Package", "0  Test", "New Package", "com.test.newpackage", "Test.NewPackage");
            yield return new TestCaseData("New Package", "4.95E-10", "New Package", "com.e-10.newpackage", "E10.NewPackage");
            yield return new TestCaseData("New Package", "-4.95E-10", "New Package", "com.e-10.newpackage", "E10.NewPackage");

            // Test dots
            yield return new TestCaseData(".New Package", "New Organization", ".New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package.", "New Organization", "New Package.", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData(".New Package.", "New Organization", ".New Package.", "com.neworganization.newpackage", "NewOrganization.NewPackage");

            yield return new TestCaseData("New Package", ".New Organization", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "New Organization.", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", ".New Organization.", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");

            yield return new TestCaseData("New.Package", "New Organization", "New.Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New..Package", "New Organization", "New..Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "New.Organization", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");

            yield return new TestCaseData("New Package", "New..Organization", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New.Package", "New.Organization", "New.Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New..Package", "New..Organization", "New..Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package...", "New Organization", "New Package...", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "New Organization...", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");

            // Test dash
            yield return new TestCaseData("New-Package", "New Organization", "New-Package", "com.neworganization.new-package", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package-", "New Organization", "New Package-", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("-New Package", "New Organization", "-New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("-New Package-", "New Organization", "-New Package-", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "New-Organization", "New Package", "com.new-organization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "New Organization-", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "-New Organization", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "-New Organization-", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");

            // Test underscore
            yield return new TestCaseData("New_Package", "New Organization", "New_Package", "com.neworganization.new_package", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package_", "New Organization", "New Package_", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("_New Package", "New Organization", "_New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("_New Package_", "New Organization", "_New Package_", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "New_Organization", "New Package", "com.new_organization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "New Organization_", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "_New Organization", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");
            yield return new TestCaseData("New Package", "_New Organization_", "New Package", "com.neworganization.newpackage", "NewOrganization.NewPackage");

            // Test specific display names and organization names
            yield return new TestCaseData("新しいテストパッケーシ", "我的组织", "新しいテストパッケーシ", "com.undefined.undefinedpackage", "Undefined.UndefinedPackage");
            yield return new TestCaseData("0001", "0001", "0001", "com.undefined.undefinedpackage", "Undefined.UndefinedPackage");
            yield return new TestCaseData("0.0001", "0.0001", "0.0001", "com.undefined.undefinedpackage", "Undefined.UndefinedPackage");

            // Test package exists: package "Package Creator Tests" already exists, should use "Package Creator Tests 1", "com.unity.packagecreatortests1" and "Unity.PackageCreatorTests2"
            yield return new TestCaseData("Package Creator Tests", "Unity", "Package Creator Tests 1", "com.unity.packagecreatortests1", "Unity.PackageCreatorTests2");

            // Test name more than kMaxPackageNameLength characters
            var longName = new string('x', PackageCreator.k_MaxPackageNameLength);
            var expectedPackageName = $"com.myorganization.{longName}".Substring(0, PackageCreator.k_MaxPackageNameLength);
            yield return new TestCaseData(longName, "My Organization", longName, expectedPackageName, $"MyOrganization.{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(longName)}");

            expectedPackageName = $"com.{longName}.mypackage".Substring(0, PackageCreator.k_MaxPackageNameLength);
            yield return new TestCaseData("My Package", longName, "My Package", expectedPackageName, $"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(longName)}.MyPackage");
        }

        [Test, TestCaseSource(nameof(CreateTemplateOptionsTestSource))]
        public void TestCreateTemplateOptions(string displayName, string organization, string expectedDisplayName, string expectedName, string expectedRootNamespace)
        {
            var options = PackageCreator.CreatePackageTemplateOptions(displayName, organization);

            Assert.AreEqual(expectedDisplayName, options.displayName);
            Assert.AreEqual(expectedName, options.name);
            Assert.AreEqual(expectedRootNamespace, options.rootNamespace);
        }

        [Test]
        public void TestCreateTemplateOptions_WithExistingDisplayNameAndNamespace()
        {
            var options = PackageCreator.CreatePackageTemplateOptions("Package Creator Tests", "Unity");

            // Package Creator Tests already exists, it should take Package Creator Tests 1
            Assert.AreEqual("Package Creator Tests 1", options.displayName);
            Assert.AreEqual("com.unity.packagecreatortests1", options.name);
            // Unity.Packageutilitytests1 already exists, it should take Unity.PackageCreatorTests2
            Assert.AreEqual("Unity.PackageCreatorTests2", options.rootNamespace);
        }

        private static string CreateTestPackage(int index)
        {
            return CreateTestPackage(index.ToString());
        }

        private static string CreateTestPackage(string suffix = "")
        {
            var options = new PackageTemplateOptions()
            {
                name = $"com.unity.packagecreatortests{suffix}",
                displayName = $"Package Creator Tests {suffix}".Trim(),
                rootNamespace = $"Unity.PackageCreatorTests{suffix}"
            };
            return PackageTemplate.CreatePackage(options);
        }

        [Ignore("Instability with created package tear down process, will be looked into in PAX-950")]
        [UnityTest]
        public IEnumerator TestCreateTemplateOptions_WithExistingPackageName_EndsWithDigits()
        {
            for (var i = 1; i < 10; i++)
            {
                m_FoldersToDelete.Add(CreateTestPackage(i));
            }

            AssetDatabase.Refresh();

            // Wait for asset db refresh
            yield return null;

            var options = PackageCreator.CreatePackageTemplateOptions("Package Creator Tests", "Unity");

            Assert.AreEqual("Package Creator Tests 10", options.displayName);
            Assert.AreEqual("com.unity.packagecreatortests10", options.name);
            Assert.AreEqual("Unity.PackageCreatorTests10", options.rootNamespace);
        }

        [Ignore("Instability with created package tear down process, will be looked into in PAX-950")]
        [UnityTest]
        public IEnumerator TestCreateTemplateOptions_WithExistingPackageName_EndsWithDigits_MissingNumber()
        {
            for (var i = 1; i < 10; i++)
            {
                if (i != 4)
                    m_FoldersToDelete.Add(CreateTestPackage(i));
            }

            AssetDatabase.Refresh();

            // Wait for asset db refresh
            yield return null;

            var options = PackageCreator.CreatePackageTemplateOptions("Package Creator Tests", "Unity");

            Assert.AreEqual("Package Creator Tests 4", options.displayName);
            Assert.AreEqual("com.unity.packagecreatortests4", options.name);
            Assert.AreEqual("Unity.PackageCreatorTests4", options.rootNamespace);
        }

        [Ignore("Instability with created package tear down process, will be looked into in PAX-950")]
        [UnityTest]
        public IEnumerator TestCreateTemplateOptions_WithExistingPackageName_EndsWithDigits_NoMoreNumberAvailable_ThrowsArgumentException()
        {
            var field = typeof(PackageCreator).GetField("s_MaxNameLoop", BindingFlags.Static | BindingFlags.NonPublic);
            var previousValue = field.GetValue(null);
            field.SetValue(null, s_NameLoopReplacement);

            for (var i = 1; i <= s_NameLoopReplacement; i++)
            {
                m_FoldersToDelete.Add(CreateTestPackage(i));
            }

            AssetDatabase.Refresh();

            // Wait for asset db refresh
            yield return null;

            Assert.Catch<ArgumentException>(() => PackageCreator.CreatePackageTemplateOptions("Package Creator Tests", "Unity"));
            field.SetValue(null, previousValue);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\r")]
        [TestCase("New Package")]
        [TestCase("NotInPackages/")]
        [TestCase("NotInPackages/New Package")]
        public void TestCreatePackage_WithInvalidPath_ThrowsArgumentException(string path)
        {
            Assert.Catch<ArgumentException>(() => PackageCreator.CreatePackage(path));
        }

        [UnityTest]
        public IEnumerator TestCreatePackage()
        {
            var path = PackageCreator.CreatePackage("Packages/Test Package");
            m_FoldersToDelete.Add(path);

            // Wait for asset db refresh
            yield return null;

            Assert.AreEqual("Packages/com.undefined.testpackage", path);
            Assert.IsTrue(Directory.Exists(path));
        }
    }
}
