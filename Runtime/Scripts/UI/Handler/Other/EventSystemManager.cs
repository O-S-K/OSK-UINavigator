using UnityEngine;
using UnityEngine.EventSystems; 

namespace OSK.UI
{
    [DefaultExecutionOrder(1000)]
    public class EventSystemManager : MonoBehaviour
    {
        private void Awake()
        {
            EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();

            switch (eventSystems.Length)
            {
                case > 1:
                {
                    for (int i = 1; i < eventSystems.Length; i++)
                    {
                        Destroy(eventSystems[i].gameObject);
                    }

                    Debug.LogWarning(
                        "There are more than one EventSystem in the scene. Destroying all except the first one.");
                    break;
                }
                case 0:
                    Debug.LogError("EventSystem not found in the scene. Adding a new one.");
                    break;
            } 
        }
    }
}