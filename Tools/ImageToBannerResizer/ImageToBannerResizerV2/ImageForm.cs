using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageToBannerResizerV2
{
    public partial class ImageForm : Form
    {
        public ImageForm()
        {
            this.InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Dialog Box
                this.OpenAndDisplayJfif();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open image:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void OpenAndDisplayJfif()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                // Dialog Configuration
                dialog.Filter = "JFIF Images (*.jfif)|*.jfif|JPEG Images (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files (*.*)|*.*";
                dialog.Title = "Select a JFIF/JPEG Image";

                // Open Dialog
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                // Load Jfif
                Bitmap loaded = this.LoadJfifAsBitmap(dialog.FileName);

                // Assign to Picture and show
                this.originalBitmap?.Dispose();
                this.originalBitmap = loaded;
                // Forces redraw
                this.pictureBox.Image = this.originalBitmap;

                int zoomPercentage = 50;
                this.zoomTrackBar.Value = zoomPercentage;
                this.percentLabel.Text = $"{zoomPercentage}%";
                
                // Zoom
                this.ApplyZoom(zoomPercentage);
            }
        }

        private Bitmap LoadJfifAsBitmap(string filePath)
        {
            // Read File
            byte[] imageBytes = File.ReadAllBytes(filePath);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                try
                {
                    // Create Image
                    using (Image temp = Image.FromStream(ms, false, true))
                    {
                        if (temp.PixelFormat == PixelFormat.Undefined)
                        {
                            throw new ArgumentException("Image format is not supported.");
                        }

                        return new Bitmap(temp);
                    }
                }
                catch (ExternalException ex)
                {
                    throw new InvalidDataException($"Failed to load '{filePath}'. File may be corrupted or unsupported.", ex);
                }
            }
        }

        private void ApplyZoom(int percentage)
        {
            // Validate Image
            if (this.originalBitmap == null)
            {
                return;
            }

            // Computed scaled Size
            int scaledWidth = (int)(this.originalBitmap.Width * (percentage / 100.0));
            int scaledHeight = (int)(this.originalBitmap.Height * (percentage / 100.0));

            // Zoom Picture size
            this.pictureBox.Size = new Size(scaledWidth, scaledHeight);

            // Capping form to screen bound
            Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
            int maxWidth = workingArea.Width;
            int maxHeight = workingArea.Height - BottomPanelHeight;
            this.ClientSize = new Size(Math.Min(scaledWidth, maxWidth), Math.Min(scaledHeight, maxHeight) + BottomPanelHeight);

            // Forces redraw
            this.pictureBox.Invalidate();
        }

        private void ZoomTrackBar_Scroll(object sender, EventArgs e)
        {
            // Adapt Label
            int percentage = this.zoomTrackBar.Value;
            this.percentLabel.Text = $"{percentage}%";
            
            // Zoom in or out
            this.ApplyZoom(percentage);
        }
    }
}

