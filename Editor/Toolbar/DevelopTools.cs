using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements.Debugger;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class DevelopTools : VisualElement
    {
        private DropdownButton m_DevelopButton;
        private IPackageVersion m_PackageVersion;
        private IPackage m_Package;

        public DevelopTools()
        {
            m_DevelopButton = new DropdownButton();
            m_DevelopButton.text = L10n.Tr("Develop");
            m_DevelopButton.visible = false;

            Add(m_DevelopButton);
        }

        private static bool CanBeEmbedded(IPackageVersion version)
        {
            return version != null && version.isInstalled && version.packageInfo?.source == PackageSource.Registry && version.packageInfo.isDirectDependency;
        }

        private void DevelopClick()
        {
            var details = panel.GetRootVisualElement().Q<PackageDetails>("packageDetails");

            if (CanBeEmbedded(m_PackageVersion))
            {
                details.detailError.ClearError();
                Client.Embed(m_PackageVersion.name);
                details.RefreshPackageActionButtons();
            }
            else
            {
                details.detailError.SetError(new UIError(UIErrorCode.NotFound, L10n.Tr("This package is not installed. Please install it before you can develop it.")));
            }
        }

        private StringBuilder m_ReadBuffer;
        private StreamReader m_StreamReader;
        private Stream m_Stream;

        private void fetchOutputWindow()
        {
            for (; ; )
            {
                var textLine = m_StreamReader.ReadLine();
                if (textLine == null)
                    break;
                m_ReadBuffer.Append(textLine);
            }
        }

        private void CloneClick()
        {
            m_DevelopButton.SetEnabled(false);

            var details = panel.GetRootVisualElement().Q<PackageDetails>("packageDetails");

            EditorUtility.DisplayProgressBar($"{L10n.Tr("Cloning")} {m_PackageVersion.displayName}", $"{m_PackageVersion.packageInfo.repository.url}", 1.0f);

            var process = new Process();
            process.StartInfo.FileName = "git";
            process.StartInfo.WorkingDirectory = Folders.GetPackagesPath();
            process.StartInfo.Arguments = string.Format("clone {0}", m_PackageVersion.packageInfo.repository.url);
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            if (!process.Start())
            {
                m_DevelopButton.SetEnabled(true);
                details.detailError.SetError(new UIError(UIErrorCode.NotFound, L10n.Tr("Couldn't start a new process to launch git.")));
                details.RefreshPackageActionButtons();
                return;
            }

            m_ReadBuffer = new StringBuilder();
            m_StreamReader = process.StandardError;

            var t = new Thread(fetchOutputWindow) { IsBackground = true };
            t.Start();

            EditorApplication.delayCall += () =>
            {
                process.WaitForExit();
                EditorUtility.ClearProgressBar();

                t.Join();

                if (process.ExitCode != 0)
                {
                    m_DevelopButton.SetEnabled(true);
                    details.detailError.SetError(new UIError(UIErrorCode.NotFound, m_ReadBuffer.ToString()));
                    details.RefreshPackageActionButtons();
                    return;
                }

                details.detailError.ClearError();
                details.RefreshPackageActionButtons();

                AssetDatabase.Refresh();
            };
        }

        private void RefreshDevelopButton()
        {
            var visibleFlag = CanBeEmbedded(m_PackageVersion);
            if (visibleFlag)
            {
                var enableButton = !EditorApplication.isCompiling && !PackageDatabase.instance.isInstallOrUninstallInProgress && m_PackageVersion?.packageInfo != null;
                m_DevelopButton.SetEnabled(enableButton);
                if (Unsupported.IsDeveloperMode() && m_PackageVersion?.packageInfo?.repository?.url?.Length > 0 && enableButton)
                {
                    var menu = new GenericMenu();
                    CreateDevelopDropdown(menu, L10n.Tr("Copy to Packages folder"), DevelopClick);
                    CreateDevelopDropdown(menu, L10n.Tr("Clone from git repo"), CloneClick);
                    m_DevelopButton.dropdownMenu = menu;
                    m_DevelopButton.clickable.clicked += m_DevelopButton.OnDropdownButtonClicked;
                }
                else
                {
                    m_DevelopButton.dropdownMenu = null;
                    m_DevelopButton.clickable.clicked += DevelopClick;
                }

            }

            UIUtils.SetElementDisplay(m_DevelopButton, visibleFlag);
        }

        private void CreateDevelopDropdown(GenericMenu menu, string labelText, GenericMenu.MenuFunction func)
        {
            var viewReportItem = new GUIContent(labelText);
            menu.AddItem(viewReportItem, false, func);
        }

        public void SetPackage(IPackage package, IPackageVersion version)
        {
            m_Package = package;
            m_PackageVersion = version;
            RefreshDevelopButton();
        }
    }
}
