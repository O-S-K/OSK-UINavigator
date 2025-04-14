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
            if (IsExist<T>())
                return Open<T>(data, hidePrevView);

            return SpawnViewCache(view);
        }

        public T Spawn<T>(string path, object[] data, bool isCache, bool hidePrevView) where T : View
        {
            if (IsExist<T>())
            {
                return Open<T>(data, hidePrevView);
            }

            var view = SpawnFromResource<T>(path);
            if (isCache)
            {
                if (_listCacheView.Contains(view))
                    _listCacheView.Add(view);
            }

            return view;
        }

        public T SpawnViewCache<T>(T _view) where T : View
        {
            var view = Instantiate(_view, _viewContainer);
            view.gameObject.SetActive(false);
            view.Initialize(this);

            view.transform.localPosition = Vector3.zero;
            view.transform.localScale = Vector3.one;

            Debug.Log($"[View] Spawn view: {view.name}");
            if (!_listCacheView.Contains(view))
                _listCacheView.Add(view);
            return view;
        }

        public T SpawnAlert<T>(T _view) where T : View
        {
            var view = Instantiate(_view, _viewContainer);
            view.gameObject.SetActive(false);
            view.Initialize(this);

            view.transform.localPosition = Vector3.zero;
            view.transform.localScale = Vector3.one;

            Debug.Log($"[View] Spawn Alert view: {view.name}");
            return view;
        }

        #endregion

        #region Open

        public View Open(View _view, object[] data = null, bool hidePrevView = false, bool checkShowing = true)
        {
            var view = _listCacheView.FirstOrDefault(v => v == _view);
            if (hidePrevView && _viewHistory.Count > 0)
            {
                var prevView = _viewHistory.Peek();
                prevView.Hide();
            }

            if (view == null)
            {
                var viewPrefab = _listViewInit.FirstOrDefault(v => v == _view);
                if (viewPrefab == null)
                {
                    Debug.LogError($"[View] Can't find view prefab for type: {_view.GetType().Name}");
                    return null;
                }

                view = SpawnViewCache(viewPrefab);
            }

            if (view.IsShowing && checkShowing)
            {
                return view;
            }

            view.Open(data);
            _viewHistory.Push(view);
            Debug.Log($"[View] Opened view: {view.name}");
            return view;
        }

        public T Open<T>(object[] data = null, bool hidePrevView = false, bool checkShowing = true) where T : View
        {
            var view = _listCacheView.FirstOrDefault(v => v is T) as T;
            if (hidePrevView && _viewHistory.Count > 0)
            {
                var prevView = _viewHistory.Peek();
                prevView.Hide();
            }

            if (view == null)
            {
                var viewPrefab = _listViewInit.FirstOrDefault(v => v is T) as T;
                if (viewPrefab == null)
                {
                    Debug.LogError($"[View] Can't find view prefab for type: {typeof(T).Name}");
                    return null;
                }

                view = SpawnViewCache(viewPrefab);
            }

            if (view.IsShowing && checkShowing)
            {
                return view;
            }

            view.Open(data);
            _viewHistory.Push(view);
            Debug.Log($"[View] Opened view: {view.name}");
            return view;
        }

        public T TryOpen<T>(object[] data = null, bool hidePrevView = false) where T : View
        {
            return Open<T>(data, hidePrevView, false);
        }

        public void OpenPrevious(object[] data = null)
        {
            if (_viewHistory.Count > 0)
            {
                var prevView = _viewHistory.Pop();
                prevView.Open(data);
                Debug.Log($"[View] Open previous view: {prevView.name}");
            }
        }

        public AlertView OpenAlert<T>(AlertSetup setup) where T : AlertView
        {
            var viewPrefab = _listViewInit.FirstOrDefault(v => v is T) as T;
            if (viewPrefab == null)
            {
                Debug.LogError($"[View] Can't find view prefab for type: {typeof(T).Name}");
                return null;
            }

            var view = SpawnAlert(viewPrefab);
            view.Open();
            view.SetData(setup);
            Debug.Log($"[View] Opened view: {view.name}");
            return view;
        }

        #endregion

        #region Get

        public View Get(View _view, bool isInitOnScene = true)
        {
            var view = GetAll(isInitOnScene).Find(x => x == _view);
            if (view == null)
            {
                Debug.LogError($"[View] Can't find view: {_view.name}");
                return null;
            }

            if (!view.isInitOnScene)
            {
                Debug.LogError($"[View] {_view.name} is not init on scene");
            }

            return view;
        }

        public T Get<T>(bool isInitOnScene = true) where T : View
        {
            var view = GetAll(isInitOnScene).Find(x => x is T) as T;
            if (view == null)
            {
                Debug.LogError($"[View] Can't find view: {typeof(T).Name}");
                return null;
            }

            if (!view.isInitOnScene)
            {
                Debug.LogError($"[View] {typeof(T).Name} is not init on scene");
            }

            return view;
        }

        public View Get(View _view)
        {
            var view = GetAll(true).Find(x => x == _view);
            if (view == null)
            {
                Debug.LogError($"[View] Can't find view: {_view.name}");
                return null;
            }

            return view;
        }

        public List<View> GetAll(bool isInitOnScene)
        {
            if (isInitOnScene)
                return _listCacheView;

            var views = _listViewInit.FindAll(x => x.isInitOnScene);
            if (views.Count <= 0)
            {
                Debug.LogError($"[View] Can't find any view");
                return null;
            }

            return views;
        }

        #endregion

        #region Hide

        public void Hide(View view)
        {
            if (_viewHistory.Count <= 0) return;
            if (_viewHistory.Peek() == view)
            {
                Remove();
            }
        }

        public void HideIgnore<T>() where T : View
        {
            foreach (var view in _listCacheView)
            {
                if (view is T)
                    continue;
                if (view.IsShowing)
                {
                    view.Hide();
                }
            }
        }

        public void HideIgnore<T>(T[] viewsToKeep) where T : View
        {
            foreach (var view in _listCacheView)
            {
                if (view is T && !viewsToKeep.Contains(view as T))
                {
                    if (view.IsShowing)
                    {
                        view.Hide();
                    }
                }
            }
        }

        public void HideAll()
        {
            var views = _listCacheView.Where(view => view.IsShowing).ToList();
            foreach (var view in views)
            {
                view.Hide();
            }
        }

        #endregion

        #region Remove

        public void RemovePopup(View view)
        {
            if (_viewHistory.Count <= 0) return;
            if (_viewHistory.Peek() == view)
            {
                Remove();
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
                curView.Hide();
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
            Debug.LogError($"[Popup] Can't find popup with path: {path}");
            return null;
        }

        private bool IsExist<T>() where T : View
        {
            return _listCacheView.Exists(x => x is T);
        }

        #endregion
    }
}