using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FFTAI
{
    public class PrefabApply
    {
        [InitializeOnLoadMethod]
        static void StartInitializeOnLoadMethod()
        {
            PrefabUtility.prefabInstanceUpdated = delegate (GameObject instance)
            {
                Debug.LogError("待添加的PrefabApply事件");
            };
        }
    }
}