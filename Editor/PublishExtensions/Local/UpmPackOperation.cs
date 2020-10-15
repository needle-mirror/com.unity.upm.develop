using System;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class UpmPackOperation
    {
        private string m_Source { get; set; }
        private string m_Destination { get; set; }

        private Action<string> onOperationSuccess;
        private Action<Error> onOperationError;

        private PackRequest m_Request;

        public UpmPackOperation(IPackageVersion packageVersion, string destination, Action<string> doneCallbackAction, Action<Error> errorCallbackAction = null)
        {
            onOperationError += errorCallbackAction;
            onOperationSuccess += doneCallbackAction;
            m_Source = packageVersion.packageInfo.resolvedPath;
            m_Destination = destination;

            m_Request = Client.Pack(m_Source, destination);
            UnityEditor.EditorApplication.update += OnProgress;
        }

        private void OnProgress()
        {
            if (m_Request.IsCompleted)
            {
                UnityEditor.EditorApplication.update -= OnProgress;
                if (m_Request.Status == StatusCode.Success)
                    onOperationSuccess.Invoke(m_Destination);
                else if (m_Request.Status >= StatusCode.Failure)
                    onOperationError.Invoke(m_Request.Error);
                else
                    onOperationError.Invoke(new Error(NativeErrorCode.Unknown, "Unsupported progress state " + m_Request.Status));
            }
        }
    }
}
