using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using System;

using Object = UnityEngine.Object;
using System.Reflection;
#endif

namespace Playmove
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PYImageSprite : PYImage
    {
        private SpriteRenderer _render;
        public SpriteRenderer Render
        {
            get
            {
                if (_render == null)
                    _render = GetComponent<SpriteRenderer>();
                return _render;
            }
        }

        public override void RestoreComponent()
        {
            if (UpdateData.UpdateFromBundle)
            {
#if UNITY_EDITOR
                if (UpdateData.DefaultComponentValue != null && !(UpdateData.DefaultComponentValue is string))
                {
                    Object sprite = (Object)UpdateData.DefaultComponentValue;
                    if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(sprite)))
                        Render.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(sprite), typeof(Sprite));
                }
                else
                    Image = null;
#else
            base.RestoreComponent();
#endif
            }
        }

        public Sprite Sprite
        {
            get
            {
                return Render.sprite;
            }
            set
            {
                Render.sprite = value;
            }
        }

        public override Texture2D Image
        {
            get
            {
                return Render.sprite != null ? Render.sprite.texture : null;
            }
            set
            {
                if (value == null)
                    Render.sprite = null;
                else
                {
                    if (Render.sprite != null)
                    {
                        // If Render already has a sprite, we need to get its pivot point
                        // to apply to the bundle sprite
                        Vector2 pivot = Vector2.zero;
                        Bounds spriteBounds = Render.sprite.bounds;

                        pivot.x = (spriteBounds.center.x * -1 + spriteBounds.extents.x) / spriteBounds.size.x;
                        pivot.y = (spriteBounds.center.y * -1 + spriteBounds.extents.y) / spriteBounds.size.y;

                        Rect spriteRect = Render.sprite.rect;
                        spriteRect.width = value.width;
                        spriteRect.height = value.height;
                        Render.sprite = Sprite.Create(value, spriteRect, pivot, Render.sprite.pixelsPerUnit);
                    }
                    else
                    {
                        Render.sprite = Sprite.Create(value, new Rect(0, 0, value.width, value.height), Vector2.one / 2);
                    }
                }
            }
        }

        public override Material Material
        {
            get
            {
                return UseSharedMaterial ? Render.sharedMaterial : Render.material;
            }
            set
            {
                if (UseSharedMaterial)
                    Render.sharedMaterial = value;
                else
                    Render.material = value;
            }
        }
    }
}