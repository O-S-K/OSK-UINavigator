using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace OSK.UI
{
    public class View : MonoBehaviour
    {
        [Header("Settings")] [EnumToggleButtons]
        public EViewType viewType = EViewType.Popup;

        public int depth;

        [Space] [ToggleLeft] public bool isAddToViewManager = true;
        [ToggleLeft] public bool isPreloadSpawn = true;
        [ToggleLeft] public bool isRemoveOnHide = false;

        [ReadOnly] [ToggleLeft] public bool isInitOnScene;
        public bool IsShowing => _isShowing;

        [ShowInInspector, ReadOnly] [ToggleLeft]
        private bool _isShowing;

        private UITransition _uiTransition;
        private RootUI _rootUI;

        [Space] [ToggleLeft] public bool isShowEvent = false;
        [ShowIf(nameof(isShowEvent))] public UnityEvent EventAfterInit;
        [ShowIf(nameof(isShowEvent))] public UnityEvent EventBeforeOpened;
        [ShowIf(nameof(isShowEvent))] public UnityEvent EventAfterOpened;
        [ShowIf(nameof(isShowEvent))] public UnityEvent EventBeforeClosed;
        [ShowIf(nameof(isShowEvent))] public UnityEvent EventAfterClosed;

        [Button]
        public void AddUITransition()
        {
            _uiTransition = gameObject.GetOrAddComponent<UITransition>();
            Debug.Log("UITransition added.");
        }

        [Button]
        public void AddShield()
        {
            if (transform.Find("Shield") != null)
            {
                Debug.LogWarning("Shield already exists.");
                return;
            }

            GameObject go = new GameObject("Shield");
            go.layer = LayerMask.NameToLayer("UI");
            RectTransform shield = go.AddComponent<RectTransform>();
            shield.SetParent(transform, false);
            shield.transform.SetAsFirstSibling();
            shield.pivot = new Vector2(0.5f, 0.5f);
            shield.localPosition = Vector3.zero;
            shield.localScale = Vector3.one;
            shield.sizeDelta = Vector2.zero;
            shield.anchorMin = Vector2.zero;
            shield.anchorMax = Vector2.one;
            shield.pivot = new Vector2(0.5f, 0.5f);

            UnityEngine.UI.Image img = go.GetOrAddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.9f);
        }

        public virtual void Initialize(RootUI rootUI)
        {
            if (isInitOnScene) return;

            isInitOnScene = true;
            _rootUI = rootUI;

            _uiTransition = GetComponent<UITransition>();
            _uiTransition?.Initialize();

            if (_rootUI == null)
            {
                Debug.LogError("RootUI is still null after initialization.");
            }

            SetDepth(depth);
            EventAfterInit?.Invoke();
        }

        public void SetDepth(EViewType viewType, int depth)
        {
            this.viewType = viewType;
            SetDepth(depth);
        }

        private void SetDepth(int depth)
        {
            /*var canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = viewType switch
                {
                    EViewType.None => (0 + canvas.sortingOrder),
                    EViewType.Popup => (1000 + canvas.sortingOrder),
                    EViewType.Overlay => (10000 + canvas.sortingOrder),
                    EViewType.Screen => (-1000 + canvas.sortingOrder),
                    _ => canvas.sortingOrder
                };
            }
            else*/
            {
                var childPages = _rootUI.GetSortedChildPages(_rootUI.GetViewContainer);
                if (childPages.Count == 0)
                    return;
                
                depth = viewType switch
                {
                    EViewType.None => (0 + depth),
                    EViewType.Popup => (1000 + depth),
                    EViewType.Overlay => (10000 + depth),
                    EViewType.Screen => (-1000 + depth),
                    _ => depth
                };

                var insertIndex = _rootUI.FindInsertIndex(childPages, depth);
                if (insertIndex == childPages.Count) transform.SetAsLastSibling();
                else transform.SetSiblingIndex(insertIndex);
            }
        }


        public virtual void Open(object[] data = null)
        {
            if (!IsViewContainerInitialized() || IsAlreadyShowing()) return;

            _isShowing = true;
            EventBeforeOpened?.Invoke();
            gameObject.SetActive(true);

            SetDepth(depth);

            if (_uiTransition != null)
                _uiTransition.OpenTrans(() => EventAfterOpened?.Invoke());
            else EventAfterOpened?.Invoke();
        }

        public virtual void Hide()
        {
            if (!_isShowing) return;

            _isShowing = false;
            EventBeforeClosed?.Invoke();

            if (_uiTransition != null)
                _uiTransition.CloseTrans(FinalizeHide);
            else FinalizeHide();
        }

        public void CloseImmediately()
        {
            _isShowing = false;

            if (_uiTransition != null) _uiTransition.AnyClose(FinalizeImmediateClose);
            else FinalizeImmediateClose();
        }

        protected bool IsViewContainerInitialized()
        {
            if (_rootUI == null)
            {
                Debug.LogError("View Manager is null. Ensure that the View has been initialized before calling Open.");
                return false;
            }

            return true;
        }

        protected bool IsAlreadyShowing()
        {
            if (_isShowing)
            {
                Debug.LogWarning("[View] View is already showing");
                return true;
            } 
            return false;
        }

        protected void FinalizeHide()
        {
            gameObject.SetActive(false);
            EventAfterClosed?.Invoke();

            if (isRemoveOnHide) 
                _rootUI.Delete(this); 
        }

        protected void FinalizeImmediateClose()
        {
            gameObject.SetActive(false); 
        }

        public virtual void Delete()
        {
            _rootUI.Delete(this);
        }
    }
}