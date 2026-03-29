using System.ComponentModel;

namespace ImageToBannerResizerV1
{
    partial class ImageForm
    {
        private System.ComponentModel.IContainer components = null;

        // Controls
        private PictureBox pictureBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.pictureBox.Image = null;

                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pictureBox = new PictureBox();
            ((ISupportInitialize)this.pictureBox).BeginInit();
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
            // ImageForm
            // 
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(282, 253);
            this.Name = "ImageForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "JFIF Banner Resizer";
            this.Load += this.MainForm_Load;

            this.Controls.Add(this.pictureBox);

            ((ISupportInitialize)this.pictureBox).EndInit();
            this.ResumeLayout(false);
        }
        #endregion
    }
}
