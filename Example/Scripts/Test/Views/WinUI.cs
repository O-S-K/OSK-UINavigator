using UnityEngine;
using OSK.UI;
using Unity.VisualScripting;
using UnityEngine.UI;

namespace Example
{
    public class WinUI : View
    {
        public Button closeButton;
        public Button wrongButton;

        public override void Initialize(RootUI rootUI)
        {
            base.Initialize(rootUI);
            closeButton.onClick.AddListener(Hide);
            wrongButton.onClick.AddListener(() =>
            {
                var wrong = UINavigator.Open<WrongUI>();
                wrong.SetData(new AlertSetup()
                {
                    message = "Get reward",
                    timeHide = 1f,
                });

            });
        }

        public override void Open(object[] data = null)
        {
            base.Open(data);
        }

        public override void Hide()
        {
            base.Hide();
        }
    }
}