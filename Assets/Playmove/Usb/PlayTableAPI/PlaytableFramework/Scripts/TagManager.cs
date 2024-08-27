using UnityEngine;

namespace Playmove
{
    public class TagManager
    {
        #region Py

        public enum Scenes
        {
            Initialize,
            MainMenu,
            Info,
            Score,
            GameMode,
            Gameplay,
            PYBundleExample,
            Gallery,
            SelectExercise,
        }

        public enum PYButtonEvents
        {
            OnClick,
            OnDown,
            OnUp,
            OnEnter,
            OnExit
        }

        public enum Axis
        {
            None,
            X,
            Y,
            Z
        }

        public enum Type
        {
            From,
            To
        }

        public enum LoopType
        {
            None,
            Loop,
            PingPong
        }

        public enum GameDifficulty
        {
            Easy,
            Normal,
            Hard,
            None,
        }

        public enum GameState
        {
            Starting,
            Running,
            Paused,
            Completed
        }

        public enum CountDirection
        {
            Crescent,
            Decrescent
        }

        public enum GamePrefs
        {
            GameToken,
            GameDifficulty,
            SomethingToSave
        }

        public enum SortingLayerNames
        {
            Background,
            Game,
            Default,
            Foreground,
            GUI
        }

        public const string POPUP_RESOURCES_PATH = "Popups/";
        public const string POPUP_BUTTON_RESOURCES_PATH = "Popups/Buttons/";

        public const float BLACK_FADER_IN_GAME = 31;
        public const float BLACK_FADER_IN_CAMERA = 1;

        /// <summary>
        /// Camera child
        /// </summary>
        public const float BLACK_FADER_Z_POSITION = 10;

        /// <summary>
        /// Global
        /// </summary>
        public const float PYPOPUPS_Z_POSITION = -40;

        #endregion Py

        public const string LOCALIZATION_BACK = "Geral_Voltar";
        public const string LOCALIZATION_RESTRICTED_ACCESS = "Placar_MsgAcessoRestrito";
        public const string LOCALIZATION_CONFIRM_DELETE_ALL = "Placar_MsgApagarPlacar";
        public const string LOCALIZATION_SCORE_DELETED = "Placar_MsgPlacarApagado";
        public const string LOCALIZATION_ATENTION = "Geral_Atencao";
        public const string LOCALIZATION_CONTINUE = "Geral_Continuar";

        public static string DIRECTORY_TO_SAVE_PENDRIVE = "{0}" + Application.companyName + "/";

        public enum EMessageType
        {
            Success,
            Normal,
            Warning,
            Error
        }

        public enum EGameName
        {
            Memoria,
            CentralDeAtividades,
            Pintura,
            QuebraCabecas
        }

        public enum EFilterOrder
        {
            NameAsc,
            NameDesc,
            ClassAsc,
            ClassDesc,
            MaisNovos,
            MaisVelhos
        }

        #region Central de atividades

        public readonly static string PATH_TO_GROUP_MANAGER_DATA_FILE = Application.persistentDataPath + "/groupdata.bin";

        // 0: Group Name
        public readonly static string PATH_TO_ROOT_EXERCISES = Application.persistentDataPath + "/Exercises/";

        public readonly static string PATH_TO_EXERCISES = Application.persistentDataPath + "/Exercises/{0}/";
        public readonly static string PATH_TO_ROOT_GALLERY = Application.persistentDataPath + "/Gallery/";

        // 0: Nome da turma
        // 1: Nome do aluno
        // 2: Nome do livro
        public readonly static string PATH_TO_GALLERY = Application.persistentDataPath + "/Gallery/{0}/{1}/{2}/";

        public readonly static string PATH_TO_USERS_IN_CLASS = Application.persistentDataPath + "/Gallery/{0}/";
        public readonly static string PATH_TO_EXERCISES_FROM_USERS = Application.persistentDataPath + "/Gallery/{0}/{1}/";

        //// 0: Caminho do pendrive
        //// 1: Nome da turma
        //// 2: Nome do aluno
        //// 3: Nome do livro
        //// 4: File name
        public static string PATH_TO_GALLERY_PENDRIVE
        {
           get
           {
               return "{0}Playmove/" + PYBundleManager.Localization.GetAsset<string>(PYBundleTags.Text_GameTitle, "Central de Atividades")
                   + "/{1}/{2}/{3}/{4}";
           }
        }

        public const string TAG_RADIOBUTTON_CONFIGURATION_ON = "RadioButtonOn";
        public const string TAG_RADIOBUTTON_CONFIGURATION_OFF = "RadioButtonOff";

        public const string AUDIO_MANAGER_GROUPS_IS_MUTE = "AudioManager{0}";

        // PlayerPrefs that holds a value for the exercise/siluet path based
        // in the name of the draw that user has made
        // {0} = Draw Name
        public const string PREFS_SILUET_PATH_BY_EXERCISE = "Draw_{0}_With_Siluet";

        public const string EXERCISE_TYPE_TRANSPARENT = "transparent";
        public const string EXERCISE_TYPE_OPAQUE = "opaque";
        #endregion Central de atividades
    }
}