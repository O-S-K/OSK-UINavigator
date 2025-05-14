using System;
using UnityEngine.UI;

namespace OSK.UI
{
    public class AlertSetup
    {
        public string title = "";
        public string message = "";
        public Action onOk = null;
        public Action onCancel = null;
        public float timeHide = 0;
    }

    public class AlertView : View
    {
        public TMPro.TMP_Text title;
        public TMPro.TMP_Text message;
        public Button okButton;
        public Button cancelButton;

        public virtual void SetData(AlertSetup setup)
        {
            SetTile(setup.title);
            SetMessage(setup.message);
            SetOkButton(setup.onOk);
            SetCancelButton(setup.onCancel);
            SetTimeHide(setup.timeHide);
        }

        private void SetTile(string title)
        {
            if (!string.IsNullOrEmpty(title)) 
                this.title.text = title;
        }

        private void SetMessage(string message)
        {
            if (!string.IsNullOrEmpty(message)) 
                this.message.text = message;
        }

        private void SetOkButton(Action onOk)
        {
            if (onOk == null)
            {
                if (okButton != null)
                {
                    okButton.gameObject.SetActive(false);
                }

                return;
            }

            if (okButton != null)
            {
                okButton.onClick.AddListener(() =>
                {
                    onOk?.Invoke();
                    OnClose();
                });
            }
        }

        private void SetCancelButton(Action onCancel)
        {
            if (onCancel == null)
            {
                if (cancelButton != null)
                {
                    cancelButton.gameObject.SetActive(false);
                }

                return;
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(() =>
                {
                    onCancel?.Invoke();
                    OnClose();
                });
            }
        }

        public virtual void SetTimeHide(float time)
        {
            if (time <= 0)
                return;
            Invoke(nameof(OnClose), time);
        }

        private void OnDisable()
        {
            if (okButton != null) okButton.onClick.RemoveAllListeners();
            if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
        }

        public virtual void OnClose()
        {
            Destroy(gameObject);
        }
    }
}