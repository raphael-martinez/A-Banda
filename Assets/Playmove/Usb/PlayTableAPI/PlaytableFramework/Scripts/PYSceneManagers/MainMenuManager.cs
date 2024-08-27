using Playmove;
using Playmove.Avatars.API;
using UnityEngine;

public class MainMenuManager : PYSceneManager
{
    private static MainMenuManager _instance;
    public static MainMenuManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<MainMenuManager>();
            return _instance;
        }
    }

    [Header("Buttons")]
    [SerializeField]
    private PYButton _buttonMenu;
    [SerializeField]
    private PYButton _buttonExit;

    [SerializeField]
    private PYButton _buttonExercise;
    [SerializeField]
    private PYButton _buttonGallery;

    [Header("Scenes to Navigate")]
    [SerializeField]
    private PYSceneManager _sceneExercise;
    [SerializeField]
    private PYSceneManager _sceneGallery;
    [SerializeField]
    private PYSceneManager _sceneMenuOptions;
    
    private static bool _gameIsOpen;
    
    void Awake()
    {
        if (_instance == null)
            _instance = this;
        
        _buttonExercise.onClick.AddListener((sender) =>
        {
            Close(() =>
            {

                _sceneExercise.Open();
            });
        });
        _buttonGallery.onClick.AddListener((sender) =>
        {
            Close(() => _sceneGallery.Open());
        });
    }

    protected override void Start()
    {
        base.Start();

        if (!_gameIsOpen)
        {
            AvatarAPI.Open(null);   
        }
        _gameIsOpen = true;

        _buttonMenu.onClick.AddListener((sender) =>
            {
                Close(() => _sceneMenuOptions.Open());
            });



        _buttonExit.onClick.AddListener((sender) =>
            {
                // Fade game to exit
                FaderManager.GameFade.FadeIn(1, 0, 1, 0, CloseApp);
                FaderManager.GameFade.GetComponent<SpriteRenderer>().sortingOrder = 9999;

                // Fade all sound groups
                PYAudioManager.Instance.StopGroup(PYGroupTag.Music, 1);
                PYAudioManager.Instance.StopGroup(PYGroupTag.SFX, 1);
                PYAudioManager.Instance.StopGroup(PYGroupTag.Voice, 1);
            });
    }

    public void CloseApp()
    {
        Close(() =>
        {
#if UNITY_EDITOR
            print("Quit");
#endif
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
        });
        Invoke("HardClose", 1.5f);
    }

    new void HardClose()
    {
        if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
}
