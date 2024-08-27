using System;
using UnityEngine;
using UnityEngine.UI;

namespace Playmove
{
    public class PositionAlertPopup : PYAlertPopup
    {

        public GameObject FirstPrize, OtherPrize;
        public Image FirstPrizeSprite, OtherPrizeSprite;
        public PYText PositionText;

        public static PYAlertPopup InvokePositionPopup(int position, bool closeFader = true)
        {
            PositionAlertPopup popUpGameObject = Resources.Load<PositionAlertPopup>("Popups/PositionPopup");
            popUpGameObject = (PositionAlertPopup)Instantiate(popUpGameObject, Vector3.zero, Quaternion.identity);

            ///*MUDAR ISSO*/popUpGameObject.transform.parent = FindObjectOfType<Canvas>().transform;//Camera.main.transform;

            popUpGameObject.transform.localPosition = new Vector3(0, 0, 10);

            bool wonFirstPrize = position == 1;
            popUpGameObject.FirstPrize.SetActive(wonFirstPrize);
            popUpGameObject.OtherPrize.SetActive(!wonFirstPrize);
            popUpGameObject.OtherPrizeSprite.sprite = Resources.Load<Sprite>("Popups/PositionSprites/" + Mathf.Clamp(position, 1, 4).ToString());

            PYAlertPopup popup = popUpGameObject.GetComponent<PYAlertPopup>();
            //popup.AlertPopup = new PYAlertPopupData();
            popUpGameObject.PositionText.Text = position.ToString();
            popup.ClosePopupByFader = closeFader;

            popup.Open();

            return popup;
        }
    }
}