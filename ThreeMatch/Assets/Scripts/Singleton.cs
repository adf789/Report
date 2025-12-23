using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonParent : MonoBehaviour
{

}
public class Singleton<T> : SingletonParent where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject[] allObejct = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var obj in allObejct)
                {
                    instance = obj.GetComponentInChildren<T>();
                    if (instance != null)
                    {
                        break;
                    }
                }
            }
            return instance;
        }
    }
}
