using System;
using UnityEngine;
using UnityEngine.UI;

namespace NotYetSafe.MenuTheme
{
    /// <summary>
    /// A resolution-independent city illustration rendered as one uGUI mesh.
    /// The serialized geometry uses a 1920 by 1080, top-left coordinate space.
    /// It uses uGUI's white texture and requires no additional art package.
    /// </summary>
    [AddComponentMenu("Not Yet Safe/Menu/City Backdrop")]
    [DisallowMultipleComponent]
    public sealed class EarthquakeMenuBackdrop : MaskableGraphic
    {
        [Serializable]
        public struct Quad
        {
            public Vector2 p0;
            public Vector2 p1;
            public Vector2 p2;
            public Vector2 p3;
            public Color top;
            public Color bottom;
        }

        [SerializeField] private Quad[] shapes = new Quad[0];

        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            if (shapes == null)
                return;

            Rect bounds = rectTransform.rect;
            Vector2 scale = new Vector2(bounds.width / 1920f, bounds.height / 1080f);

            for (int i = 0; i < shapes.Length; i++)
            {
                Quad q = shapes[i];
                int start = helper.currentVertCount;
                AddVertex(helper, q.p0, q.top * color, bounds, scale);
                AddVertex(helper, q.p1, q.top * color, bounds, scale);
                AddVertex(helper, q.p2, q.bottom * color, bounds, scale);
                AddVertex(helper, q.p3, q.bottom * color, bounds, scale);
                helper.AddTriangle(start, start + 1, start + 2);
                helper.AddTriangle(start, start + 2, start + 3);
            }
        }

        private static void AddVertex(VertexHelper helper, Vector2 point, Color tint,
            Rect bounds, Vector2 scale)
        {
            Vector3 position = new Vector3(bounds.xMin + point.x * scale.x,
                bounds.yMax - point.y * scale.y, 0f);
            helper.AddVert(position, tint, Vector2.zero);
        }
    }
}
