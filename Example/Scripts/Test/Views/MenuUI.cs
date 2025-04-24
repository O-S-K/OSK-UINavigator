using UnityEngine;
using OSK.UI;
using UnityEngine.UI;

namespace Example
{
    
    public class MenuUI : View
    {
        public Button ingameButton;
    
        public override void Initialize(RootUI rootUI)
        {
            base.Initialize(rootUI);
            ingameButton.onClick.AddListener(() =>
            {
                Hide();
                UINavigator.Open<IngameUI>();
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