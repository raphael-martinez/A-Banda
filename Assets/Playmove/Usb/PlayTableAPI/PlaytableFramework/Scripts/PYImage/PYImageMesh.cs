using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Playmove
{
    public class PYImageMesh : PYImage
    {
        private Renderer _render;
        public Renderer Render
        {
            get
            {
                if (_render == null)
                    _render = GetComponent<Renderer>();
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
                    Object tex = (Object)UpdateData.DefaultComponentValue;
                    if (tex != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tex)))
                        Render.sharedMaterial.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(tex), typeof(Texture2D));
                }
                else
                    Image = null;
#else
            base.RestoreComponent();
#endif
            }
        }

        public override Texture2D Image
        {
            get
            {
                return (Texture2D)(UseSharedMaterial ? Render.sharedMaterial.mainTexture : Render.material.mainTexture);
            }
            set
            {
                if (UseSharedMaterial)
                    Render.sharedMaterial.mainTexture = value;
                else
                    Render.material.mainTexture = value;
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