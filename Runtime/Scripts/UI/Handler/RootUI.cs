using System;
using System.Collections;
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
        private class QueuedView
        {
            public View view;
            public object[] data;
            public bool hidePrevView;
            public Action<View> onOpened;
        }

        #region Lists

        [BoxGroup("🔍 Views", ShowLabel = true)] 
        [ShowInInspector, ReadOnly] [SerializeField]
        private List<View> _listViewInit = new();

        [BoxGroup("🔍 Views")] [ShowInInspector, ReadOnly] [SerializeField]
        private List<View> _listCacheView = new();

        [HideInInspector] // Ẩn Stack nếu không cần xem
        private Stack<View> _viewHistory = new();

        [ShowInInspector, BoxGroup("🔍 Views")]
        public Stack<View> ListViewHistory => _viewHistory;

        [ShowInInspector, BoxGroup("🔍 Views")]
        public List<View> ListCacheView => _listCacheView;

        [ShowInInspector, BoxGroup("🔍 Views")]
        public List<View> ListViewInit => _listViewInit;

        [ShowInInspector, ReadOnly] private List<QueuedView> _queuedViews = new List<QueuedView>();

        [ShowInInspector, ReadOnly] private bool _isProcessingQueue = false;

        #endregion

        #region References

        public ListViewSO listViewSO;

        [Title("📌 References")] [Required, BoxGroup("📌 References")] [SerializeField]
        private Camera _uiCamera;

        [Required, BoxGroup("📌 References")] [SerializeField]
        private Canvas _canvas;

        [Required, BoxGroup("📌 References")] [SerializeField]
        private CanvasScaler _canvasScaler;

        [BoxGroup("📌 References")] [SerializeField]
        private Transform _viewContainer;

        #endregion

        #region Settings

        [Title("⚙️ Settings")] [BoxGroup("⚙️ Settings")] [SerializeField]
        private bool isPortrait = true;

        [BoxGroup("⚙️ Settings")] [SerializeField]
        private bool dontDestroyOnLoad = true;

        [BoxGroup("⚙️ Settings")] [SerializeField]
        private bool isUpdateRatioScaler = true;

        [BoxGroup("⚙️ Settings")] [SerializeField]
        private bool enableLog = true;

        #endregion

        public Canvas GetCanvas => _canvas;
        public CanvasScaler GetCanvasScaler => _canvasScaler;
        public Camera GetUICamera => _uiCamera;
        public Transform GetViewContainer => _viewContainer;

        public bool IsPortrait => isPortrait;
        public bool EnableLog => enableLog;

        public void Initialize()
        {
            if (listViewSO != null)
            {
                Preload();
            }
        }

        public void SetupCanvasScaleForRatio()
        { 
            float ratio = (float)Screen.width / Screen.height;
            if (IsIpad())
            {
                // For iPad, use MatchWidthOrHeight = 0 to maintain aspect ratio
                GetCanvasScaler.matchWidthOrHeight = 0f;
            }
            else
            {
                // For other devices, use MatchWidthOrHeight = 1 if the aspect ratio is wider than 0.65f
                GetCanvasScaler.matchWidthOrHeight = ratio > 0.65f ? 1 : 0;
            }
            
            string log = Mathf.Approximately(GetCanvasScaler.matchWidthOrHeight, 1f) ? "1 (Match Width)" : "0 (Match Height)";
            Debug.Log($"Ratio: {ratio}. IsPad {IsIpad()} matchWidthOrHeight: {log}");
        }
        
        public  bool IsIpad()
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            if (UnityEngine.iOS.Device.generation.ToString().Contains("iPad"))
                return true;
#endif

            float w = Screen.width;
            float h = Screen.height;

            // Normalize to portrait
            if (w > h) (w, h) = (h, w);

            // Aspect ratio check (iPad thường ~ 4:3 → ~1.33)
            return (h / w) < 1.65f;
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
                UnityEditor.EditorUtility.SetDirty(gameObject);
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

            Debug.Log($"[View] Spawn view: {_view.name}");
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

            Debug.Log($"[View] Spawn Alert view: {_view.name}");
            return _view;
        }

        #endregion

        #region Open

        public View Open(View view, object[] data = null, bool hidePrevView = false, bool checkShowing = true)
        {
            var _view = _listCacheView.FirstOrDefault(v => v.GetType() == view.GetType());
            if (hidePrevView && _viewHistory.Count > 0)
            {
                var prevView = _viewHistory.Peek();
                prevView.Hide();
            }

            if (_view == null)
            {
                var viewPrefab = _listViewInit.FirstOrDefault(v => v.GetType() == view.GetType());
                if (viewPrefab == null)
                {
                    Debug.LogError($"[View] Can't find view prefab for type: {view.GetType().Name}");
                    return null;
                }

                _view = SpawnViewCache(viewPrefab);
            }

            if (_view.IsShowing && checkShowing)
            {
                Debug.Log($"[View] Opened view IsShowing: {_view.name}");
                return _view;
            }

            _view.Open(data);
            _viewHistory.Push(_view);
            Debug.Log($"[View] Opened view: {_view.name}");
            return _view;
        }

        public T Open<T>(object[] data = null, bool hidePrevView = false, bool checkShowing = true) where T : View
        {
            var _view = _listCacheView.FirstOrDefault(v => v.GetType() == typeof(T)) as T;
            if (hidePrevView && _viewHistory.Count > 0)
            {
                var prevView = _viewHistory.Peek();
                prevView.Hide();
            }

            if (_view == null)
            {
                var viewPrefab = _listViewInit.FirstOrDefault(v => v.GetType() == typeof(T)) as T;
                if (viewPrefab == null)
                {
                    Debug.LogError($"[View] Can't find view prefab for type: {typeof(T).Name}");
                    return null;
                }

                _view = SpawnViewCache(viewPrefab);
            }

            if (_view.IsShowing && checkShowing)
            {
                Debug.Log($"[View] Opened view: {_view.name}");
                return _view;
            }

            _view.Open(data);
            _viewHistory.Push(_view);
            Debug.Log($"[View] Opened view: {_view.name}");
            return _view;
        }

        public T TryOpen<T>(object[] data = null, bool hidePrevView = false) where T : View
        {
            return Open<T>(data, hidePrevView, false);
        }
        
        public void OpenAddStack(View view, object[] data = null, bool hidePrevView = false)
        {
            _queuedViews.Add(new QueuedView
            {
                view = view,
                data = data,
                hidePrevView = hidePrevView
            });

            // Sort the queue by priority
            _queuedViews = _queuedViews.OrderByDescending(q => q.view.depth).ToList();

            if (!_isProcessingQueue)
            {
                StartCoroutine(ProcessQueue());
            }
        }
        
        public void OpenAddStack<T>(object[] data = null, bool hidePrev = false, Action<T> onOpened = null) where T : View
        {
            var _view = _listCacheView.FirstOrDefault(v => v is T) as T;

            if (_view == null)
            {
                var prefab = _listViewInit.FirstOrDefault(v => v is T) as T;
                if (prefab == null)
                {
                    Debug.LogError($"[OpenAddStack<{typeof(T).Name}>] Not found view prefab for type: {typeof(T).Name}");
                    return;
                }

                _view = SpawnViewCache(prefab);
            }

            var queued = new QueuedView
            {
                view = _view,
                data = data,
                hidePrevView = hidePrev,
                onOpened = v => onOpened?.Invoke(v as T)
            };

            _queuedViews.Add(queued);
 
            // Allways sort the queue by priority
            if (!_isProcessingQueue)
                StartCoroutine(ProcessQueue());
        }

        
        private IEnumerator ProcessQueue()
        {
            _isProcessingQueue = true;

            while (_queuedViews.Count > 0)
            {
                // Get the next view in the queue that is not showing
                var next = _queuedViews
                    .Where(q => q.view != null && !q.view.IsShowing)
                    .OrderByDescending(q => q.view.Priority) //  Sort by priority
                    .FirstOrDefault();

                if (next == null)
                {
                    //  All views are already showing or null, wait for next frame
                    yield return null;
                    continue;
                }
                 
                var openedView = Open(next.view, next.data, next.hidePrevView);
                next.onOpened?.Invoke(openedView);

                // Wait  until the view is closed
                yield return new WaitUntil(() => next.view == null || !next.view.IsShowing);
                // Remove view from queue
                _queuedViews.Remove(next);
            }

            _isProcessingQueue = false;
        }
        
        /// <summary>
        /// Open previous view in history
        /// </summary>
        public View OpenPrevious(object[] data = null, bool isHidePrevPopup = false)
        {
            if (_viewHistory.Count <= 1)
            {
                Debug.LogWarning("[View] No previous view to open");
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
                    Debug.LogError($"[View] Error hiding current view: {ex.Message}");
                }
            }

            // Peek previous view
            var previousView = _viewHistory.Peek();
            if (previousView == null || previousView.Equals(null))
            {
                Debug.LogWarning("[View] Previous view is null or destroyed");
                return null;
            }

            previousView.Open(data);
            Debug.Log($"[View] Opened previous view: {previousView.name}");
            return previousView;
        }

        /// <summary>
        /// Spawn and open alert view, destroy it when closed
        /// </summary>
        public AlertView OpenAlert<T>(AlertSetup setup) where T : AlertView
        {
            var viewPrefab = _listViewInit.FirstOrDefault(v => v.GetType() == typeof(T)) as T;
            if (viewPrefab == null)
            {
                Debug.LogError($"[View] Can't find view prefab for type: {typeof(T).Name}");
                return null;
            }

            var view = SpawnAlert(viewPrefab);
            view.Open(new object[] { setup });
            Debug.Log($"[View] Opened view: {view.name}");
            return view;
        }

        #endregion

        #region Get

        public View Get(View view, bool isInitOnScene)
        {
            var _view = GetAll(isInitOnScene).Find(x => x == view);
            if (_view == null)
            {
                Debug.LogError($"[View] Can't find view: {view.name}");
                return null;
            }

            if (!_view.isInitOnScene)
            {
                Debug.LogError($"[View] {view.name} is not init on scene");
            }

            return _view;
        }

        public T Get<T>(bool isInitOnScene = true) where T : View
        {
            var _view = GetAll(isInitOnScene).Find(x => x is T) as T;
            if (_view == null)
            {
                Debug.LogError($"[View] Can't find view: {typeof(T).Name}");
                return null;
            }

            if (!_view.isInitOnScene)
            {
                Debug.LogError($"[View] {typeof(T).Name} is not init on scene");
            }

            return _view;
        }

        public View Get(View view)
        {
            var _view = GetAll(true).Find(x => x == view);
            if (_view != null)
            {
                Debug.Log($"[View] Found view: {_view.name} is showing {_view.IsShowing}");
                return _view;
            }

            Debug.LogError($"[View] Can't find view: {view.name}");
            return null;
        }

        public List<View> GetAll(bool isInitOnScene)
        {
            if (isInitOnScene) // check if the view is already initialized
                return _listCacheView;

            var views = _listViewInit.FindAll(x => x.isInitOnScene);
            if (views.Count > 0)
            {
                Debug.Log($"[View] Found {views.Count} views");
                return views;
            }

            Debug.LogError($"[View] Can't find any view");
            return null;
        }

        #endregion

        #region Hide

        public void Hide(View view)
        {
            if (view == null || !_listCacheView.Contains(view))
            {
                Debug.LogError($"[View] Can't hide: invalid view");
                return;
            }

            if (!view.IsShowing)
            {
                Debug.Log($"[View] Can't hide: {view.name} is not showing");
                return;
            }

            try
            {
                view.Hide();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[View] Hide failed: {view.name} - {ex.Message}");
            }
        }

        public void HideIgnore<T>() where T : View
        {
            foreach (var view in _listCacheView.ToList())
            {
                if (view == null)
                {
                    Debug.Log($"[View] {nameof(view)} is null in HideIgnore");
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
                    Debug.LogError($"[View] Error hiding view {view.name}: {ex.Message}");
                }
            }
        }

        public void HideIgnore<T>(T[] viewsToKeep) where T : View
        {
            foreach (var view in _listCacheView.ToList())
            {
                if (view == null)
                {
                    Debug.Log($"[View] {nameof(view)}  is null in HideIgnore");
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
                    Debug.LogError($"[View] Error hiding view {view.name}: {ex.Message}");
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
                    Debug.LogError($"[View] {nameof(view)} is null in HideAll");
                    _listCacheView.Remove(view);
                    continue;
                }

                try
                {
                    view.Hide();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[View] Error hiding view: {ex.Message}");
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
                Debug.LogWarning($"[View] Can't remove {view.name}: not on top of history");
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
                    Debug.LogWarning($"[View] {nameof(curView)} null view");
                    continue;
                }

                try
                {
                    curView.Hide();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[View] Error hiding popped view: {ex.Message}");
                }
            }
        }

        #endregion

        #region Delete

        public void Delete<T>(T view, Action action = null) where T : View
        {
            if (!_listCacheView.Contains(view))
                return;

            Debug.Log($"[View] Delete view: {view.name}");
            _listCacheView.Remove(view);
            action?.Invoke();
            Destroy(view.gameObject);
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
            Debug.LogError($"[View] Can't find popup with path: {path}");
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
            Debug.Log($"[View] Total views: {_listCacheView.Count}");
            foreach (var view in _listCacheView)
            {
                Debug.Log($"[View] View: {view.name} - IsShowing: {view.IsShowing}");
            }

            Debug.Log($"[View] Total views: {_listViewInit.Count}");
            foreach (var view in _listViewInit)
            {
                Debug.Log($"[View] View: {view.name} - IsShowing: {view.IsShowing}");
            }

            Debug.Log($"[View] Total views: {_viewHistory.Count}");
            foreach (var view in _viewHistory)
            {
                Debug.Log($"[View] View: {view.name} - IsShowing: {view.IsShowing}");
            }
        }

        #endregion
    }
}