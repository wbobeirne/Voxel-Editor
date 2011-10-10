using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;

namespace VoxelEditor {
    public class DialogBox : Form {

        Game1 game;
        public bool InUse { get; set; }

        public DialogBox(Game1 game) {
            InUse = false;

            this.game = game;
        }

        private void FilePath_TextChanged(object sender, EventArgs e) {

        }

        public void SaveDialog() {
            // Displays a SaveFileDialog so the user can save the voxels
            this.InUse = true;

            this.Visible = false;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Voxel Model|*.vox";
            saveFileDialog1.Title = "Save a Voxel File";
            saveFileDialog1.InitialDirectory = @"Voxel Saves";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
                SaveVoxelList(saveFileDialog1.FileName, game.voxelList);

            this.InUse = false;
        }

        public void LoadDialog() {
            this.InUse = true;
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Voxel Model|*.vox";
            openFileDialog1.Title = "Load a Voxel File";
            openFileDialog1.InitialDirectory = @"Voxel Saves";
            openFileDialog1.ShowDialog();

            if (openFileDialog1.FileName != "")
                LoadVoxelList(openFileDialog1.FileName);

            this.InUse = false;
        }

        private void DialogBox_Load(object sender, EventArgs e) {

        }

        public void SaveVoxelList(string filename, List<Voxel> voxelListToSave) {
            try {
                List<Matrix> voxelPositionList = new List<Matrix>();
                foreach (Voxel voxel in voxelListToSave) {
                    voxelPositionList.Add(voxel.world);
                }
                List<Vector3> voxelColorList = new List<Vector3>();
                foreach (Voxel voxel in voxelListToSave) {
                    voxelColorList.Add(voxel.color);
                }

                Stream stream = File.Open(filename, FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, voxelPositionList);
                bf.Serialize(stream, voxelColorList);
                stream.Close();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        public void LoadVoxelList(string filename) {
            List<Matrix> voxelPositionList;
            List<Vector3> voxelColorList;
            try {
                Stream stream = File.Open(filename, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                voxelPositionList = (List<Matrix>)bf.Deserialize(stream);
                voxelColorList = (List<Vector3>)bf.Deserialize(stream);
                stream.Close();

                game.LoadVoxelList(voxelPositionList, voxelColorList);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                voxelPositionList = null;
            }
        }

        public void ColorSelection(){
            Vector3 returnColor;
            ColorDialog cd = new ColorDialog();

            this.InUse = true;
            cd.ShowDialog();
            returnColor = new Vector3((float)cd.Color.R/255, (float)cd.Color.G/255, (float)cd.Color.B/255);
            Console.WriteLine(returnColor);

            this.InUse = false;
            game.color = returnColor;

        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {

        }
    }
}
