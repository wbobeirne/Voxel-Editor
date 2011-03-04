using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace VoxelEditor {

    public class Camera : Microsoft.Xna.Framework.GameComponent {

        public Matrix view { get; protected set; }
        public Matrix projection { get; protected set; }
        public Vector3 position { get; set; }
        public Vector3 target { get; set; }
        public Vector3 up { get; set; }


        public Camera(Game game, Vector3 pos, Vector3 tar, Vector3 u)
            : base(game) {

            position = pos;
            target = tar;
            up = u;

            view = Matrix.CreateLookAt(position, target, up);

            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                (float)Game.Window.ClientBounds.Width /
                (float)Game.Window.ClientBounds.Height,
                1, 10000);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize() {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime) {

            view = Matrix.CreateLookAt(position, target, up);

            base.Update(gameTime);
        }

        public void Rotate(float radsToRotate, Vector3 rotationAxis) {

            Matrix yrMatrix = new Matrix();

            yrMatrix.M11 = 1; yrMatrix.M21 = 0; yrMatrix.M31 = 0; 
            yrMatrix.M12 = 0; yrMatrix.M22 = 0; yrMatrix.M32 = -1; 
            yrMatrix.M13 = 0; yrMatrix.M23 = 1; yrMatrix.M33 = 0; 

            if (rotationAxis == Vector3.Up) {
                position = Vector3.Transform((position - target),
                    Matrix.CreateRotationY(radsToRotate)) + target;
            }
            else if (rotationAxis == Vector3.Left) {
                position = Vector3.Transform((position - target),
                    Matrix.CreateRotationX(radsToRotate)) + target;
                
            }

        }

        public void Zoom(float distanceToZoom){           
            Vector3 direction = position - target;
            direction.Normalize();

            position -= direction * distanceToZoom;
        }

        public void Reset(Vector3 pos, Vector3 tar, Vector3 u) {

            position = pos;
            target = tar;
            this.up = Vector3.Up;

        }
    }
}