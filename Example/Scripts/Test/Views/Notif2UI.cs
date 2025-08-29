using OSK.UI;
using TMPro;
using UnityEngine.UI;

namespace Example
{
    public class Notif2UI : View
    {
        public Button closeButton => GetRef<Button>("CloseButton"); 
        public override void Initialize(RootUI rootUI)
        {
            base.Initialize(rootUI); 
            closeButton.onClick.AddListener(() =>
            {
                Hide();
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