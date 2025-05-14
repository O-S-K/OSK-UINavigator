using UnityEngine;
using OSK.UI;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace Example
{
    
    public class MenuUI : View
    {
        public Button playButton => GetRef<Button>("PlayButton");
    
        public override void Initialize(RootUI rootUI)
        {
            base.Initialize(rootUI);
            playButton.onClick.AddListener(() =>
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