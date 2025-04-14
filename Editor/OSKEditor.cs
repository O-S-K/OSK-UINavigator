using System.IO;
using System.Linq;
using OSK.UI;
using UnityEditor;
using UnityEngine;

namespace OSK.UI
{
    public class OSKEditor : MonoBehaviour
    {
        [MenuItem("OSK-Framework/Create UINavigator")]
        public static void CreateHUDOnScene()
        {
            if (FindObjectOfType<RootUI>() == null)
            {
                PrefabUtility.InstantiatePrefab(Resources.LoadAll<RootUI>("").First());
            }
        }
          
        [MenuItem("OSK-Framework/Install Dependencies/Dotween")]
        public static void InstallDependenciesDotween()
        {
            AddPackage("https://github.com/O-S-K/DOTween.git");
        }

        [MenuItem("OSK-Framework/Install Dependencies/UIFeel")]
        public static void InstallDependenciesUIFeel()
        {
            AddPackage("https://github.com/O-S-K/UIFeel.git");
        }

        private static void AddPackage(string packageName)
        {
            UnityEditor.PackageManager.Client.Add(packageName);
            UnityEditor.EditorUtility.DisplayDialog("OSK-Framework", "Package added successfully", "OK");
            UnityEditor.AssetDatabase.Refresh();
        }
    }
}