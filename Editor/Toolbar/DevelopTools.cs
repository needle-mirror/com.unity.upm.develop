using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using System.Threading;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class DevelopTools : IWindowCreatedHandler, IPackageSelectionChangedHandler
    {
        private const string k_PackagesFolder = "Packages";

        private IPackageActionMenu m_DevelopMenu;
        private IPackageActionDropdownItem m_EmbedDropdown;
        private IPackageActionDropdownItem m_CloneDropdown;

        public void OnWindowCreated(WindowCreatedArgs args)
        {
            m_DevelopMenu = args.window.AddPackageActionMenu();
            m_DevelopMenu.text = L10n.Tr("Develop");
            m_DevelopMenu.visible = false;

            m_EmbedDropdown = m_DevelopMenu.AddDropdownItem();
            m_EmbedDropdown.text = L10n.Tr("Copy to Packages folder");
            m_EmbedDropdown.action = EmbedClicked;

            m_CloneDropdown = m_DevelopMenu.AddDropdownItem();
            m_CloneDropdown.text = L10n.Tr("Clone from git repo");
            m_CloneDropdown.action =  CloneClicked;
            m_CloneDropdown.visible = Unsupported.IsDeveloperMode();
        }

        public void OnPackageSelectionChanged(PackageSelectionArgs args)
        {
            var packageInfo = PackageInfoHelper.GetPackageInfo(args.package?.name);
            if (packageInfo == null)
            {
                m_DevelopMenu.visible = false;
                return;
            }
            
            m_DevelopMenu.visible = CanBeEmbedded(args.packageVersion, packageInfo);
        }

        private static bool CanBeEmbedded(IPackageVersion version, PackageInfo packageInfo)
        {
            return version != null && version.isInstalled && packageInfo.source == PackageSource.Registry && packageInfo.isDirectDependency;
        }

        private void EmbedClicked(PackageSelectionArgs args)
        {
            if (CanBeEmbedded(args.packageVersion, PackageInfoHelper.GetPackageInfo(args.package.name)))
            {
                Client.Embed(args.packageVersion.name);
            }
            else
            {
                //TODO: Update error message since it's misleading
                UnityEngine.Debug.LogError(L10n.Tr("This package is not installed. Please install it before you can develop it."));
            }
        }

        private StringBuilder m_ReadBuffer;
        private StreamReader m_StreamReader;

        private void FetchOutputWindow()
        {
            for (; ; )
            {
                var textLine = m_StreamReader.ReadLine();
                if (textLine == null)
                    break;
                m_ReadBuffer.Append(textLine);
            }
        }

        private void CloneClicked(PackageSelectionArgs args)
        {
            var packageInfo = PackageInfoHelper.GetPackageInfo(args.package.name);
            var packageVersion = args.packageVersion;
            EditorUtility.DisplayProgressBar($"{L10n.Tr("Cloning")} {packageVersion.displayName}", $"{packageInfo.repository.url}", 1.0f);

            var process = new Process();
            process.StartInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";
            process.StartInfo.FileName = "git";
            process.StartInfo.WorkingDirectory = k_PackagesFolder;
#if UNITY_2023_1_OR_NEWER
            process.StartInfo.Arguments = string.Format("clone {0} {1}", packageInfo.repository.url, packageVersion.uniqueId);
#else
            process.StartInfo.Arguments = string.Format("clone {0} {1}", packageInfo.repository.url, packageVersion.packageUniqueId);
#endif
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            if (!process.Start())
            {
                m_DevelopMenu.enabled = true;
                UnityEngine.Debug.LogError(L10n.Tr("Couldn't start a new process to launch git."));
                return;
            }

            m_ReadBuffer = new StringBuilder();
            m_StreamReader = process.StandardError;

            var t = new Thread(FetchOutputWindow) { IsBackground = true };
            t.Start();

            EditorApplication.delayCall += () =>
            {
                process.WaitForExit();
                EditorUtility.ClearProgressBar();

                t.Join();

                if (process.ExitCode != 0)
                {
                    m_DevelopMenu.enabled = true;
                    UnityEngine.Debug.LogError(m_ReadBuffer.ToString());
                    return;
                }
                AssetDatabase.Refresh();
            };
        }
    }
}
