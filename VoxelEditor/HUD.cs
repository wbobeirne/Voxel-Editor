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

    //This is my delegate I pass buttons
    //It needs to be cs wide so that both of
    //em know what it is
    public delegate void ButtonClick();

    public class HUD : Microsoft.Xna.Framework.GameComponent {

        SpriteBatch spriteBatch;

        int windowSizeX;
        int windowSizeY;
        int verticalPosition;

        Vector3 color;

        Texture2D hudBarTex;

        //Button stuff
        List<Button> buttonList = new List<Button>();

        Button colorButton;
        Button saveButton;
        Button loadButton;

        Texture2D colorButtonTex;
        Texture2D saveButtonTex;
        Texture2D loadButtonTex;

        ButtonClick colorButtonClick;
        ButtonClick saveButtonClick;
        ButtonClick loadButtonClick;

        public HUD(Game1 game, SpriteBatch sb)
            : base(game) {

            windowSizeX = Game.Window.ClientBounds.Width;
            windowSizeY = Game.Window.ClientBounds.Height;

            verticalPosition = windowSizeY - 40;

            colorButtonClick = new ButtonClick(game.dialogBox.ColorSelection);
            saveButtonClick = new ButtonClick(game.dialogBox.SaveDialog);
            loadButtonClick = new ButtonClick(game.dialogBox.LoadDialog);

            spriteBatch = sb;

        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize() {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        public void LoadContent(ContentManager Content, GraphicsDevice graphics) {

            hudBarTex = Content.Load<Texture2D>("Textures\\HUD\\HUDbar");
            colorButtonTex = Content.Load<Texture2D>("Textures\\HUD\\ColorButtonFull");
            saveButtonTex = Content.Load<Texture2D>("Textures\\HUD\\SaveButton");
            loadButtonTex = Content.Load<Texture2D>("Textures\\HUD\\LoadButton");

            LoadButtons();

        }

        public void LoadButtons(){

            Console.WriteLine(colorButtonClick);

            colorButton = new Button(this, colorButtonTex, colorButtonClick);
            saveButton = new Button(this, saveButtonTex, saveButtonClick);
            loadButton = new Button(this, loadButtonTex, loadButtonClick);

            buttonList.Add(colorButton);
            buttonList.Add(saveButton);
            buttonList.Add(loadButton);

            for (int i = 0; i < buttonList.Count; i++){

                Vector2 pos = new Vector2((windowSizeX / buttonList.Count) * (i + 1), verticalPosition);

                buttonList[i].position = pos;

            }

        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime, MouseState ms)
        {

            verticalPosition = windowSizeY - 60;

            //Update the positions for the buttons
            for (int i = 0; i < buttonList.Count; i++){

                Vector2 pos = new Vector2((windowSizeX / (buttonList.Count + 1)) *
                    (i + 1), verticalPosition);

                buttonList[i].position = pos;

                //Checks for being clicked, send the mouse state
                buttonList[i].Update(ms);

            }

            base.Update(gameTime);
        }

        public void Resize(Game game) {
            windowSizeX = Game.Window.ClientBounds.Width;
            windowSizeY = Game.Window.ClientBounds.Height;
        }

        public void Draw(Vector3 col) {

            color = col;

            //This draws the 1 pixel wide HUD bar along the bottom of the screen based
            //On the width of the current window
            for (int i = 0; i < windowSizeX; i++) {
                spriteBatch.Draw(hudBarTex, new Vector2(i, windowSizeY - hudBarTex.Height / 2), null, Color.White,
                    0, new Vector2(hudBarTex.Width / 2, hudBarTex.Height / 2), 1, SpriteEffects.None, 0);
            }

            //Draw all the buttons
            foreach (Button button in buttonList) {
                button.Draw(spriteBatch);
            }
        }
    }

    public class Button{

        HUD hud;
        Texture2D texture;
        Rectangle boundingBox;
        public Vector2 position { get; set; }
        Vector2 center;

        ButtonClick click;

        //To check against new mouse states
        MouseState msOld;

        public Button(HUD h, Vector2 pos, Texture2D tex, ButtonClick c){

            hud = h;
            position = pos;
            texture = tex;
            click = c;

            center = new Vector2(texture.Width / 2, texture.Height / 2);

        }

        public Button(HUD h, Texture2D tex, ButtonClick c) {

            hud = h;
            position = Vector2.Zero;
            texture = tex;
            click = c;

            center = new Vector2(texture.Width / 2, texture.Height / 2);

        }

        public Rectangle CreateBoundingBox() {

            Rectangle bb = new Rectangle((int)position.X - (int)center.X, (int)position.Y,
                texture.Width, texture.Height);

            return bb;

        }

        public void Click(){

            click();

        }

        public void Update(MouseState ms) {

            boundingBox = CreateBoundingBox();

            Rectangle mouseRec = new Rectangle(ms.X, ms.Y, 1, 1);

            if((ms.LeftButton == ButtonState.Pressed) &&
                (msOld.LeftButton == ButtonState.Released) &&
                (boundingBox.Intersects(mouseRec))){

                Click();

            }

            msOld = ms;

        }

        public void Draw(SpriteBatch sb){

            sb.Draw(texture, new Vector2 (position.X - center.X, position.Y), Color.White);

        }

    }

}