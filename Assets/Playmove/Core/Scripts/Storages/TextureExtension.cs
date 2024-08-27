using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Playmove.Core.Storages
{
    public static class TextureExtension
    {
        public static bool HasAlpha(this Texture2D texture, float percentageAlphaPixels)
        {
            if (texture == null)
                return false;

            percentageAlphaPixels = Mathf.Clamp(percentageAlphaPixels, 0, 100);
            Color32[] pixels = texture.GetPixels32();
            int necessaryAlphaPixels = (int)(pixels.Length * percentageAlphaPixels / 100);

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a < 0xff)
                {
                    necessaryAlphaPixels--;
                    if (necessaryAlphaPixels <= 0)
                        return true;
                }
            }

            return false;
        }

        #region Texture Fill
        public struct Point
        {
            public short x;
            public short y;
            public Point(short aX, short aY) { x = aX; y = aY; }
            public Point(int aX, int aY) : this((short)aX, (short)aY) { }
        }

        public static bool CompareColor32(Color32 colA, Color32 colB)
        {
            return (colA.r + colA.g + colA.b) == (colB.r + colB.g + colB.b);
        }

        private static Color32[] colors;
        private static Color32[] colorsComp;

        public static void ReleaseFillCache()
        {
            colors = colorsComp = null;
            System.GC.Collect();
        }

        public static void FloodFillArea(this Texture2D aTex, int pixelX, int pixelY, Color32 aFillColor)
        {
            int w = aTex.width;
            int h = aTex.height;

            if (colors == null)
                colors = aTex.GetPixels32();

            Color32 refCol = colors[pixelX + pixelY * w];
            Queue<Point> nodes = new Queue<Point>();
            nodes.Enqueue(new Point(pixelX, pixelY));
            while (nodes.Count > 0)
            {
                Point current = nodes.Dequeue();
                for (int i = current.x; i < w; i++)
                {
                    Color32 C = colors[i + current.y * w];
                    if (!CompareColor32(C, refCol) ||
                        CompareColor32(C, aFillColor))
                        break;
                    colors[i + current.y * w] = aFillColor;
                    if (current.y + 1 < h)
                    {
                        C = colors[i + current.y * w + w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }
                    if (current.y - 1 >= 0)
                    {
                        C = colors[i + current.y * w - w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }
                for (int i = current.x - 1; i >= 0; i--)
                {
                    Color32 C = colors[i + current.y * w];
                    if (!CompareColor32(C, refCol) ||
                       CompareColor32(C, aFillColor))
                        break;
                    colors[i + current.y * w] = aFillColor;
                    if (current.y + 1 < h)
                    {
                        C = colors[i + current.y * w + w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }
                    if (current.y - 1 >= 0)
                    {
                        C = colors[i + current.y * w - w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }
            }
            aTex.SetPixels32(colors);
        }

        public static void FloodFillArea(this Texture2D aTex, Texture2D comparableTex, int pixelX, int pixelY, Color32 aFillColor)
        {
            int w = comparableTex.width;
            int h = comparableTex.height;

            if (colors == null)
                colors = aTex.GetPixels32();

            if (colorsComp == null)
                colorsComp = comparableTex.GetPixels32();

            Color32 refCol = colorsComp[pixelX + pixelY * w];
            if (refCol.r == 0 &&
                refCol.g == 0 &&
                refCol.b == 0)
                return;

            Queue<Point> nodes = new Queue<Point>();
            nodes.Enqueue(new Point(pixelX, pixelY));
            while (nodes.Count > 0)
            {
                Point current = nodes.Dequeue();
                for (int i = current.x; i < w; i++)
                {
                    Color32 C = colorsComp[i + current.y * w];
                    if (!CompareColor32(C, refCol) ||
                        CompareColor32(C, aFillColor))
                        break;

                    colors[i + current.y * w] = colorsComp[i + current.y * w] = aFillColor;

                    if (current.y + 1 < h)
                    {
                        C = colorsComp[i + current.y * w + w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }
                    if (current.y - 1 >= 0)
                    {
                        C = colorsComp[i + current.y * w - w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }
                for (int i = current.x - 1; i >= 0; i--)
                {
                    Color32 C = colorsComp[i + current.y * w];
                    if (!CompareColor32(C, refCol) ||
                       CompareColor32(C, aFillColor))
                        break;

                    colors[i + current.y * w] = colorsComp[i + current.y * w] = aFillColor;

                    if (current.y + 1 < h)
                    {
                        C = colorsComp[i + current.y * w + w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }
                    if (current.y - 1 >= 0)
                    {
                        C = colorsComp[i + current.y * w - w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }
            }

            aTex.SetPixels32(colors);
            comparableTex.SetPixels32(colorsComp);
        }

        public static void FloodFillAreaWithTexturePattern(this Texture2D aTex, Texture2D comparableTex, int pixelX, int pixelY)
        {
            int w = comparableTex.width;
            int h = comparableTex.height;

            Color32 aFillColor = new Color(1, 1, 1, 1);

            if (colors == null)
                colors = aTex.GetPixels32();

            if (colorsComp == null)
                colorsComp = comparableTex.GetPixels32();

            Color32 refCol = colorsComp[pixelX + pixelY * w];
            if (refCol.r == 0 &&
                refCol.g == 0 &&
                refCol.b == 0)
                return;

            Queue<Point> nodes = new Queue<Point>();
            nodes.Enqueue(new Point(pixelX, pixelY));
            while (nodes.Count > 0)
            {
                Point current = nodes.Dequeue();
                for (int i = current.x; i < w; i++)
                {
                    Color32 C = colorsComp[i + current.y * w];

                    if (!CompareColor32(C, refCol) ||
                        CompareColor32(C, aFillColor))
                        break;

                    colors[i + current.y * w] = colorsComp[i + current.y * w] = aFillColor;

                    if (current.y + 1 < h)
                    {
                        C = colorsComp[i + current.y * w + w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }
                    if (current.y - 1 >= 0)
                    {
                        C = colorsComp[i + current.y * w - w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }
                for (int i = current.x - 1; i >= 0; i--)
                {
                    Color32 C = colorsComp[i + current.y * w];
                    if (!CompareColor32(C, refCol) ||
                       CompareColor32(C, aFillColor))
                        break;

                    colors[i + current.y * w] = colorsComp[i + current.y * w] = aFillColor;

                    if (current.y + 1 < h)
                    {
                        C = colorsComp[i + current.y * w + w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }
                    if (current.y - 1 >= 0)
                    {
                        C = colorsComp[i + current.y * w - w];
                        if (CompareColor32(C, refCol) &&
                            !CompareColor32(C, aFillColor))
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }
            }

            aTex.SetPixels32(colors);
            comparableTex.SetPixels32(colorsComp);
        }
        #endregion

        #region Scale
        public static Sprite Scaled(this Sprite src, int width, int height, bool keepAspect = true,
            float pixelsPerUnit = 100, uint extrude = 1, SpriteMeshType meshType = SpriteMeshType.FullRect,
            bool generateFallbackPhysicsShape = false, FilterMode mode = FilterMode.Bilinear)
        {
            src.texture.Scale(width, height, keepAspect, mode);
            return Sprite.Create(src.texture, new Rect(0, 0, src.texture.width, src.texture.height), Vector2.one / 2, pixelsPerUnit, extrude,
                meshType, Vector4.one, generateFallbackPhysicsShape);
        }

        /// <summary>
        /// Returns a scaled copy of given texture.
        /// </summary>
        /// <param name="tex">Source texure to scale</param>
        /// <param name="width">Destination texture width</param>
        /// <param name="height">Destination texture height</param>
        /// <param name="mode">Filtering mode</param>
        public static Texture2D Scaled(this Texture2D src, int width, int height, bool keepAspect = true, FilterMode mode = FilterMode.Bilinear)
        {
            if (keepAspect)
                KeepAspectRatio(src.width, src.height, ref width, ref height);

            Rect texR = new Rect(0, 0, width, height);
            _gpu_scale(src, width, height, mode);

            //Get rendered data back to a new texture
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
            result.Reinitialize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            result.Apply();
            return result;
        }

        /// <summary>
        /// Scales the texture data of the given texture.
        /// </summary>
        /// <param name="src">Texure to scale</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="mode">Filtering mode</param>
        public static void Scale(this Texture2D src, int width, int height, bool keepAspect = true, FilterMode mode = FilterMode.Bilinear)
        {
            if (keepAspect)
                KeepAspectRatio(src.width, src.height, ref width, ref height);

            if (src.width == width && src.height == height) return;

            Rect texR = new Rect(0, 0, width, height);
            _gpu_scale(src, width, height, mode);

            // Update new texture
            src.Reinitialize(width, height);
            src.ReadPixels(texR, 0, 0, true);
            src.Apply(true);
        }

        // Internal unility that renders the source texture into the RTT - the scaling method itself.
        static Rect rectForGPU = new Rect(0, 0, 1, 1);
        static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
        {
            // We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);

            // Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = RenderTexture.GetTemporary(width, height, 16);

            // Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            // Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            // Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, Color.clear);
            Graphics.DrawTexture(rectForGPU, src);

            RenderTexture.ReleaseTemporary(rtt);
        }

        static void KeepAspectRatio(int originalWidth, int originalHeight, ref int width, ref int height)
        {
            float ratioX = (float)originalWidth / originalHeight;
            float ratioY = (float)originalHeight / originalWidth;
            if (ratioX > ratioY)
                height = (int)(width / ratioX);
            else
                width = (int)(height / ratioY);
        }
        #endregion
    }
}
