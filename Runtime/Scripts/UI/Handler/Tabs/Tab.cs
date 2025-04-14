using UnityEngine;

namespace OSK.UI
{
    public abstract class Tab : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract void Active();
        public abstract void DeActive();
    }
}