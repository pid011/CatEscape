using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CatEscape.UI
{
    public class TabNavigation : MonoBehaviour
    {
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab))
            {
                return;
            }

            var system = EventSystem.current;
            var curObj = system.currentSelectedGameObject;
            GameObject nextObj = null;
            if (!curObj)
            {
                nextObj = system.firstSelectedGameObject;
            }
            else
            {
                var curSelect = curObj.GetComponent<Selectable>();
                var nextSelect =
                    Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                        ? curSelect.FindSelectableOnUp()
                        : curSelect.FindSelectableOnDown();
                if (nextSelect)
                {
                    nextObj = nextSelect.gameObject;
                }
            }
            if (nextObj)
            {
                system.SetSelectedGameObject(nextObj, new BaseEventData(system));
            }
        }
    }
}
