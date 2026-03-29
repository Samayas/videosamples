using System.ComponentModel;

namespace ImageToBannerResizerV2
{
    partial class ImageForm
    {
        private System.ComponentModel.IContainer components = null;

        // Controls
        private PictureBox pictureBox;
        private Panel scrollPanel;
        private Panel bottomPanel;
        private TrackBar zoomTrackBar;
        private Label percentLabel;

        // Constants
        private const int BottomPanelHeight = 30;

        // Bitmap
        private Bitmap originalBitmap = null;

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.pictureBox.Image = null;
                this.originalBitmap?.Dispose();
                this.originalBitmap = null;

                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Initialize Component.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox = new PictureBox();
            this.scrollPanel = new Panel();
            this.percentLabel = new Label();
            this.zoomTrackBar = new TrackBar();
            this.bottomPanel = new Panel();
            ((ISupportInitialize)this.pictureBox).BeginInit();
            this.scrollPanel.SuspendLayout();
            ((ISupportInitialize)this.zoomTrackBar).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new Size(100, 50);
            this.pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // scrollPanel
            // 
            this.scrollPanel.AutoScroll = true;
            this.scrollPanel.Controls.Add(this.pictureBox);
            this.scrollPanel.Dock = DockStyle.Fill;
            this.scrollPanel.Location = new Point(0, 0);
            this.scrollPanel.Name = "scrollPanel";
            this.scrollPanel.Size = new Size(1300, 672);
            this.scrollPanel.TabIndex = 0;
            // 
            // percentLabel
            // 
            this.percentLabel.Dock = DockStyle.Right;
            this.percentLabel.Location = new Point(1250, 0);
            this.percentLabel.Name = "percentLabel";
            this.percentLabel.Size = new Size(50, 30);
            this.percentLabel.TabIndex = 1;
            this.percentLabel.Text = "100%";
            this.percentLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // zoomTrackBar
            // 
            this.zoomTrackBar.Dock = DockStyle.Fill;
            this.zoomTrackBar.LargeChange = 10;
            this.zoomTrackBar.Location = new Point(250, 0);
            this.zoomTrackBar.Maximum = 100;
            this.zoomTrackBar.Minimum = 20;
            this.zoomTrackBar.Name = "zoomTrackBar";
            this.zoomTrackBar.Size = new Size(1000, 30);
            this.zoomTrackBar.TabIndex = 0;
            this.zoomTrackBar.TickFrequency = 10;
            this.zoomTrackBar.Value = 100;
            this.zoomTrackBar.Scroll += this.ZoomTrackBar_Scroll;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.zoomTrackBar);
            this.bottomPanel.Controls.Add(this.percentLabel);
            this.bottomPanel.Dock = DockStyle.Bottom;
            this.bottomPanel.Location = new Point(0, 672);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new Size(1300, 30);
            this.bottomPanel.TabIndex = 1;
            // 
            // ImageForm
            // 
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(1300, 772);
            this.Controls.Add(this.scrollPanel);
            this.Controls.Add(this.bottomPanel);
            this.Name = "ImageForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "JFIF Banner Resizer";
            this.Load += this.MainForm_Load;
            ((ISupportInitialize)this.pictureBox).EndInit();
            this.scrollPanel.ResumeLayout(false);
            ((ISupportInitialize)this.zoomTrackBar).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
        }
        #endregion
    }
}
