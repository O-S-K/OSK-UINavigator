using OSK.UI;
using TMPro;
using UnityEngine.UI;

namespace Example
{
    public class WinUI : View
    {
        public Button closeButton => GetRef<Button>("CloseButton");
        public Button rewardButton => GetRef<Button>("RewardButton");
        public TextMeshProUGUI tileText => GetRef<TextMeshProUGUI>("TileText");

        

        public override void Initialize(RootUI rootUI)
        {
            base.Initialize(rootUI); 
            closeButton.onClick.AddListener(() =>
            {
                Hide();
            });
            rewardButton.onClick.AddListener(() =>
            {
                UINavigator.OpenAlert<WrongUI>( new AlertSetup()
                {
                    message = "Reward",
                    timeHide = 1f,
                });
            });
        }

        public override void Open(object[] data = null)
        {
            base.Open(data);
            tileText.text = "Win";
        }

        public override void Hide()
        {
            
            base.Hide();
        }
    }
}