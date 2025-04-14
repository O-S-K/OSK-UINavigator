#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OSK.UI
{
    [CustomEditor(typeof(RootUI))]
    public class UIManagerEditor : Editor
    {
        private RootUI uiManager;
        private void OnEnable() => uiManager = (RootUI)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            if(GUILayout.Button("Create Data UI SO"))
            {
                if(uiManager.listViewSO == null)
                {
                    uiManager.listViewSO = ScriptableObject.CreateInstance<ListViewSO>();
                    string path = "Assets/Resources/View/ListViewSO.asset";
                    if(File.Exists(path))
                    {
                        Debug.LogError("ListViewSO already exists at " + path);
                        return;
                    }
                    if (!Directory.Exists("Assets/Resources/View"))
                    {
                        Directory.CreateDirectory("Assets/Resources/View");
                    } 
                    AssetDatabase.CreateAsset(uiManager.listViewSO, path);
                    AssetDatabase.SaveAssets();
                    EditorUtility.SetDirty(uiManager.listViewSO);
                }
                else
                {
                    Debug.LogError("ListViewSO already exists.");
                }
            }

            if (GUILayout.Button("Select Data UI SO"))
            {
                FindViewDataSOAssets();
            }

            if (GUILayout.Button("Setup Canvas"))
            {
                uiManager.SetupCanvas();
            }

            GUILayout.Space(10);

            // Draw background for the views section
            DrawBackground(Color.green);
            EditorGUILayout.LabelField("--- List Views  ---", EditorStyles.boldLabel);
            DisplayViews();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("--- List Views History ---", EditorStyles.boldLabel);
            ShowListViewHistory();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
         

        private void ShowListViewHistory()
        {
            var views = uiManager.ListViewHistory.ToList();
            if (views.Count == 0) return;
            foreach (var view in views)
            {
                EditorGUILayout.LabelField(view.name);
            }
        } 


        private void FindViewDataSOAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:ListViewSO");
            if (guids.Length == 0)
            {
                Debug.LogError("No ListViewSO found in the project.");
                return;
            }

            var viewData = new List<ListViewSO>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ListViewSO v = AssetDatabase.LoadAssetAtPath<ListViewSO>(path);
                viewData.Add(v);
            }

            if (viewData.Count == 0)
            {
                Debug.LogError("No ListViewSO found in the project.");
            }
            else
            {
                foreach (var v in viewData)
                {
                    Debug.Log("ListViewSO found: " + v.name);
                    Selection.activeObject = v;
                    EditorGUIUtility.PingObject(v);
                }
            }
        }

        private void DrawBackground(Color color)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, color);
        }

        private void DisplayViews()
        {
            if (!Application.isPlaying)
                return;

            List<View> views = uiManager.GetAll(true);
            foreach (var _view in views)
            {
                if(_view == null)
                    continue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(_view.name, GUILayout.Width(400));

                bool isVisible = uiManager.Get(_view).IsShowing;

                GUI.enabled = !isVisible;
                if (GUILayout.Button("Open"))
                {
                    uiManager.Open(_view, null, true);
                }

                GUI.enabled = isVisible;
                if (GUILayout.Button("Hide"))
                {
                    uiManager.Hide(_view);
                }

                if (GUILayout.Button("Delete"))
                {
                    uiManager.Delete(_view);
                }

                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#endif