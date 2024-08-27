using Playmove.Core;
using Playmove.Core.API;
using Playmove.Core.Bundles;
using Playmove.Core.Controls;
using Playmove.Framework;
using Playmove.Framework.Popups;
using TouchScript;
using UnityEngine;

public class Inactivity : MonoBehaviour
{
    private int _timer;
    private Popup _popup;

    public void GetInactiveTime()
    {
        PlaytableAPI.GetInactiveTime(result =>
        {
            if (result.Data > 0)
            {
                GameSettings.InactiveTime = 60 * result.Data;
                TouchManager.Instance.TouchesBegan += (sender, args) =>
                {
                    ResetTimer();
                };
                ResetTimer();
            }
        });
    }

    private void AskForLeave()
    {
        if (Playmove.Avatars.API.AvatarAPI.IsAvatarLock) return;

        ControlBoxSubPopup.CloseIfAny();
        Fader.FadeTo(0.75f, 0.5f);
        _timer = 30;
        _popup = Popup.Open(Localization.GetAsset<string>(AssetsCatalog.string_Attention), string.Format(Localization.GetAsset<string>(AssetsCatalog.string_Close_Game_After), "30"),
            new PopupButton(Localization.GetAsset<string>(AssetsCatalog.string_Cancel)) { Action = _ => ResetTimer() },
            new PopupButton(Localization.GetAsset<string>(AssetsCatalog.string_Fechar)) { Action = _ => Playtable.Instance.ForceExit() },
            _ => Fader.FadeTo(0, 0.5f));
        Invoke("StartCountdown", 1);
    }

    public void StartCountdown()
    {
        _timer--;
        if (_timer == 0)
            Playtable.Instance.ForceExit();

        _popup.UpdateMessage(string.Format(Localization.GetAsset<string>(AssetsCatalog.string_Close_Game_After), _timer));
        Invoke("StartCountdown", 1);
    }

    public void ResetTimer()
    {
        if (IsInvoking("StartCountdown"))
            CancelInvoke("StartCountdown");

        if (IsInvoking("AskForLeave"))
            CancelInvoke("AskForLeave");

        Invoke("AskForLeave", GameSettings.InactiveTime);
    }
}
