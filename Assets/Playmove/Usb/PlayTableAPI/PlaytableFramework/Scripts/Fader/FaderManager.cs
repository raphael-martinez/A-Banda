using Old;
using System;
using UnityEngine;

namespace Playmove
{
    public class FaderManager : MonoBehaviour
    {
        private static Fader _gameFade;
        public static Fader GameFade
        {
            get
            {
                if (!_gameFade)
                {
                    _gameFade = Create("GameFade");
                    _gameFade.ResetPosition(false);
                }
                return _gameFade;
            }
        }

        private static Fader _cameraFade;
        public static Fader CameraFade
        {
            get
            {
                if (!_cameraFade)
                {
                    _cameraFade = Create("CameraFade");
                    _cameraFade.ResetPosition(true);
                }
                return _cameraFade;
            }
        }

        private static Fader Create(string name)
        {
            GameObject faderResource = Resources.Load<GameObject>("Fader");

            Fader fade = Instantiate(faderResource).AddComponent<Fader>();
            fade.name = name;
            fade.GetComponent<Collider2D>().enabled = false;

            if (Camera.main != null)
                fade.transform.parent = Camera.main.transform;

            fade.FadeSpriteRenderer = fade.GetComponent<SpriteRenderer>();
            fade.FadeAnimation = PYTweenAnimation.Add(fade.gameObject).SetDuration(1);

            return fade;
        }

        [Obsolete("Use the method FadeInGame")]
        public static void FadeGame(float duration = 1, float delay = 0, Color? color = null, Action callback = null)
        {
            FadeInGame(duration, delay, color, callback);
        }
        [Obsolete("Use the method FadeOutGame")]
        public static void RemoveGameFade(float duration = 1, float delay = 0, Color? color = null, Action callback = null)
        {
            FadeOutGame(duration, delay, color, callback);
        }

        public static void FadeInGame(float duration = 1, float delay = 0, Color? color = null, Action callback = null)
        {
            // If fader is already open we dont need to animate it all over again
            if (GameFade.IsOpen)
            {
                if (callback != null)
                    callback();
                return;
            }

            GameFade.FadeSpriteRenderer.color = color.GetValueOrDefault();
            GameFade.FadeIn(duration, 0, 0.5f, delay, callback);
        }
        public static void FadeOutGame(float duration = 1, float delay = 0, Color? color = null, Action callback = null)
        {
            GameFade.FadeSpriteRenderer.color = color.GetValueOrDefault();
            GameFade.FadeOut(duration, 0.5f, 0, delay, callback);
        }

        public static void FadeInCamera(float duration = 1, float delay = 0, Color? color = null, Action callback = null)
        {
            // If fader is already open we dont need to animate it all over again
            if (CameraFade.IsOpen)
            {
                if (callback != null)
                    callback();
                return;
            }

            CameraFade.FadeSpriteRenderer.color = color.GetValueOrDefault();
            CameraFade.FadeIn(duration, 0, 1, delay, callback);
        }

        public static void FadeOutCamera(float duration = 1, float delay = 0, Color? color = null, Action callback = null)
        {
            CameraFade.FadeSpriteRenderer.color = color.GetValueOrDefault();
            CameraFade.FadeOut(duration, 1, 0, delay, callback);
        }
    }
}