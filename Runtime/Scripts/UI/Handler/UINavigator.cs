using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace OSK.UI
{
    public partial class UINavigator : MonoBehaviour
    {
        #region Singleton
        // singleton on scene
        public static UINavigator Instance;
        #endregion

        [SerializeField] private RootUI _rootUI;
        public Canvas GetCanvas => _rootUI.GetCanvas;
        public Camera GetUICamera => _rootUI.GetUICamera;
        public bool autoInit = true;

        public RootUI RootUI
        {
            get
            {
                if (_rootUI == null)
                {
                    Debug.LogError("RootUI is null. Please check the initialization of the UIManager.");
                    return null;
                }
                return _rootUI;
            }
        }

        private void Awake()
        {
            Instance = this;
            if (autoInit) OnInit();
        }

        public void OnInit()
        {
            if(_rootUI == null)
                _rootUI = FindObjectOfType<RootUI>();
            if (_rootUI != null)
                _rootUI.Initialize();
        }
 
        #region Views

        public static T Spawn<T>(string path, object[] data = null, bool isCache = true, bool isHidePrevPopup = false)
            where T : View
        {
            return Instance.RootUI.Spawn<T>(path, data, isCache, isHidePrevPopup);
        }

        public static T SpawnCache<T>(T view, object[] data = null, bool isHidePrevPopup = false) where T : View
        {
            return Instance.RootUI.Spawn(view, data, isHidePrevPopup);
        }

        public static T Open<T>(object[] data = null, bool isHidePrevPopup = false) where T : View
        {
            return Instance.RootUI.Open<T>(data, isHidePrevPopup);
        }

        public static void OpenPrevious()
        {
            Instance.RootUI.OpenPrevious();
        }

        public static T TryOpen<T>(object[] data = null, bool isHidePrevPopup = false) where T : View
        {
            return Instance.RootUI.TryOpen<T>(data, isHidePrevPopup);
        }

        public static void Open(View view, object[] data = null, bool isHidePrevPopup = false)
        {
            Instance.RootUI.Open(view, data, isHidePrevPopup);
        }

        public static AlertView OpenAlert<T>(AlertSetup setup) where T : AlertView
        {
            return Instance.RootUI.OpenAlert<T>(setup);
        }

        public static void Hide(View view)
        {
            Instance.RootUI.Hide(view);
        }

        public static void HideAll()
        {
            Instance.RootUI.HideAll();
        }

        public static void HideAllIgnoreView<T>() where T : View
        {
            Instance.RootUI.HideIgnore<T>();
        }

        public static void HideAllIgnoreView<T>(T[] viewsToKeep) where T : View
        {
            Instance.RootUI.HideIgnore(viewsToKeep);
        }

        public static void Delete<T>(T popup) where T : View
        {
            Instance.RootUI.Delete<T>(popup);
        }


        public static T Get<T>(bool isInitOnScene = true) where T : View
        {
            return Instance.RootUI.Get<T>(isInitOnScene);
        }

        public static T GetOrOpen<T>(object[] data = null, bool hidePrevView = false) where T : View
        {
            var view = Instance.RootUI.Get<T>();
            if (view == null)
            {
                if (!view.IsShowing)
                    view = Open<T>(data, hidePrevView);
            }
            else
            {
                if (!view.IsShowing)
                    view.Open(data);
            }

            return view;
        }

        public static bool IsShowing(View view)
        {
            return Instance.RootUI.Get<View>().IsShowing;
        }

        public static List<View> GetAll(bool isInitOnScene)
        {
            return Instance.RootUI.GetAll(isInitOnScene);
        }

        #endregion
    }
}