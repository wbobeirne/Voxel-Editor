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

namespace VoxelEditor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model voxelModel;

        //Mouse states
        Vector2 mousePos;
        Vector2 mousePosOld;
        int scrollWheel;
        int scrollWheelOld;

        int counter = 0;        //These are for the update method
        KeyboardState oldState; //to stop held down keys from rapidly doing stuffs

        Camera camera;

        //Color stuffs
        Vector3 color = new Vector3(1.0f, 1.0f, 1.0f);
        Random randomColor = new Random();

        //Voxel stuffs
        List<Voxel> voxelList = new List<Voxel>();
        Voxel voxelCursor;

        float aspectRatio;

        Texture2D voxelTexture;

        Matrix placementWorld = Matrix.Identity;


        public Game1()
        {
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
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            this.IsMouseVisible = true;

            // Initialize camera
            camera = new Camera(this, new Vector3(20.0f, 50.0f, 40.0f),
                Vector3.Zero, Vector3.Up);
            Components.Add(camera);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            voxelModel = Content.Load<Model>("Models\\cube");
            voxelTexture = Content.Load<Texture2D>("Textures\\whiteTexture");

            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            voxelCursor = new Voxel(voxelModel, placementWorld,
                voxelTexture, new Vector3(1.0f, 0.0f, 0.0f));
            voxelCursor.alpha = 0.7f;
            voxelCursor.scale = 1.01f;
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
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //Keyboard based stuff
            KeyboardState keyboardState = Keyboard.GetState();

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

            oldState = keyboardState;

            //Color randomizer, a placeholder until I get some real color picking
            if (keyboardState.IsKeyDown(Keys.C))
                color = new Vector3((float)randomColor.NextDouble(),
                    (float)randomColor.NextDouble(), (float)randomColor.NextDouble());

            MouseState ms = Mouse.GetState();

            mousePos.X = ms.X;
            mousePos.Y = ms.Y;
            scrollWheel = ms.ScrollWheelValue;

            if(mousePos.X != mousePosOld.X && ms.LeftButton == ButtonState.Pressed){
                camera.Rotate((float)-(mousePos.X - mousePosOld.X)/100, Vector3.Left);
            }
            if (mousePos.Y != mousePosOld.Y && ms.LeftButton == ButtonState.Pressed) {
                camera.Rotate((float)(mousePos.Y - mousePosOld.Y)/100, Vector3.Up);
            }
            if (mousePos.X != mousePosOld.X && ms.RightButton == ButtonState.Pressed) {
                //camera.PanX((float)-(mousePos.X - mousePosOld.X)/30, new Vector3(0, 1, 0));
            }
            if (mousePos.Y != mousePosOld.Y && ms.RightButton == ButtonState.Pressed) {
                //camera.PanY((float)(mousePos.Y - mousePosOld.Y)/500, new Vector3(1, 0, 0));
            }
            if (ms.MiddleButton == ButtonState.Pressed){
                camera.Reset(new Vector3(20.0f, 50.0f, 40.0f), Vector3.Zero, Vector3.Up);
            }
            if (scrollWheel > scrollWheelOld) {
                camera.Zoom(3f);
            }
            if (scrollWheel < scrollWheelOld) {
                camera.Zoom(-3f);
            }

            mousePosOld.X = ms.X;
            mousePosOld.Y = ms.Y;
            scrollWheelOld = scrollWheel;

            //Makes the cursor fade in and out
            voxelCursor.AlphaFluctuate();

            //Update the camera
            //camera.Update(gameTime);

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

            if (placementVoxel == null) {
                voxelList.Add(new Voxel(voxelModel, voxelCursor.world, voxelTexture, color));
            }

        }

        private void RemoveVoxel() {
            Voxel voxelToDelete = CheckForVoxel();
            if (voxelToDelete != null) {
                voxelList.Remove(voxelToDelete);
            }
        }


        //Much appreciation to Joel Martinez of Stack Overflow for showing me this
        //This makes it so movement isn't done every time update rolls around and you're holding a key
        //Instead, it does it once, then waits a tick to see if you really wanted to hold it
        //Then it does it once every 5th update
        protected void MovementInputCheck(KeyboardState keyboardState, KeyboardState oldState, 
            Keys key, Vector3 direction){

            Vector3 voxelCursorPosition;

            if (keyboardState.IsKeyDown(key) && !oldState.IsKeyDown(key)) {
                voxelCursor.world *= Matrix.CreateTranslation(direction);

                voxelCursorPosition = new Vector3(voxelCursor.world.M41,
                    voxelCursor.world.M43, -(voxelCursor.world.M42));

                camera.target = voxelCursorPosition;
                Console.WriteLine(voxelCursor.world);
            }
            else if (keyboardState.IsKeyDown(key) && oldState.IsKeyDown(key)) {
                counter++;
                if(counter > 40 && (counter % 5 == 0))
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
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
  
            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            foreach (Voxel voxel in voxelList) {
                voxel.Draw(camera);
            }

            voxelCursor.Draw(camera);

            base.Draw(gameTime);
        }
    }
}
