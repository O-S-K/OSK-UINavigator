using UnityEngine;
using OSK.UI;
using TMPro;
using UnityEngine.UI;

namespace Example
{
    public class IngameUI : View
    {
        public Button wrongButton => GetRef<Button>("WrongButton");
        public Button winButton => GetRef<Button>("WinButton");
        public Button menuButton => GetRef<Button>("MenuButton");
        public TextMeshProUGUI tileText => GetRef<TextMeshProUGUI>("TileText");
        
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
            menuButton.onClick.AddListener(() =>
            {
                Hide();
                UINavigator.Open<MenuUI>();
            });
        }

        public override void Open(object[] data = null)
        {
            base.Open(data);
            tileText.text = "Ingame";
        }
    

        public override void Hide()
        {
            base.Hide();
        }
    }
}