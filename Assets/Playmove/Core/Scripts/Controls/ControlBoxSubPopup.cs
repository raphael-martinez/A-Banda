using UnityEngine;

namespace Playmove.Core.Controls
{
    public class ControlBoxSubPopup : MonoBehaviour
    {
        public static Openable CurrentSubPopup { get; private set; }

        public static void CloseIfAny()
        {
            if (CurrentSubPopup != null && CurrentSubPopup.IsOpen)
                CurrentSubPopup.Close();
            CurrentSubPopup = null;
        }

        public void ToggleSubPopup(Openable subPopup)
        {
            if (CurrentSubPopup == subPopup)
            {
                CloseIfAny();
                return;
            }

            if (CurrentSubPopup != null)
                CurrentSubPopup.Close();
            CurrentSubPopup = subPopup;
            CurrentSubPopup.Open();
        }
    }
}
