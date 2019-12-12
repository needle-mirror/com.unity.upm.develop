using System;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;

namespace Unity.PackageManagerUI.Develop.Editor {
    class UpmPackOperation
    {
        string Source { get; set; }
        string Destination { get; set; }

        Action<string> onOperationSuccess;
        Action<Error> onOperationError;

        PackRequest m_Request;
        
        public UpmPackOperation(IPackageVersion packageVersion, string destination, Action<string> doneCallbackAction, Action<Error> errorCallbackAction = null)
        {
            onOperationError += errorCallbackAction;
            onOperationSuccess += doneCallbackAction;
            Source = packageVersion.packageInfo.resolvedPath;
            Destination = destination;

            m_Request = Client.Pack(Source, destination);
            UnityEditor.EditorApplication.update += OnProgress;
        }

        private void OnProgress()
        {
            if (m_Request.IsCompleted)
            {
                UnityEditor.EditorApplication.update -= OnProgress;
                if (m_Request.Status == StatusCode.Success)
                    onOperationSuccess.Invoke(Destination);
                else if (m_Request.Status >= StatusCode.Failure)
                    onOperationError.Invoke(m_Request.Error);
                else
                    onOperationError.Invoke(new Error(NativeErrorCode.Unknown, "Unsupported progress state " + m_Request.Status));
            }
        }

    }
}