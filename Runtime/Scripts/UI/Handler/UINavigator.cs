using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace OSK.UI
{
    public partial class UINavigator : MonoBehaviour
    {
        #region Singleton
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

        public T Spawn<T>(string path, object[] data = null, bool isCache = true, bool isHidePrevPopup = false)
            where T : View
        {
            return RootUI.Spawn<T>(path, data, isCache, isHidePrevPopup);
        }

        public T SpawnCache<T>(T view, object[] data = null, bool isHidePrevPopup = false) where T : View
        {
            return RootUI.Spawn(view, data, isHidePrevPopup);
        }

        public T Open<T>(object[] data = null, bool isHidePrevPopup = false) where T : View
        {
            return RootUI.Open<T>(data, isHidePrevPopup);
        }

        public void OpenPrevious()
        {
            RootUI.OpenPrevious();
        }

        public T TryOpen<T>(object[] data = null, bool isHidePrevPopup = false) where T : View
        {
            return RootUI.TryOpen<T>(data, isHidePrevPopup);
        }

        public void Open(View view, object[] data = null, bool isHidePrevPopup = false)
        {
            RootUI.Open(view, data, isHidePrevPopup);
        }

        public AlertView OpenAlert<T>(AlertSetup setup) where T : AlertView
        {
            return RootUI.OpenAlert<T>(setup);
        }

        public void Hide(View view)
        {
            RootUI.Hide(view);
        }

        public void HideAll()
        {
            RootUI.HideAll();
        }

        public void HideAllIgnoreView<T>() where T : View
        {
            RootUI.HideIgnore<T>();
        }

        public void HideAllIgnoreView<T>(T[] viewsToKeep) where T : View
        {
            RootUI.HideIgnore(viewsToKeep);
        }

        public void Delete<T>(T popup) where T : View
        {
            RootUI.Delete<T>(popup);
        }


        public T Get<T>(bool isInitOnScene = true) where T : View
        {
            return RootUI.Get<T>(isInitOnScene);
        }

        public T GetOrOpen<T>(object[] data = null, bool hidePrevView = false) where T : View
        {
            var view = RootUI.Get<T>();
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

        public bool IsShowing(View view)
        {
            return RootUI.Get<View>().IsShowing;
        }

        public List<View> GetAll(bool isInitOnScene)
        {
            return RootUI.GetAll(isInitOnScene);
        }

        #endregion
    }
}