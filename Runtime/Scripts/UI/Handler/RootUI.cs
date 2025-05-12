using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace OSK.UI
{
    [DefaultExecutionOrder(-101)]
    public class RootUI : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] [SerializeField]
        private List<View> _listViewInit = new List<View>();

        [ShowInInspector, ReadOnly] [SerializeField]
        private List<View> _listCacheView = new List<View>();

        private Stack<View> _viewHistory = new Stack<View>();
        public Stack<View> ListViewHistory => _viewHistory;
        public List<View> ListViewInit => _listViewInit;

        public ListViewSO listViewSO;
        [SerializeField] private Camera _uiCamera;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private Transform _viewContainer;

        [SerializeField] private bool isPortrait = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool enableLog = true;

        public Canvas GetCanvas => _canvas;
        public CanvasScaler GetCanvasScaler => _canvasScaler;
        public Camera GetUICamera => _uiCamera;
        public Transform GetViewContainer => _viewContainer;

        private void Awake()
        {
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        public void Initialize()
        { 
            if (listViewSO != null)
            {
                Preload();
            } 
        }

        public void SetupCanvas()
        {
            _canvas.referencePixelsPerUnit = 100;
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            if (isPortrait)
            {
                _canvasScaler.referenceResolution = new Vector2(1080, 1920);
                _canvasScaler.matchWidthOrHeight = 0;
            }
            else
            {
                _canvasScaler.referenceResolution = new Vector2(1920, 1080);
                _canvasScaler.matchWidthOrHeight = 1;
            }

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(this))
            {
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(_canvas);
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(_canvasScaler);

                UnityEditor.EditorUtility.SetDirty(_canvas);
                UnityEditor.EditorUtility.SetDirty(_canvasScaler);
                UnityEditor.EditorUtility.SetDirty(gameObject); // Optional

                Debug.Log($"[SetupCanvas] IsPortrait: {isPortrait} => Saved to prefab instance");
            }
#endif
        }


        #region Init

        private void Preload()
        { 
            if (listViewSO == null)
            {
                Debug.LogError("[View] is null");
                return;
            }

            _listViewInit.Clear();
            _listViewInit = listViewSO.Views.Select(view => view.view).ToList();

            for (int i = 0; i < _listViewInit.Count; i++)
            {
                if (_listViewInit[i].isPreloadSpawn)
                {
                    SpawnViewCache(_listViewInit[i]);
                }
            }
        }

        #endregion

         #region Spawn

        public T Spawn<T>(T view, object[] data, bool hidePrevView) where T : View
        {
            return IsExist<T>() ? Open<T>(data, hidePrevView) : SpawnViewCache(view);
        }

        public T Spawn<T>(string path, object[] data, bool cache, bool hidePrevView) where T : View
        {
            if (IsExist<T>())
            {
                return Open<T>(data, hidePrevView);
            }

            var view = SpawnFromResource<T>(path);
            if (!cache) return view;

            if (_listCacheView.Contains(view))
                _listCacheView.Add(view);

            return view;
        }

        public T SpawnViewCache<T>(T view) where T : View
        {
            var _view = Instantiate(view, _viewContainer);
            _view.gameObject.SetActive(false);
            _view.Initialize(this);

            _view.transform.localPosition = Vector3.zero;
            _view.transform.localScale = Vector3.one;

            LLoger($"[View] Spawn view: {_view.name}", isLog: enableLog);
            if (!_listCacheView.Contains(_view))
                _listCacheView.Add(_view);
            return _view;
        }

        public T SpawnAlert<T>(T view) where T : View
        {
            var _view = Instantiate(view, _viewContainer);
            _view.gameObject.SetActive(false);
            _view.Initialize(this);

            _view.transform.localPosition = Vector3.zero;
            _view.transform.localScale = Vector3.one;

            LLoger($"[View] Spawn Alert view: {_view.name}", isLog: enableLog);
            return _view;
        }

        #endregion

        #region Open

        public View Open(View view, object[] data = null, bool hidePrevView = false, bool checkShowing = true)
        {
            var _view = _listCacheView.FirstOrDefault(v => v == view);
            if (hidePrevView && _viewHistory.Count > 0)
            {
                var prevView = _viewHistory.Peek();
                prevView.Hide();
            }

            if (_view == null)
            {
                var viewPrefab = _listViewInit.FirstOrDefault(v => v == view);
                if (viewPrefab == null)
                {
                    LLoger($"[View] Can't find view prefab for type: {view.GetType().Name}", isLog: enableLog);
                    return null;
                }

                _view = SpawnViewCache(viewPrefab);
            }

            if (_view.IsShowing && checkShowing)
            {
                LLoger($"[View] Opened view IsShowing: {_view.name}", isLog: enableLog);
                return _view;
            }

            _view.Open(data);
            _viewHistory.Push(_view);
            LLoger($"[View] Opened view: {_view.name}", isLog: enableLog);
            return _view;
        }

        public T Open<T>(object[] data = null, bool hidePrevView = false, bool checkShowing = true) where T : View
        {
            var _view = _listCacheView.FirstOrDefault(v => v is T && v != null) as T;
            if (hidePrevView && _viewHistory.Count > 0)
            {
                var prevView = _viewHistory.Peek();
                prevView.Hide();
            }

            if (_view == null)
            {
                var viewPrefab = _listViewInit.FirstOrDefault(v => v is T && v !) as T;
                if (viewPrefab == null)
                {
                    LLoger($"[View] Can't find view prefab for type: {typeof(T).Name}", isLog: enableLog);
                    return null;
                }

                _view = SpawnViewCache(viewPrefab);
            }

            if (_view.IsShowing && checkShowing)
            {
                LLoger($"[View] Opened view: {_view.name}", isLog: enableLog);
                return _view;
            }

            _view.Open(data);
            _viewHistory.Push(_view);
            LLoger($"[View] Opened view: {_view.name}", isLog: enableLog);
            return _view;
        }

        public T TryOpen<T>(object[] data = null, bool hidePrevView = false) where T : View
        {
            return Open<T>(data, hidePrevView, false);
        }

        /// <summary>
        /// Open previous view in history
        /// </summary>
        public View OpenPrevious(object[] data = null, bool isHidePrevPopup = false)
        {
            if (_viewHistory.Count <= 1)
            {
                LLoger("[View] No previous view to open", isLog: enableLog);
                return null;
            }

            // Pop current view
            var currentView = _viewHistory.Pop();

            if (isHidePrevPopup && currentView != null && !currentView.Equals(null))
            {
                try
                {
                    currentView.Hide();
                }
                catch (Exception ex)
                {
                    LLoger($"[View] Error hiding current view: {ex.Message}", isLog: enableLog);
                }
            }

            // Peek previous view
            var previousView = _viewHistory.Peek();
            if (previousView == null || previousView.Equals(null))
            {
                LLoger("[View] Previous view is null or destroyed", isLog: enableLog);
                return null;
            }

            previousView.Open(data);
            LLoger($"[View] Opened previous view: {previousView.name}", isLog: enableLog);
            return previousView;
        }

        /// <summary>
        /// Spawn and open alert view, destroy it when closed
        /// </summary>
        public AlertView OpenAlert<T>(AlertSetup setup) where T : AlertView
        {
            var viewPrefab = _listViewInit.FirstOrDefault(v => v is T) as T;
            if (viewPrefab == null)
            {
                LLoger($"[View] Can't find view prefab for type: {typeof(T).Name}", isLog: enableLog);
                return null;
            }

            var view = SpawnAlert(viewPrefab);
            view.Open();
            view.SetData(setup);
            LLoger($"[View] Opened view: {view.name}", isLog: enableLog);
            return view;
        }

        #endregion

        #region Get

        public View Get(View view, bool isInitOnScene)
        {
            var _view = GetAll(isInitOnScene).Find(x => x == view);
            if (_view == null)
            {
                LLoger($"[View] Can't find view: {view.name}", isLog: enableLog);
                return null;
            }

            if (!_view.isInitOnScene)
            {
                LLoger($"[View] {view.name} is not init on scene", isLog: enableLog);
            }

            return _view;
        }

        public T Get<T>(bool isInitOnScene = true) where T : View
        {
            var _view = GetAll(isInitOnScene).Find(x => x is T) as T;
            if (_view == null)
            {
                LLoger($"[View] Can't find view: {typeof(T).Name}", isLog: enableLog);
                return null;
            }

            if (!_view.isInitOnScene)
            {
                LLoger($"[View] {typeof(T).Name} is not init on scene", isLog: enableLog);
            }

            return _view;
        }

        public View Get(View view)
        {
            var _view = GetAll(true).Find(x => x == view);
            if (_view != null) return _view;

            LLoger($"[View] Can't find view: {view.name}", isLog: enableLog);
            return null;
        }

        public List<View> GetAll(bool isInitOnScene)
        {
            if (isInitOnScene)
                return _listCacheView;

            var views = _listViewInit.FindAll(x => x.isInitOnScene);
            if (views.Count > 0) return views;

            LLoger($"[View] Can't find any view", isLog: enableLog);
            return null;
        }

        #endregion

        #region Hide

        public void Hide(View view)
        {
            if (view == null || !_listCacheView.Contains(view))
            {
                LLoger($"[View] Can't hide: invalid view", isLog: enableLog);
                return;
            }

            if (!view.IsShowing)
            {
                LLoger($"[View] Can't hide: {view.name} is not showing", isLog: enableLog);
                return;
            }

            try
            {
                view.Hide();
            }
            catch (Exception ex)
            {
                LLoger($"[View] Hide failed: {view.name} - {ex.Message}", isLog: enableLog);
            }
        }

        public void HideIgnore<T>() where T : View
        {
            foreach (var view in _listCacheView.ToList())
            {
                if (view == null)
                {
                    LLoger($"[View] {nameof(view)} is null in HideIgnore", isLog: enableLog);
                    continue;
                }

                if (view is T) continue;
                if (!view.IsShowing) continue;

                try
                {
                    view.Hide();
                }
                catch (Exception ex)
                {
                    LLoger($"[View] Error hiding view {view.name}: {ex.Message}", isLog: enableLog);
                }
            }
        }

        public void HideIgnore<T>(T[] viewsToKeep) where T : View
        {
            foreach (var view in _listCacheView.ToList())
            {
                if (view == null)
                {
                    LLoger($"[View] {nameof(view)}  is null in HideIgnore", isLog: enableLog);
                    continue;
                }

                if (view is not T tView || viewsToKeep.Contains(tView)) continue;
                if (!view.IsShowing) continue;

                try
                {
                    view.Hide();
                }
                catch (Exception ex)
                {
                    LLoger($"[View] Error hiding view {view.name}: {ex.Message}", isLog: enableLog);
                }
            }
        }

        public void HideAll()
        {
            var views = _listCacheView.Where(view => view.IsShowing).ToList();
            foreach (var view in views)
            {
                if (view == null)
                {
                    LLoger($"[View] {nameof(view)} is null in HideAll", isLog: enableLog);
                    _listCacheView.Remove(view);
                    continue;
                }

                try
                {
                    view.Hide();
                }
                catch (Exception ex)
                {
                    LLoger($"[View] Error hiding view: {ex.Message}", isLog: enableLog);
                }
            }
        }

        #endregion

        #region Remove

        public void Remove(View view)
        {
            if (view == null || _viewHistory.Count == 0)
                return;

            if (_viewHistory.Peek() == view)
            {
                _viewHistory.Pop();
                Hide(view);
            }
            else
            {
                LLoger($"[View] Can't remove {view.name}: not on top of history", isLog: enableLog);
            }
        }

        public void Remove(bool hidePrevView = false)
        {
            if (_viewHistory.Count <= 0)
                return;

            var curView = _viewHistory.Pop();
            curView.Hide();

            if (hidePrevView)
                OpenPrevious();
        }

        public void RemoveAll()
        {
            while (_viewHistory.Count > 0)
            {
                var curView = _viewHistory.Pop();
                if (curView == null)
                {
                    LLoger($"[View] {nameof(curView)} null view", isLog: enableLog);
                    continue;
                }

                try
                {
                    curView.Hide();
                }
                catch (Exception ex)
                {
                    LLoger($"[View] Error hiding popped view: {ex.Message}", isLog: enableLog);
                }
            }
        }

        #endregion

        #region Delete

        public void Delete<T>(T view, Action action = null) where T : View
        {
            if (!_listCacheView.Contains(view))
                return;

            LLoger($"[View] Delete view: {view.name}", isLog: enableLog);
            _listCacheView.Remove(view);
            Destroy(view.gameObject);
            action?.Invoke();
        }

        #endregion

        #region Sort oder

        public List<View> GetSortedChildPages(Transform container)
        {
            List<View> childPages = new List<View>();
            for (int i = 0; i < container.childCount; i++)
            {
                var childPage = container.GetChild(i).GetComponent<View>();
                if (childPage != null)
                    childPages.Add(childPage);
            }

            return childPages;
        }

        public int FindInsertIndex(List<View> childPages, int depth)
        {
            int left = 0, right = childPages.Count - 1;
            int insertIndex = childPages.Count;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (depth < childPages[mid].depth)
                {
                    insertIndex = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            return insertIndex;
        }

        #endregion

        #region Private

        private T SpawnFromResource<T>(string path) where T : View
        {
            var view = Instantiate(Resources.Load<T>(path), _viewContainer);

            if (view != null)
                return SpawnViewCache(view);
            LLoger($"[View] Can't find popup with path: {path}");
            return null;
        }

        private bool IsExist<T>() where T : View
        {
            return _listCacheView.Exists(x => x is T);
        }

        #endregion

        #region Debug

        public void LogAllViews()
        {
            LLoger($"[View] Total views: {_listCacheView.Count}");
            foreach (var view in _listCacheView)
            {
                LLoger($"[View] View: {view.name} - IsShowing: {view.IsShowing}");
            }

            Debug.Log($"[View] Total views: {_listViewInit.Count}");
            foreach (var view in _listViewInit)
            {
                LLoger($"[View] View: {view.name} - IsShowing: {view.IsShowing}");
            }

            LLoger($"[View] Total views: {_viewHistory.Count}");
            foreach (var view in _viewHistory)
            {
                LLoger($"[View] View: {view.name} - IsShowing: {view.IsShowing}");
            }
        }
        #endregion
        
        private void LLoger(string message, bool isLog = true)
        {
            if (isLog)
                Debug.Log(message);
        }
    }
}