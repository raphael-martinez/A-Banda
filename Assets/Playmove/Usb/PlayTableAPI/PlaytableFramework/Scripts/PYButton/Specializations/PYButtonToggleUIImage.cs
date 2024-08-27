using Playmove;
using UnityEngine;
using UnityEngine.UI;

public class PYButtonToggleUIImage : PYButtonToggle
{
    [SerializeField]
    private Sprite _unSelectedImage, _selectedImage;

    private Image _image { get { return GetComponent<Image>(); } }

    protected override void SelectAction()
    {
        base.SelectAction();
        _image.sprite = _selectedImage;
    }

    protected override void DeselectAction()
    {
        base.DeselectAction();
        _image.sprite = _unSelectedImage;
    }
}