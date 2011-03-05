using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelEditor {
    public class Voxel {

        public Model model { get; protected set; }
        public Matrix world { get; set; }
        public float scale { get; set; }
        public Texture2D texture;
        public Vector3 color { get; set; }
        public float alpha { get; set; }

        private float desiredAlpha = 1f;

        public Voxel(Model m, Matrix w, Texture2D t, Vector3 c) {

            model = m;
            world = w;
            texture = t;
            color = c;
            alpha = 1.0f;
            scale = 1.0f;

        }

        public virtual void Update() {

        }

        public void Draw(Camera camera) {

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes) {
                foreach (BasicEffect effect in mesh.Effects) {

                    effect.EnableDefaultLighting();
                    effect.Projection = camera.projection;
                    effect.View = camera.view;
                    effect.World = Matrix.CreateScale(scale) * world * mesh.ParentBone.Transform;
                    effect.Alpha = alpha;
                    effect.TextureEnabled = true;
                    effect.Texture = texture;
                    effect.DiffuseColor = color;

                }

                mesh.Draw();
            }
        }

        public virtual Matrix GetWorld() {

            return world;

        }

        public void AlphaFluctuate() {
            if (alpha > desiredAlpha)
                alpha -= 0.005f;
            else if (alpha < desiredAlpha)
                alpha += 0.005f;

            if (alpha >= 0.7f)
                desiredAlpha = 0.4f;
            if (alpha <= 0.4f)
                desiredAlpha = 0.7f;
        }
    }
}
