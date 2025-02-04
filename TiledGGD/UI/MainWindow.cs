using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TiledGGD.BindingTools;
using TiledGGD.UI;

namespace TiledGGD
{
    public partial class MainWindow : Form
    {

        private static GraphicsData graphicsData;
        internal static GraphicsData GraphData { get { return graphicsData; } }
        private static PaletteData paletteData;
        internal static PaletteData PalData { get { return paletteData; } }

        private static MainWindow mainWindow;

        private static BindingSet bindingSet;
        internal static BindingSet BindingSet { get { return bindingSet; } }

        #region constructor
        public MainWindow()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            mainWindow = this;

            paletteData = new PaletteData(PaletteFormat.FORMAT_2BPP, PaletteOrder.BGR);
            graphicsData = new GraphicsData(paletteData);           

            this.Icon = new Icon(this.GetType().Assembly.GetManifestResourceStream("TiledGGD.program_icon.ico"));

            updateMenu();
        }

        static MainWindow()
        {
            bindingSet = new BindingSet(); // load the default binding set
        }
        #endregion

        #region Methods: Quit
        void Quit(object sender, EventArgs e)
        {
            MainWindow.Quit();
        }
        internal static void Quit()
        {
            Application.Exit();
        }
        #endregion

        #region KeyDown event handler
        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.P: if (e.Shift) paletteData.TogglePaletteOrder(); break;
                case Keys.O: paletteData.toggleFormat(); break;
                case Keys.End: paletteData.DoSkip(true); break;
                case Keys.Home: paletteData.DoSkip(false); break;
                case Keys.Up:
					if(e.Shift)
						GraphicsData.Zoom *= 2;
					else
						graphicsData.decreaseHeight();
					break;
                case Keys.Down:
					if (e.Shift)
						GraphicsData.Zoom /= 2;
					else
						graphicsData.increaseHeight();
					break;
                case Keys.Left:
					if(e.Shift)
						graphicsData.Offset--;
					else
						graphicsData.decreaseWidth();
					break;
                case Keys.Right:
					if (e.Shift)
						graphicsData.Offset++;
					else
						graphicsData.increaseWidth();
					break;
                case Keys.PageDown: graphicsData.DoSkip(true); break;
                case Keys.PageUp: graphicsData.DoSkip(false); break;
                case Keys.B: graphicsData.toggleGraphicsFormat(); break;
                case Keys.F:
					if (e.Shift)
						paletteData.toggleTiled();
					else
						graphicsData.toggleTiled();
					break;
                case Keys.E:
					if (e.Control)
						graphicsData.toggleEndianness();
					else if (e.Shift)
						paletteData.toggleEndianness();
					else
						return;
					break;
                case Keys.Z:
					if (e.Control)
						graphicsData.toggleSkipSize();
					else if (e.Shift)
						paletteData.toggleSkipSize();
					else
						return; 
break;
                case Keys.W: graphicsData.toggleWidthSkipSize(); break;
                case Keys.H: graphicsData.toggleHeightSkipSize(); break;
            }
            updateMenu();
            DoRefresh();
        }
        #endregion

        #region method: updateMenu
        /// <summary>
        /// Updates the checks in the menu
        /// </summary>
        private void updateMenu()
        {
            // graphics format
            foreach (ToolStripMenuItem tsme in this.graphFormatTSMI.DropDownItems)
                tsme.Checked = false;
            (this.graphFormatTSMI.DropDownItems[(int)GraphicsData.GraphFormat - 1] as ToolStripMenuItem).Checked = true;

            // palette format
            foreach (ToolStripMenuItem tsme in this.palFormatTSMI.DropDownItems)
                tsme.Checked = false;
            (this.palFormatTSMI.DropDownItems[(int)PaletteData.PalFormat - 5] as ToolStripMenuItem).Checked = true;

            // graphics endianness
            graphEndian_littleTSMI.Checked = !(graphEndian_bigTSMI.Checked = GraphicsData.IsBigEndian);

            // palette endianness
            palEndian_littleTSMI.Checked = !(palEndian_bigTSMI.Checked = PaletteData.IsBigEndian);

            // graphics mode
            graphMode_LinearTSMI.Checked = !(graphMode_tiledTSMI.Checked = GraphicsData.Tiled);

            // graphics mode
            palMode_LinearTSMI.Checked = !(palMode_TiledTSMI.Checked = PaletteData.Tiled);

            // graphics skip size
            foreach (ToolStripMenuItem tsme in this.graphSSTSMI.DropDownItems)
                tsme.Checked = false;
            (this.graphSSTSMI.DropDownItems[(int)GraphicsData.SkipSize] as ToolStripMenuItem).Checked = true;

            // palette order
            foreach (ToolStripMenuItem tsme in palOrderTSMI.DropDownItems)
                tsme.Checked = false;
            (palOrderTSMI.DropDownItems[(int)PaletteData.PalOrder] as ToolStripMenuItem).Checked = true;

            // palette skip size
            foreach (ToolStripMenuItem tsme in this.palSSTSMI.DropDownItems)
                tsme.Checked = false;
            switch (PaletteData.SkipMetric)
            {
                case PaletteSkipMetric.METRIC_BYTES:
                    switch (PaletteData.SkipSize)
                    {
                        case 1: palSS_1byteTSMI.Checked = true; break;
                        case 0x10000: palSS_64kbytesTSMI.Checked = true; break;
                        default: throw new Exception("Unknown palette skip size: " + PaletteData.SkipSize + " bytes");
                    }
                    break;
                case PaletteSkipMetric.METRIC_COLOURS:
                    switch (PaletteData.SkipSize)
                    {
                        case 1: palSS_1colTSMI.Checked = true; break;
                        case 16: palSS_16colTSMI.Checked = true; break;
                        case 256: palSS_256colTSMI.Checked = true; break;
                        default: throw new Exception("Unknown palette skip size: " + PaletteData.SkipSize + " colours");
                    }
                    break;
                default: throw new Exception("Unknown palette skip metric: " + PaletteData.SkipMetric.ToString());
            }

            // width skip size
            foreach (ToolStripMenuItem tsmi in this.graphWSSTSMI.DropDownItems)
                tsmi.Checked = false;
            (this.graphWSSTSMI.DropDownItems[(int)GraphicsData.WidthSkipSize] as ToolStripMenuItem).Checked = true;

            // height skip size
            foreach (ToolStripMenuItem tsmi in this.graphHSSTSMI.DropDownItems)
                tsmi.Checked = false;
            (this.graphHSSTSMI.DropDownItems[(int)GraphicsData.HeightSkipSize] as ToolStripMenuItem).Checked = true;

            // zoom
            foreach (ToolStripMenuItem tsmi in this.zoomTSMI.DropDownItems)
                tsmi.Checked = false;
            (this.zoomTSMI.DropDownItems[(int)Math.Floor(Math.Log(GraphicsData.Zoom, 2))] as ToolStripMenuItem).Checked = true;

            // update the data panel, just to be sure
            DataPanel_Paint(this, null);
        }
        #endregion

        #region drag methods
        void PalettePanel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string fname = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                paletteData.load(fname);
                if (!graphicsData.HasData)
                    graphicsData.load(fname);
                DoRefresh();
            }
            catch { }
        }

        void GraphicsPanel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string fname = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                graphicsData.load(fname);
                if (!paletteData.HasData)
                    paletteData.load(fname);
                DoRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        void palGraphDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void DataPanel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string fname = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                graphicsData.load(fname);
                paletteData.load(fname);
                DoRefresh();
            }
            catch { }
        }
        #endregion

        #region paint methods
        void DataPanel_Paint(object sender, PaintEventArgs e)
        {
            string val;

            #region graphics data
            this.listBox1.Items.Clear();

            this.listBox1.Items.Add("Graphics Data:");

            this.listBox1.Items.Add("Source File: \t" + GraphicsData.Filename);

            this.listBox1.Items.Add("Offset:\t\t0x" + String.Format("{0:X}", graphicsData.Offset));

            this.listBox1.Items.Add("Panel Size:\t" + GraphicsData.PanelWidth + " x " + GraphicsData.PanelHeight);

            this.listBox1.Items.Add("Tile Size:\t\t" + GraphicsData.TileSize.X + " x " + GraphicsData.TileSize.Y);

            switch (GraphicsData.GraphFormat)
            {
                case GraphicsFormat.FORMAT_1BPP: val = "1 bits/pixel"; break;
                case GraphicsFormat.FORMAT_2BPP: val = "2 bits/pixel"; break;
                case GraphicsFormat.FORMAT_4BPP: val = "4 bits/pixel"; break;
                case GraphicsFormat.FORMAT_8BPP: val = "8 bits/pixel"; break;
                case GraphicsFormat.FORMAT_16BPP: val = "16 bits/pixel"; break;
                case GraphicsFormat.FORMAT_24BPP: val = "24 bits/pixel"; break;
                case GraphicsFormat.FORMAT_32BPP: val = "32 bits/pixel"; break;
                default: val = "ERROR"; break;
            }
            this.listBox1.Items.Add("Format:\t\t" + val);

            switch (GraphicsData.IsBigEndian)
            {
                case true: val = "Big Endian"; break;
                case false: val = "Little Endian"; break;
                default: val = "ERROR"; break;
            }
            this.listBox1.Items.Add("Endianness:\t" + val);

            switch (GraphicsData.Tiled)
            {
                case true: val = "Tiled"; break;
                case false: val = "Linear"; break;
                default: val = "ERROR"; break;
            }
            this.listBox1.Items.Add("Mode:\t\t" + val);

            switch (GraphicsData.SkipSize)
            {
                case GraphicsSkipSize.SKIPSIZE_1BYTE: val = "1 Bytes"; break;
                case GraphicsSkipSize.SKIPSIZE_2BYTES: val = "2 Bytes"; break;
                case GraphicsSkipSize.SKIPSIZE_4BYTES: val = "4 Bytes"; break;
                case GraphicsSkipSize.SKIPSIZE_1TILE: val = "1 Tile"; break;
                case GraphicsSkipSize.SKIPSIZE_1PIXROW: val = "1 row of pixels"; break;
                case GraphicsSkipSize.SKIPSIZE_1TILEROW: val = "1 row of tiles"; break;
                case GraphicsSkipSize.SKIPSIZE_HEIGHTROWS: val = "(Height) rows of pixels"; break;
                case GraphicsSkipSize.SKIPSIZE_WIDTHROWS: val = "(Width) rows of pixels"; break;
                default: val = "ERROR"; break;
            }
            this.listBox1.Items.Add("Skip Size:\t" + val);

            if (GraphicsData.WidthSkipSize != HWSkipSize.SKIPSIZE_1TILE)
                this.listBox1.Items.Add("Width Skip Size:\t" + GraphicsData.WidthSkipSizeUInt + " pixels");
            else
                this.listBox1.Items.Add("Width Skip Size:\t1 Tile");
            if (GraphicsData.HeightSkipSize != HWSkipSize.SKIPSIZE_1TILE)
                this.listBox1.Items.Add("Height Skip Size:\t" + GraphicsData.HeightSkipSizeUInt + " pixels");
            else
                this.listBox1.Items.Add("Height Skip Size:\t1 Tile");
            #endregion

            #region palette data
            this.listBox2.Items.Clear();

            this.listBox2.Items.Add("Palette Data:");

            this.listBox2.Items.Add("Source File: \t" + PaletteData.Filename);

            this.listBox2.Items.Add("Offset:\t\t0x" + String.Format("{0:X}", paletteData.Offset));

            this.listBox2.Items.Add(string.Format("Tile Size: \t{0:g} x {1:g}", PaletteData.TileSize.X, PaletteData.TileSize.Y));

            switch (PaletteData.PalFormat)
            {
                case PaletteFormat.FORMAT_2BPP: val = "2 Bytes/colour"; break;
                case PaletteFormat.FORMAT_3BPP: val = "3 Bytes/colour"; break;
                case PaletteFormat.FORMAT_4BPP: val = "4 Bytes/colour"; break;
                default: val = "ERROR"; break;
            }
            this.listBox2.Items.Add("Format:\t\t" + val);

            val = PaletteData.PalOrder.ToString();
            this.listBox2.Items.Add("Colour Order:\t" + val);

            switch (PaletteData.IsBigEndian)
            {
                case true: val = "Big Endian"; break;
                case false: val = "Little Endian"; break;
                default: val = "ERROR"; break;
            }
            this.listBox2.Items.Add("Endianness:\t" + val);

            this.listBox2.Items.Add(string.Format("Mode: \t\t{0:s}", PaletteData.Tiled ? "Tiled" : "Linear"));

            switch (PaletteData.AlphaSettings.Location)
            {
                case AlphaLocation.START: val = "Start"; break;
                case AlphaLocation.END: val = "End"; break;
                default: val = "ERROR"; break;
            }
            if (PaletteData.AlphaSettings.IgnoreAlpha)
                val += ", but ignored";
             else if (PaletteData.AlphaSettings.Stretch)
                val += string.Format(" [0x{0:X}, 0x{1:X}]", PaletteData.AlphaSettings.Minimum, PaletteData.AlphaSettings.Maximum);
            this.listBox2.Items.Add("Alpha Location:\t" + val);

            switch (PaletteData.SkipMetric)
            {
                case PaletteSkipMetric.METRIC_BYTES:
                    if (PaletteData.SkipSize == 1) val = "1 Byte";
                    else if (PaletteData.SkipSize == 0x10000) val = "64k Bytes";
                    else val = "ERROR";
                    break;
                case PaletteSkipMetric.METRIC_COLOURS:
                    if (PaletteData.SkipSize == 1) val = "1 Colour";
                    else val = PaletteData.SkipSize + " Colours";
                    break;
                default: val = "ERROR"; break;
            }
            this.listBox2.Items.Add("Skip Size:\t" + val);



            #endregion
        }

        void PalettePanel_Paint(object sender, PaintEventArgs e)
        {
            paletteData.paint(this, e);
        }

        void GraphicsPanel_Paint(object sender, PaintEventArgs e)
        {
            graphicsData.paint(this, e);
        }
        #endregion

        #region method: DoRefresh
        /// <summary>
        /// Refresh the Main Window
        /// </summary>
        public static void DoRefresh()
        {
            mainWindow.Refresh();
        }
        #endregion

        /// <summary>
        /// Shows an error message
        /// </summary>
        /// <param name="p">The string to show as an error</param>
        internal static void ShowError(string p)
        {
            MessageBox.Show(mainWindow, p, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }
        /// <summary>
        /// Shows a warning message
        /// </summary>
        /// <param name="w">The string to show as a warning</param>
        internal static void ShowWarning(string w)
        {
            MessageBox.Show(mainWindow, w, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Gets the current path
        /// </summary>
        /// <returns></returns>
        internal static string getPath()
        {
            string fl = typeof(MainWindow).Assembly.Location;
            return fl.Substring(0, fl.LastIndexOfAny(new char[] { '/', '\\' })) + "/";
        }

        #region toolstrip response methods

        #region about box
        private void aboutTSMI_Click(object sender, EventArgs e)
        {
            if (this.aboutBox == null || this.aboutBox.IsDisposed)
                this.aboutBox = new AboutBox();
            this.aboutBox.Visible = true;
        }
        #endregion

        #region graphical format
        private void graphicalFormatTSMI_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem tsme in this.graphFormatTSMI.DropDownItems)
                tsme.Checked = false;
            (sender as ToolStripMenuItem).Checked = true;

            if (sender == graphFormat_1bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_1BPP;
            else if (sender == graphFormat_2bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_2BPP;
            else if (sender == graphFormat_4bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_4BPP;
            else if (sender == graphFormat_8bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_8BPP;
            else if (sender == graphFormat_16bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_16BPP;
            else if (sender == graphFormat_24bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_24BPP;
            else if (sender == graphFormat_32bppTSMI)
                GraphicsData.GraphFormat = GraphicsFormat.FORMAT_32BPP;
            else
                throw new Exception("Invalid Graphcial format action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region copy to clipboard
        private void copyToClipboard(object sender, EventArgs e)
        {
            if (sender == copyGraphicsToolStripMenuItem)
                graphicsData.copyToClipboard();
            else if (sender == copyPaletteToolStripMenuItem)
                paletteData.copyToClipboard();
            else
                throw new Exception("Invalid Copy To Clipboard action");
        }
        #endregion

        #region graphical mode
        private void graphicalModeTSMI_Click(object sender, EventArgs e)
        {
            if (sender == graphMode_LinearTSMI)
                GraphicsData.Tiled = false;
            else if (sender == graphMode_tiledTSMI)
                GraphicsData.Tiled = true;
            else
                throw new Exception("Invalid Linear/Tiled action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region palette mode
        private void paletteModeTSMI_CLick(object sender, EventArgs e)
        {
            if (sender == palMode_LinearTSMI)
                PaletteData.Tiled = false;
            else if (sender == palMode_TiledTSMI)
                PaletteData.Tiled = true;
            else
                throw new Exception("Invalid Linear/Tiled action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region shortcuts
        private void shortcutsTSMI_Click(object sender, EventArgs e)
        {
            if (this.controlShortBox == null || this.controlShortBox.IsDisposed)
                this.controlShortBox = new ControlShorts();
            this.controlShortBox.Visible = true;
        }
        #endregion

        #region graphical endianness
        private void graphEndianTSMI_Click(object sender, EventArgs e)
        {
            if (sender == graphEndian_bigTSMI)
                GraphicsData.IsBigEndian = true;
            else if (sender == graphEndian_littleTSMI)
                GraphicsData.IsBigEndian = false;
            else
                throw new Exception("Invalid graphical endianness action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region graphical skip size
        private void graphSSTSMI_Click(object sender, EventArgs e)
        {
            if(graphSSTSMI.DropDownItems.Contains(sender as ToolStripMenuItem))
                GraphicsData.SkipSize = (GraphicsSkipSize)graphSSTSMI.DropDownItems.IndexOf(sender as ToolStripMenuItem);
            else
                throw new Exception("Invalid Graphics Skip Size action");

            updateMenu();
        }
        #endregion

        #region palette format
        private void palFormatTSMI_Click(object sender, EventArgs e)
        {
            if (sender == palFormat_2BpcTSMI)
                PaletteData.PalFormat = PaletteFormat.FORMAT_2BPP;
            else if (sender == palFormat_3BpcTSMI)
                PaletteData.PalFormat = PaletteFormat.FORMAT_3BPP;
            else if (sender == palFormat_4BpcTSMI)
                PaletteData.PalFormat = PaletteFormat.FORMAT_4BPP;
            else
                throw new Exception("Invalid Palette Format action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region palette endianness
        private void palEndianTSMI_Click(object sender, EventArgs e)
        {
            if (sender == palEndian_bigTSMI)
                PaletteData.IsBigEndian = true;
            else if (sender == palEndian_littleTSMI)
                PaletteData.IsBigEndian = false;
            else
                throw new Exception("Invalid Palette Endianness action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region palette order
        private void palOrderTSMI_Click(object sender, EventArgs e)
        {
            if (sender == palOrder_bgrTSMI)
                PaletteData.PalOrder = PaletteOrder.BGR;
            else if (sender == palOrder_brgTSMI)
                PaletteData.PalOrder = PaletteOrder.BRG;
            else if (sender == palOrder_gbrTSMI)
                PaletteData.PalOrder = PaletteOrder.GBR;
            else if (sender == palOrder_grbTSMI)
                PaletteData.PalOrder = PaletteOrder.GRB;
            else if (sender == palOrder_rbgTSMI)
                PaletteData.PalOrder = PaletteOrder.RBG;
            else if (sender == palOrder_rgbTSMI)
                PaletteData.PalOrder = PaletteOrder.RGB;
            else
                throw new Exception("Invalid palette order action");
            DoRefresh();
            updateMenu();
        }
        #endregion

        #region palette skip size
        private void palSSTSMI_Click(object sender, EventArgs e)
        {
            if (sender == palSS_1byteTSMI)
            {
                PaletteData.SkipSize = 1;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_BYTES;
            }
            else if (sender == palSS_1colTSMI)
            {
                PaletteData.SkipSize = 1;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_COLOURS;
            }
            else if (sender == palSS_16colTSMI)
            {
                PaletteData.SkipSize = 16;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_COLOURS;
            }
            else if (sender == palSS_256colTSMI)
            {
                PaletteData.SkipSize = 256;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_COLOURS;
            }
            else if (sender == palSS_64kbytesTSMI)
            {
                PaletteData.SkipSize = 0x10000;
                PaletteData.SkipMetric = PaletteSkipMetric.METRIC_BYTES;
            }
            else
                throw new Exception("Invalid palette skip size action");
            updateMenu();
        }
        #endregion

        #region save graphics
        private void saveGraphTSMI_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG file (*.png)|*.png";
            sfd.DefaultExt = "png";
            sfd.Title = "Save Graphics as PNG";
            sfd.SupportMultiDottedExtensions = true;
            sfd.ShowHelp = false;
            sfd.OverwritePrompt = true;
            sfd.AddExtension = true;
            sfd.RestoreDirectory = true;
            sfd.FileName = GraphicsData.Filename + ".png";
            DialogResult res = sfd.ShowDialog();

            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                string flnm = sfd.FileName;
                if (!flnm.ToLower().EndsWith(".png"))
                    flnm += ".png";
                graphicsData.toBitmap().Save(flnm, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        #endregion

        #region save palette
        private void savePalTSMI_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG file (*.png)|*.png";
            sfd.DefaultExt = "png";
            sfd.Title = "Save Palette as PNG";
            sfd.SupportMultiDottedExtensions = true;
            sfd.ShowHelp = false;
            sfd.OverwritePrompt = true;
            sfd.AddExtension = true;
            sfd.RestoreDirectory = true;
            sfd.FileName = PaletteData.Filename;
            DialogResult res = sfd.ShowDialog();

            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                string flnm = sfd.FileName;
                if (!flnm.ToLower().EndsWith(".png"))
                    flnm += ".png";
                paletteData.toBitmap().Save(flnm);
            }
        }
        #endregion

        #region load graphics
        private void openGraphTSMI_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Any file (*.*)|*.*";
            ofd.RestoreDirectory = true;
            ofd.ShowHelp = false;
            ofd.Multiselect = false;
            ofd.Title = "Open file as Graphics";
            DialogResult res = ofd.ShowDialog();

			if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                graphicsData.load(ofd.FileName);
                if (!paletteData.HasData)
                    paletteData.load(ofd.FileName);
                DoRefresh();
            }
        }
		#endregion

		#region Batch Convert graphics
		private void batchConvertGraphTSMI_Click(object sender, EventArgs e) {
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Any file (*.*)|*.*";
			ofd.RestoreDirectory = true;
			ofd.ShowHelp = false;
			ofd.Multiselect = true;
			ofd.Title = "Batch Convert files as Graphics";
			DialogResult res = ofd.ShowDialog();

			if (res == DialogResult.OK || res == DialogResult.Yes) {

				var fdb = new FolderBrowserDialog();
				DialogResult result = fdb.ShowDialog();
				if (result == DialogResult.OK) {
					string saveFolder = fdb.SelectedPath;
					foreach (string loadPath in ofd.FileNames) {
						graphicsData.load(loadPath);
						if (!paletteData.HasData)
							paletteData.load(ofd.FileName);
						string savePath = Path.Combine(saveFolder, Path.GetFileNameWithoutExtension(loadPath) + ".png");
						graphicsData.toBitmap().Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
					}
				}
			}
		}
		#endregion

		#region load palette
		private void openPalTSMI_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Any file (*.*)|*.*";
            ofd.RestoreDirectory = true;
            ofd.ShowHelp = false;
            ofd.Multiselect = false;
            ofd.Title = "Open file as Palette";
            DialogResult res = ofd.ShowDialog();
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                paletteData.load(ofd.FileName);
                if (!graphicsData.HasData)
                    graphicsData.load(ofd.FileName);
                DoRefresh();
            }
        }
        #endregion

        #region width skip size
        private void graphWSSTSMI_Click(object sender, EventArgs e)
        {
            if (sender == graphWSS_1pixTSMI)
                GraphicsData.WidthSkipSize = HWSkipSize.SKIPSIZE_1PIX;
            else if (sender == graphWSS_2pixTSMI)
                GraphicsData.WidthSkipSize = HWSkipSize.SKIPSIZE_2PIX;
            else if (sender == graphWSS_4pixTSMI)
                GraphicsData.WidthSkipSize = HWSkipSize.SKIPSIZE_4PIX;
            else if (sender == graphWSS_8pixTSMI)
                GraphicsData.WidthSkipSize = HWSkipSize.SKIPSIZE_8PIX;
            else if (sender == graphWSS_16pixTSMI)
                GraphicsData.WidthSkipSize = HWSkipSize.SKIPSIZE_16PIX;
            else if (sender == graphWSS_1tileTSMI)
                GraphicsData.WidthSkipSize = HWSkipSize.SKIPSIZE_1TILE;
            else
                throw new Exception("Invalid graphics width skip size action");
            DoRefresh();
        }
        #endregion

        #region height skip size
        private void graphHSSTSMI_Click(object sender, EventArgs e)
        {
            if (sender == graphHSS_1pixTSMI)
                GraphicsData.HeightSkipSize = HWSkipSize.SKIPSIZE_1PIX;
            else if (sender == graphHSS_2pixTSMI)
                GraphicsData.HeightSkipSize = HWSkipSize.SKIPSIZE_2PIX;
            else if (sender == graphHSS_4pixTSMI)
                GraphicsData.HeightSkipSize = HWSkipSize.SKIPSIZE_4PIX;
            else if (sender == graphHSS_8pixTSMI)
                GraphicsData.HeightSkipSize = HWSkipSize.SKIPSIZE_8PIX;
            else if (sender == graphHSS_16pixTSMI)
                GraphicsData.HeightSkipSize = HWSkipSize.SKIPSIZE_16PIX;
            else if (sender == graphHSS_1tileTSMI)
                GraphicsData.HeightSkipSize = HWSkipSize.SKIPSIZE_1TILE;
            else
                throw new Exception("Invalid graphics height skip size action");
            updateMenu();
        }
		#endregion

		#region panel size
		private void setPanelSizeTSMI_Click(object sender, EventArgs e) {
			SizeDialog tsd = new SizeDialog(new Point((int) GraphicsData.PanelWidth, (int) GraphicsData.PanelHeight));
			tsd.ShowDialog();
			Point p = tsd.NewSize;
			if (((p.X | p.Y) & 1) == 0) {
				GraphicsData.PanelWidth = (uint)p.X;
				GraphicsData.PanelHeight = (uint)p.Y;
				this.DataPanel_Paint(this, null);
			} else {
				MessageBox.Show("The dimensions of a tile can not be odd", "Invalid Tile Size");
			}
		}
		#endregion

		#region tile size
		private void setTileSizeTSMI_Click(object sender, EventArgs e)
        {
            SizeDialog tsd = new SizeDialog(GraphicsData.TileSize);
            tsd.ShowDialog();
            Point p = tsd.NewSize;
            if (((p.X | p.Y) & 1) == 0)
            {
                GraphicsData.TileSize = tsd.NewSize;
                this.DataPanel_Paint(this, null);
            }
            else
            {
                MessageBox.Show("The dimensions of a tile can not be odd", "Invalid Tile Size");
            }
        }

        private void setTileSizePalTSMI_Click(object sender, EventArgs e)
        {
            SizeDialog tsd = new SizeDialog(PaletteData.TileSize);
            tsd.ShowDialog();
            PaletteData.TileSize = tsd.NewSize;
            this.DataPanel_Paint(this, null);
        }
        #endregion

        #region go to
        private void goToTSMI_Click(object sender, EventArgs e)
        {
            GoToOffsetDialog gtod = new GoToOffsetDialog();
            gtod.ShowDialog();
            bool rel;
            long off;
            BrowsableData bd;
            if (sender == graphGoToTSMI)
                bd = graphicsData;
            else if (sender == palGoToTSMI)
                bd = paletteData;
            else
                throw new Exception("Extremely improbably Exception");
            if (gtod.getResult(out rel, out off))
            {
                if (rel)
                    bd.Offset += off;
                else
                    bd.Offset = off;
            }
            this.BringToFront();
            DoRefresh();
        }
        #endregion

        #region zoom
        private void zoomTSMI_Click(object sender, EventArgs e)
        {
            if (!zoomTSMI.DropDownItems.Contains(sender as ToolStripMenuItem))
                throw new Exception("Only drop-down items from Zoom can call zoomTSMI_Click");
            GraphicsData.Zoom = (int)Math.Pow(2, zoomTSMI.DropDownItems.IndexOf(sender as ToolStripMenuItem));
            DoRefresh();
        }
        #endregion

        #region reload
        private void dataReloadTSMI_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                throw new Exception("Stupid error exception");
            bool specific = (tsmi.OwnerItem as ToolStripMenuItem).DropDownItems.IndexOf(tsmi) == 1;
            if (tsmi.OwnerItem == this.menuImageReload)
                graphicsData.reload(specific);
            else if (tsmi.OwnerItem == this.palReloadTSMI)
                paletteData.reload(specific);
            else throw new Exception("Stupid error exception");
        }
        #endregion

        #region Save all Graphics
        private void saveallGraphTSMI_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG file (*.png)|*.png";
            sfd.DefaultExt = "png";
            sfd.Title = "Save Graphics as PNG";
            sfd.SupportMultiDottedExtensions = true;
            sfd.ShowHelp = false;
            sfd.OverwritePrompt = true;
            sfd.AddExtension = true;
            sfd.RestoreDirectory = true;
            sfd.FileName = GraphicsData.Filename;
            DialogResult res = sfd.ShowDialog();

            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                string flnm = sfd.FileName;
                if (!flnm.ToLower().EndsWith(".png"))
                    flnm += ".png";

                try
                {
                    graphicsData.toFullBitmap().Save(flnm, System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    ShowError("Unable to save all graphics, possibly\nbecause the file is too large.");
                }
            }
        }
        #endregion

        #region Reload the binding set
        private void reloadBindingsTSMI_Click(object sender, EventArgs e)
        {
            try
            {
                BindingSet.Reload();
                MessageBox.Show("Bindings succesfully reloaded");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to reload the bindings, the following exception happened:\n"
                + ex.Message + "\n"
                + ex.StackTrace);
            }
            
        }
        #endregion

        private void palAlphaTSMI_Click(object sender, EventArgs e)
        {
            AlphaPanel panel = new AlphaPanel(PaletteData.AlphaSettings);
            panel.ShowDialog(); // the panel will automatically alter the settings when OK is clicked / enter is pressed
        }


        #endregion


    }
}