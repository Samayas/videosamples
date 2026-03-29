using System.ComponentModel;

namespace ImageToBannerResizerV4
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
        private Button extractButton;
        private Button loadNewImageButton;

        private Rectangle imageSelectionRect;
        private bool isDragging = false;
        private Point dragOffset;

        // Constants
        private const float BannerAspectRatio = 7.0f;
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
            this.extractButton = new Button();
            this.loadNewImageButton = new Button();
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
            this.pictureBox.Paint += this.PictureBox_Paint;
            this.pictureBox.MouseDown += this.PictureBox_MouseDown;
            this.pictureBox.MouseMove += this.PictureBox_MouseMove;
            this.pictureBox.MouseUp += this.PictureBox_MouseUp;
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
            // extractButton
            // 
            this.extractButton.Dock = DockStyle.Left;
            this.extractButton.Location = new Point(130, 0);
            this.extractButton.Name = "extractButton";
            this.extractButton.Size = new Size(120, 30);
            this.extractButton.TabIndex = 2;
            this.extractButton.Text = "Extract Banner";
            this.extractButton.Click += this.ExtractButton_Click;
            // 
            // loadNewImageButton
            // 
            this.loadNewImageButton.Dock = DockStyle.Left;
            this.loadNewImageButton.Location = new Point(0, 0);
            this.loadNewImageButton.Name = "loadNewImageButton";
            this.loadNewImageButton.Size = new Size(130, 30);
            this.loadNewImageButton.TabIndex = 3;
            this.loadNewImageButton.Text = "Load New Image";
            this.loadNewImageButton.Click += this.LoadNewImageButton_Click;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.zoomTrackBar);
            this.bottomPanel.Controls.Add(this.percentLabel);
            this.bottomPanel.Controls.Add(this.extractButton);
            this.bottomPanel.Controls.Add(this.loadNewImageButton);
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
