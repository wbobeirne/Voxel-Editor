using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEditor {
    class Backdrop {

        Model model;
        Texture2D texture;
        public Matrix world { get; set; }
        float alpha;
        Vector3 color = new Vector3(1.0f, 1.0f, 1.0f);

        public Backdrop(Model m, Texture2D t, Matrix w) {

            model = m;
            texture = t;

            w.M43 -= 1;

            world = w;
            alpha = 1f;

        }

        public void Draw(Camera camera) {

            foreach (ModelMesh mesh in model.Meshes) {
                foreach (BasicEffect effect in mesh.Effects) {

                    effect.Begin();

                    effect.EnableDefaultLighting();
                    effect.Projection = camera.projection;
                    effect.View = camera.view;
                    effect.World = Matrix.CreateScale(10.0f) * world * mesh.ParentBone.Transform;
                    effect.TextureEnabled = true;
                    effect.Alpha = alpha;
                    effect.Texture = texture;
                    effect.DiffuseColor = color;

                    effect.End();

                }

                mesh.Draw();
            }


        }

    }
}
