using GameEngine.Core;
using GameEngine.Util.Attributes;
using GameEngine.Util.Interfaces;
using GameEngine.Util.Resources;
using GameEngine.Util.Values;
using System.Numerics;

namespace GameEngine.Util.Nodes
{
    internal class Checkbox : NodeUI, ICanvasItem
    {

        [Inspect]
        public bool Visible { get; set; } = true;

        [Inspect]
        public bool value = false;

        [Inspect]
        public Texture? actived_texture = null; 
        public Texture? unactived_texture = null;

        private Color _color = new();

        [Inspect]
        public Material material = new Material2D(Material2D.DrawTypes.Texture);

        protected override void Init_()
        {
            float[] v = new float[] { 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 1.0f };
            float[] uv = new float[] { 0f, 0f, 1f, 0f, 1f, 1f, 0f, 1f };
            uint[] i = new uint[] { 0, 1, 3, 1, 2, 3 };

            DrawService.CreateBuffer(NID, "aPosition");
            DrawService.SetBufferData(NID, "aPosition", v, 2);

            DrawService.CreateBuffer(NID, "aTextureCoord");
            DrawService.SetBufferData(NID, "aTextureCoord", uv, 2);

            DrawService.SetElementBufferData(NID, i);

            DrawService.EnableAtributes(NID, material);
        }
        protected override void Draw(double deltaT)
        {
            var gl = Engine.gl;

            material.Use();
            bool useTexture = false;

            if (value)
            {
                if (actived_texture != null)
                {
                    actived_texture.Use();
                    useTexture = true;
                }
                else
                {
                    _color = new Color(0, 0, 255);
                    useTexture = false;
                }
            }
            else
            {
                if (unactived_texture != null)
                {
                    unactived_texture.Use();
                    useTexture = true;
                }
                else
                {
                    _color = new Color(0, 0, 0);
                    useTexture = false;
                }
            }

            material.SetUniform("color", _color);
            material.SetUniform("configDrawType", useTexture? 1 : 0);

            var world = MathHelper.Matrix4x4CreateRect(Position, Size) * Viewport!.Camera2D.GetViewOffset();
            var proj = Viewport!.Camera2D.GetProjection();

            material.SetTranslation(world);
            material.SetProjection(proj);


            DrawService.Draw(NID);

        }

        public void Hide() => Visible = false;
        public void Show() => Visible = true;

    }
}
