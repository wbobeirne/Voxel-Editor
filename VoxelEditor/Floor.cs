using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace VoxelEditor {
    class Floor {

        Vector3 size;
        Vector3 position;
        VertexPositionNormalTexture[] verticies;
        int triangles;
        VertexBuffer buffer;
        Texture2D texture;
        BasicEffect effect;


        public Floor(Vector3 size, Vector3 pos, Texture2D tex) {

            this.size = size;
            position = pos;
            texture = tex;

        }

        private void BuildFloor() {

            triangles = 2;

            verticies = new VertexPositionNormalTexture[6];

            Vector3 bottomLeft = position + new Vector3(-1f, -1f, 0);
            Vector3 bottomRight = position + new Vector3(-1f, 1f, 0);
            Vector3 topLeft = position + new Vector3(1f, -1f, 0);
            Vector3 topRight = position + new Vector3(1f, 1f, 0);

            Vector3 floorNormal = new Vector3(0.0f, 1.0f, 0.0f) * size;

            Vector2 textureTopLeft = new Vector2(0.5f * size.X, 0.0f * size.Y);
            Vector2 textureTopRight = new Vector2(0.0f * size.X, 0.0f * size.Y);
            Vector2 textureBottomLeft = new Vector2(0.5f * size.X, 0.5f * size.Y);
            Vector2 textureBottomRight = new Vector2(0.0f * size.X, 0.5f * size.Y);

            verticies[0] = new VertexPositionNormalTexture(
                topLeft, floorNormal, textureTopLeft);
            verticies[1] = new VertexPositionNormalTexture(
                bottomRight, floorNormal, textureBottomRight);
            verticies[2] = new VertexPositionNormalTexture(
                bottomLeft, floorNormal, textureTopLeft);
            verticies[3] = new VertexPositionNormalTexture(
                topLeft, floorNormal, textureTopLeft);
            verticies[4] = new VertexPositionNormalTexture(
                topRight, floorNormal, textureTopRight);
            verticies[5] = new VertexPositionNormalTexture(
                bottomRight, floorNormal, textureBottomRight);

        }

        public void Draw(GraphicsDevice gd, Camera camera) {

            effect = new BasicEffect(gd, null);

            effect.Begin();

            effect.World = Matrix.Identity;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {

                pass.Begin();

                effect.Texture = texture;

                RenderFloor(gd, camera);

                pass.End();
            }

            effect.End();
        }

        public void RenderFloor(GraphicsDevice gd, Camera camera) {
            BuildFloor();

            effect.TextureEnabled = true;

            buffer = new VertexBuffer(gd,
                VertexPositionNormalTexture.SizeInBytes * verticies.Length,
                BufferUsage.WriteOnly);
            buffer.SetData(verticies);

            gd.Vertices[0].SetSource(buffer, 0,
                VertexPositionNormalTexture.SizeInBytes);
            gd.VertexDeclaration = new VertexDeclaration(
                gd, VertexPositionNormalTexture.VertexElements);
            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, triangles);

        }

    }
}
