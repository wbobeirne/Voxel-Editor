using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game {
        //Graphics stuffs
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        float aspectRatio;

        //Mouse states
        Vector2 mousePos;
        Vector2 mousePosOld;
        int scrollWheel;
        int scrollWheelOld;

        //Keyboard stuffs
        int counter = 0;        //These are for the update method
        KeyboardState oldState; //to stop held down keys from rapidly doing stuffs

        //Here are some required game objects
        Camera camera;
        HUD hud;

        //HUD Graphics


        //Color stuffs

        public Vector3 color { get; set; }
        Random randomColor = new Random();

        //Voxel stuffs
        public List<Voxel> voxelList = new List<Voxel>();
        Voxel voxelCursor;
        Model voxelModel;
        Texture2D voxelTex;
        Matrix placementWorld = Matrix.Identity;

        Model backdropModel;
        Backdrop backdrop;
        Texture2D backdropTex;
        Floor floor;

        public DialogBox dialogBox; //For the save/load dialog box


        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(GraphicsPreparingDeviceSettings);
            //Make an EH for when the user wants to resize the window, updates the camera's projection
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += new EventHandler(WindowSizeChanged);
            Content.RootDirectory = "Content";
            //Anti aliasing stuffs
            //graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here

            this.IsMouseVisible = true;

            // Initialize camera
            camera = new Camera(this, new Vector3(20.0f, 50.0f, 40.0f),
                Vector3.Zero, Vector3.Up);
            Components.Add(camera);

            //Set the color to white
            color = new Vector3(1.0f, 1.0f, 1.0f);

            //initialize dialog box
            dialogBox = new DialogBox(this);


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            voxelModel = Content.Load<Model>("Models\\highPolyCube");
            voxelTex = Content.Load<Texture2D>("Textures\\whiteTexture");

            backdropModel = Content.Load<Model>("Models\\flatePlane");
            backdropTex = Content.Load<Texture2D>("Textures\\grid");

            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            //Backdrop stuffs
            backdrop = new Backdrop(backdropModel, voxelTex, Matrix.Identity);
            floor = new Floor(new Vector3(10f, 10f, 10f), Vector3.Zero, voxelTex);

            //Cursor stuffs
            voxelCursor = new Voxel(voxelModel, placementWorld,
                voxelTex, new Vector3(1.0f, 1.0f, 1.0f));
            voxelCursor.alpha = 0.7f;
            voxelCursor.scale = 1.001f;

            //HUD stuffs
            hud = new HUD(this, spriteBatch);
            hud.LoadContent(Content, GraphicsDevice);
        }

        //Found this method on XNA Fever, meant for anti aliasing. Many thanks.
        void GraphicsPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e) {
            PresentationParameters pp = e.GraphicsDeviceInformation.PresentationParameters;
#if XBOX
                pp.MultiSampleQuality = 1;
                pp.MultiSampleType = MultiSampleType.FourSamples;
                return;
#endif
            GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
            SurfaceFormat format = adapter.CurrentDisplayMode.Format;

            int quality = 0;
            if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, false, MultiSampleType.FourSamples, out quality)) {
                pp.MultiSampleQuality = 0;
                pp.MultiSampleType = MultiSampleType.FourSamples;
            }
            else if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, false, MultiSampleType.TwoSamples, out quality)) {
                pp.MultiSampleQuality = 0;
                pp.MultiSampleType = MultiSampleType.TwoSamples;
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        #region Update & its methods

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {

            KeyboardState keyboardState = Keyboard.GetState();

            MouseState ms = Mouse.GetState();
            mousePos.X = ms.X;
            mousePos.Y = ms.Y;
            scrollWheel = ms.ScrollWheelValue;

            //This is where all of the input is, I want to wrap
            //it all in this if so that if the window is alt-tabbed,
            //or if the dialog box is open,
            //inputting stuff doesn't mess with the editor

            //You do, however, want to set new and old positions
            //so that when you come back in to the editor, it doesn't
            //compare its last position when you were in the window to
            //your new position
            if (this.IsActive && !dialogBox.InUse) {
                // Allows the game to exit
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    this.Exit();
                //Keyboard based stuff

                MovementInputCheck(keyboardState, oldState, Keys.Left, new Vector3(-2f, 0, 0));
                MovementInputCheck(keyboardState, oldState, Keys.Right, new Vector3(2f, 0, 0));
                MovementInputCheck(keyboardState, oldState, Keys.Up, new Vector3(0, 0, 2f));
                MovementInputCheck(keyboardState, oldState, Keys.Down, new Vector3(0, 0, -2f));
                MovementInputCheck(keyboardState, oldState, Keys.Add, new Vector3(0, 2f, 0));
                MovementInputCheck(keyboardState, oldState, Keys.Subtract, new Vector3(0, -2f, 0));
                if (keyboardState.IsKeyDown(Keys.Space))
                    PlaceVoxel();
                if (keyboardState.IsKeyDown(Keys.Delete))
                    RemoveVoxel();
                //Saving and loading
                if (keyboardState.IsKeyDown(Keys.S) && !oldState.IsKeyDown(Keys.S)) {
                    dialogBox.SaveDialog();
                }
                if (keyboardState.IsKeyDown(Keys.L) && !oldState.IsKeyDown(Keys.L)) {
                    dialogBox.LoadDialog();
                }

                //Color randomizer, a placeholder until I get some real color picking
                if (keyboardState.IsKeyDown(Keys.C))
                    dialogBox.ColorSelection();

                if (mousePos.X != mousePosOld.X && ms.LeftButton == ButtonState.Pressed) {
                    camera.Rotate((float)-(mousePos.X - mousePosOld.X) / 100, Vector3.Left);
                }
                if (mousePos.Y != mousePosOld.Y && ms.LeftButton == ButtonState.Pressed) {
                    camera.Rotate((float)(mousePos.Y - mousePosOld.Y) / 100, Vector3.Up);
                    MathHelper.Clamp(camera.position.Z, backdrop.world.M44, camera.position.Z);
                }
                if (mousePos.X != mousePosOld.X && ms.RightButton == ButtonState.Pressed) {
                    //camera.PanX((float)-(mousePos.X - mousePosOld.X)/30, new Vector3(0, 1, 0));
                }
                if (mousePos.Y != mousePosOld.Y && ms.RightButton == ButtonState.Pressed) {
                    //camera.PanY((float)(mousePos.Y - mousePosOld.Y)/500, new Vector3(1, 0, 0));
                }
                if (ms.MiddleButton == ButtonState.Pressed) {
                    camera.Reset(new Vector3(20.0f, 50.0f, 40.0f), Vector3.Zero, Vector3.Up);
                }
                if (scrollWheel > scrollWheelOld) {
                    camera.Zoom(3f);
                }
                if (scrollWheel < scrollWheelOld) {
                    camera.Zoom(-3f);
                }
            }

            oldState = keyboardState;

            mousePosOld.X = ms.X;
            mousePosOld.Y = ms.Y;
            scrollWheelOld = scrollWheel;

            //Makes the cursor fade in and out
            //Also change the color of the cursor to
            //reflect the current color
            voxelCursor.color = color;
            voxelCursor.AlphaFluctuate();

            //Update the HUD
            hud.Update(gameTime, ms);

            base.Update(gameTime);
        }

        //The following 3 methods are for placing and removing voxels
        //While visually, you wouldn't see anything with just adding a voxel to the list
        //You could basically put an infinite amount in one spot
        //Them there's some bad news bears
        private Voxel CheckForVoxel() {

            Voxel returnVoxel = null;

            foreach (Voxel voxel in voxelList) {
                if (voxel.world == voxelCursor.world) {
                    returnVoxel = voxel;
                    continue;
                }
            }

            return returnVoxel;

        }

        private void PlaceVoxel() {
            Voxel placementVoxel = CheckForVoxel();
            Voxel voxelToAdd;

            if (placementVoxel == null) {
                voxelToAdd = new Voxel(voxelModel, voxelCursor.world, voxelTex, color);
                voxelList.Add(voxelToAdd);
                Console.WriteLine(voxelToAdd);
            }
            else {
                placementVoxel.color = color;
            }

        }

        private void RemoveVoxel() {
            Voxel voxelToDelete = CheckForVoxel();
            if (voxelToDelete != null) {
                voxelList.Remove(voxelToDelete);
            }
        }

        public void LoadVoxelList(List<Matrix> voxelPositionList,
            List<Vector3> voxelColorList) {

            voxelList.Clear();

            for (int i = 0; i < voxelPositionList.Count; i++) {
                voxelList.Add(new Voxel(voxelModel, voxelPositionList[i],
                        voxelTex, voxelColorList[i]));
            }
        }


        //Much appreciation to Joel Martinez of Stack Overflow for showing me this
        //This makes it so movement isn't done every time update rolls around and you're holding a key
        //Instead, it does it once, then waits a tick to see if you really wanted to hold it
        //Then it does it once every 5th update
        protected void MovementInputCheck(KeyboardState keyboardState, KeyboardState oldState,
            Keys key, Vector3 direction) {

            Vector3 voxelCursorPosition;

            if (keyboardState.IsKeyDown(key) && !oldState.IsKeyDown(key)) {
                voxelCursor.world *= Matrix.CreateTranslation(direction);

                voxelCursorPosition = new Vector3(voxelCursor.world.M41,
                    voxelCursor.world.M43, -(voxelCursor.world.M42));

                camera.target = voxelCursorPosition;
            }
            else if (keyboardState.IsKeyDown(key) && oldState.IsKeyDown(key)) {
                counter++;
                if (counter > 40 && (counter % 5 == 0))
                    voxelCursor.world *= Matrix.CreateTranslation(direction);

                voxelCursorPosition = new Vector3(voxelCursor.world.M41,
                    voxelCursor.world.M43, -(voxelCursor.world.M42));

                camera.target = voxelCursorPosition;
            }
            else if (!keyboardState.IsKeyDown(key) && oldState.IsKeyDown(key)) {
                counter = 0;
            }

        }

        //Event handler for when the client changes the camera size
        //Have to update the camera for when that happens
        void WindowSizeChanged(object sender, EventArgs e) {
            camera.UpdateProjection(this);

            hud.Resize(this);
        }

        #endregion

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            spriteBatch.Begin();
            Color c = new Color(240, 241, 238);
            GraphicsDevice.Clear(c);

            //This is for alpha'd textures, such as my see-through cursor
            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            //This is because, I have no idea why, XNA vomits when you
            //draw 2D things over 3D things. Thanks to Shawn Hargreaves
            //for his blog post about this
            //http://blogs.msdn.com/b/shawnhar/archive/2006/11/13/spritebatch-and-renderstates.aspx
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.AlphaTestEnable = false;

            backdrop.Draw(camera);
            floor.Draw(GraphicsDevice, camera);

            foreach (Voxel voxel in voxelList) {
                voxel.Draw(camera);
            }

            voxelCursor.Draw(camera);

            hud.Draw(color);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}