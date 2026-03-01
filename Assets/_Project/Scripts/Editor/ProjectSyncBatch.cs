using System;
using System.Reflection;
using UnityEditor;

namespace AscendantContinuum.Editor
{
    public static class ProjectSyncBatch
    {
        public static void RegenerateProjectFiles()
        {
            AssetDatabase.Refresh();

            Type syncType = Type.GetType("UnityEditor.SyncVS,UnityEditor");
            MethodInfo syncMethod = syncType?.GetMethod("SyncSolution", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (syncMethod == null)
            {
                throw new MissingMethodException("UnityEditor.SyncVS.SyncSolution method not found.");
            }

            syncMethod.Invoke(null, null);
            AssetDatabase.Refresh();
        }
    }
}
