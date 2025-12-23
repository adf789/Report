using UnityEngine;

public struct AsyncOperationCallback
{
    public AsyncOperation AsyncOperation;
    public System.Action Callback;

    public AsyncOperationCallback(AsyncOperation asyncOperation, System.Action callback)
    {
        AsyncOperation = asyncOperation;
        Callback = callback;
    }
}
