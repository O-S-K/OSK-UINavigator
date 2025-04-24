using System;
using OSK.UI;
using UnityEngine;

namespace Example
{
    public class Run : MonoBehaviour
    {
        private void Start()
        {
            UINavigator.Open<MenuUI>();
        }
    }
}
