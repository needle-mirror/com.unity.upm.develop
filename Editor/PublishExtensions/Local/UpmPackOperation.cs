using System;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class UpmPackOperation
    {
        private string m_Source { get; set; }
        private string m_Destination { get; set; }

        private Action<string> onOperationSuccess;
        private Action<string> onOperationError;

        private PackRequest m_Request;

        public UpmPackOperation(PackageInfo packageInfo, string destination, Action<string> doneCallbackAction,
            Action<string> errorCallbackAction = null)
        {
            onOperationError += errorCallbackAction;
            onOperationSuccess += doneCallbackAction;
            m_Source = packageInfo.resolvedPath;
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
                    onOperationError.Invoke(m_Request.Error.message);
                else
                    onOperationError.Invoke("Unsupported progress state " + m_Request.Status);
            }
        }
    }
}