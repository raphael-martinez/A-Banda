using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Playmove.Avatars.API;

#if UNITY_5_3_1
using UnityEngine.SceneManagement;
#endif

namespace Playmove
{

    /// <summary>
    /// Responsable to manage a scene,
    /// managing the current state and the animations of entry and exit for each state.
    /// It has the control and transitions of scenes and submenus.
    /// </summary>
    public abstract class PYSceneManager : PYOpenable
    {
        [Header("PYSceneManager")]
        public bool StartSceneOnStart = true;
        public float OpenDelay = 0.2f;

        [Serializable]
        public struct AnimatorTag
        {
            [FormerlySerializedAs("Animator")]
            public PYAnimation Animation;
            public bool PlayReverse;
            public string Tag;
        }

        public List<AnimatorTag> StartingAnimations;
        public List<AnimatorTag> EndingAnimations;

        private Action _animationAction;

        public static bool isToOpenAvatar = true;


        protected virtual void Start()
        {
            if (StartSceneOnStart)
                Invoke("Open", OpenDelay);
            else if (State == OpenableState.Closed)
                Closed();
        }

        /// <summary>
        /// Start the scene, it will start 
        /// Irá abrir estando no Fechada, irá alterar o estado e lançar o evento.
        /// Ativa o inicio de cena iniciando animações de entrada.
        /// </summary>
        public override void Open()
        {
            base.Open();
            if (StartingAnimations.Count == 0)
                Opened();
            else
            {
                _animationAction += Opened;
                PlayAnimations(StartingAnimations, () =>
                {
                    Opened();
                    // #avatar
                    if (isToOpenAvatar)
                    {
                        AvatarAPI.GetSlots((result) =>
                        {
                            if (result.Data == null || result.Data[0].Players.Count == 0)
                            {
                                Avatars.API.AvatarAPI.Open(null);
                                isToOpenAvatar = false;
                            }
                        });
                    }
                });
            }
        }

        /// <summary>
        /// Irá abrir se estiver no estado Abrir, alterar o estado e lançar o evento.
        /// Irá lançar animações de saida.
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (EndingAnimations.Count == 0)
                Closed();
            else
            {
                _animationAction += Closed;
                PlayAnimations(EndingAnimations, () => Closed());
            }
        }

        [SerializeField]
        private int _callbackCounter;
        private int CallbackCounter
        {
            get { return _callbackCounter; }
            set
            {
                _callbackCounter = value;
                if (_callbackCounter <= 0)
                {
                    _callbackCounter = 0;
                    if (_animationAction != null) _animationAction();
                    _animationAction = null;
                }
            }
        }

        private void PlayAnimations(List<AnimatorTag> animations, Action complited)
        {
            foreach (AnimatorTag animation in animations)
            {
                CallbackCounter++;
                if (animation.Animation is PYAnimator)
                {
                    if (!animation.PlayReverse)
                        ((PYAnimator)animation.Animation).Play(animation.Tag, () => CallbackCounter--);
                    else
                        ((PYAnimator)animation.Animation).Reverse(animation.Tag, () => CallbackCounter--);
                }
                else
                {
                    if (!animation.PlayReverse)
                        animation.Animation.Play(() => CallbackCounter--);
                    else
                        animation.Animation.Reverse(() => CallbackCounter--);
                }
            }
            complited.Invoke();
        }

        #region Transitions - Cenas - SubMenus
        /// <summary>
        /// Quando a animação de close da cena atual terminar
        /// chamará o Open da cena passada.
        /// Deve ser usada quando precisar carregar uma cena diferente
        /// sem ter que mudar de cena na unity.
        /// </summary>
        /// <param name="newScene">Próxima cena para abrir</param>
        public virtual void ChangeScene(PYSceneManager newScene)
        {
            Close(newScene.Open);
        }
        /// <summary>
        /// Quando a animação de close da cena atual terminar
        /// chamará Application.LoadLevel com o nome da cena passado.
        /// Deve ser usada quando precisar carregar outra cena da unity.
        /// </summary>
        /// <param name="newScene">Próxima cena para abrir</param>
        public virtual void ChangeScene(TagManager.Scenes newScene)
        {
            Close(() => SceneManager.LoadScene(newScene.ToString()));
        }

        /// <summary>
        /// Quando a animação de close da cena atual terminar 
        /// chamará Application.LoadLevel com o nome da cena passado.
        /// Deve ser usada quando precisar carregar outra cena da unity.
        /// </summary>
        /// <param name="newScene">Próxima cena para abrir</param>
        public virtual void ChangeScene(string newScene)
        {
            Close(() => SceneManager.LoadScene(newScene));
        }

        private PYOpenable _lastSubMenuOpened;

        /// <summary>
        /// Usado para quando houver um menu que irá abrir com união ao novo,
        /// geralmente uma popups ou objetos (openable) complementares ao menu.
        /// </summary>
        /// <param name="subMenu">Objeto de complemento de cena</param>
        public virtual void OpenSubMenu(PYOpenable subMenu)
        {
            _lastSubMenuOpened = subMenu;
            subMenu.Open();
        }

        public virtual void CloseSubMenu()
        {
            _lastSubMenuOpened.Close();
        }
        public virtual void CloseSubMenu(PYOpenable subMenu)
        {
            subMenu.Close();
        }
        #endregion
    }
}