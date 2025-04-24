using UnityEngine;
using OSK.UI;
using UnityEngine.UI;

namespace Example
{
    public class IngameUI : View
    {
        public Button wrongButton;
        public Button winButton;
        public Button backMenuButton;
    
    
        public override void Initialize(RootUI rootUI)
        {
            base.Initialize(rootUI);
            wrongButton.onClick.AddListener(() =>
            {
                var wrong = UINavigator.Open<WrongUI>();
                wrong.SetData(new AlertSetup()
                {
                    message = "Wrong",
                    timeHide = 1f,
                });
            });
        
            winButton.onClick.AddListener(() =>
            {
                UINavigator.Open<WinUI>();
            });
            backMenuButton.onClick.AddListener(() =>
            {
                Hide();
                UINavigator.Open<MenuUI>();
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