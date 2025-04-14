
using DG.Tweening;
using UnityEngine;
using OSK.UI;

namespace Example
{
    public class WrongUI : AlertView
    {
        public override void Initialize(RootUI rootUI)
        {
            base.Initialize(rootUI);
        }

        public override void Open(object[] data = null)
        {
            base.Open(data);
            message.transform.localPosition = new Vector3(0, 0, 0);
            message.transform.DOLocalMoveY(250f, 0.5f).SetEase(Ease.OutBack);
        }

        public override void Hide()
        {
            base.Hide();
        }
    }
}