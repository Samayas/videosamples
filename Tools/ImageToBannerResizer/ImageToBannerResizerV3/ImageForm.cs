using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageToBannerResizerV3
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
                this.InitializeSelection();
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

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            // Validate Left Button and Image
            if (e.Button != MouseButtons.Left || this.originalBitmap == null)
            {
                return;
            }

            // Get Rectangle
            Rectangle currentDisplayRectangle = this.GetDisplaySelectionRectangle();
            // If inside Rectangle
            if (currentDisplayRectangle.Contains(e.Location))
            {
                // Start Drag
                this.isDragging = true;
                this.dragOffset = new Point(e.X - currentDisplayRectangle.X, e.Y - currentDisplayRectangle.Y);
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            // Cancel Drag
            this.isDragging = false;
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            // Validate Drag
            if (!this.isDragging)
            {
                Rectangle displayRectangle = this.GetDisplaySelectionRectangle();
                // Revert Icon
                this.pictureBox.Cursor = displayRectangle.Contains(e.Location) ? Cursors.SizeAll : Cursors.Default;

                return;
            }

            // Determine space
            Rectangle currentDisplayRectangle = this.GetDisplaySelectionRectangle();

            int newPbX = Math.Max(0, Math.Min(e.X - this.dragOffset.X, this.pictureBox.Width - currentDisplayRectangle.Width));
            int newPbY = Math.Max(0, Math.Min(e.Y - this.dragOffset.Y, this.pictureBox.Height - currentDisplayRectangle.Height));

            // Convert Image Space
            float scaleX = (float)this.originalBitmap.Width / this.pictureBox.Width;
            float scaleY = (float)this.originalBitmap.Height / this.pictureBox.Height;

            // Selected Image
            this.imageSelectionRectangle = new Rectangle((int)(newPbX * scaleX), (int)(newPbY * scaleY), this.imageSelectionRectangle.Width, this.imageSelectionRectangle.Height);

            // Forces redraw
            this.pictureBox.Invalidate();
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            // Validate
            if (this.originalBitmap == null)
            {
                return;
            }

            // Determine space
            Rectangle currentDisplayRectangle = this.GetDisplaySelectionRectangle();
            if (currentDisplayRectangle.IsEmpty)
            {
                return;
            }

            // Four dark strips surrounding the selection
            using (SolidBrush dark = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            {
                e.Graphics.FillRectangle(dark, 0, 0, this.pictureBox.Width, currentDisplayRectangle.Top);
                e.Graphics.FillRectangle(dark, 0, currentDisplayRectangle.Bottom, this.pictureBox.Width, this.pictureBox.Height - currentDisplayRectangle.Bottom);
                e.Graphics.FillRectangle(dark, 0, currentDisplayRectangle.Top, currentDisplayRectangle.Left, currentDisplayRectangle.Height);
                e.Graphics.FillRectangle(dark, currentDisplayRectangle.Right, currentDisplayRectangle.Top, this.pictureBox.Width - currentDisplayRectangle.Right, currentDisplayRectangle.Height);
            }

            // Rule-of-thirds grid
            using (Pen grid = new Pen(Color.FromArgb(90, Color.White), 1))
            {
                grid.DashStyle = DashStyle.Dash;
                int stepX = currentDisplayRectangle.Width / 3;
                int stepY = currentDisplayRectangle.Height / 3;
                for (int i = 1; i <= 2; i++)
                {
                    e.Graphics.DrawLine(grid, currentDisplayRectangle.X + stepX * i, currentDisplayRectangle.Y, currentDisplayRectangle.X + stepX * i, currentDisplayRectangle.Bottom);
                    e.Graphics.DrawLine(grid, currentDisplayRectangle.X, currentDisplayRectangle.Y + stepY * i, currentDisplayRectangle.Right, currentDisplayRectangle.Y + stepY * i);
                }
            }

            // Dashed border
            using (Pen border = new Pen(Color.White, 2) { DashStyle = DashStyle.Dash })
            {
                e.Graphics.DrawRectangle(border, currentDisplayRectangle);
            }

            // Make Corners
            int h = 8;
            using (SolidBrush handle = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(handle, currentDisplayRectangle.Left - h / 2, currentDisplayRectangle.Top - h / 2, h, h);
                e.Graphics.FillRectangle(handle, currentDisplayRectangle.Right - h / 2, currentDisplayRectangle.Top - h / 2, h, h);
                e.Graphics.FillRectangle(handle, currentDisplayRectangle.Left - h / 2, currentDisplayRectangle.Bottom - h / 2, h, h);
                e.Graphics.FillRectangle(handle, currentDisplayRectangle.Right - h / 2, currentDisplayRectangle.Bottom - h / 2, h, h);
            }

            // Add Label
            string label = "Banner 7:1 | Drag to reposition";
            using (Font f = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
            using (SolidBrush white = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(label, f, shadow, currentDisplayRectangle.X + 9, currentDisplayRectangle.Y + 7);
                e.Graphics.DrawString(label, f, white, currentDisplayRectangle.X + 8, currentDisplayRectangle.Y + 6);
            }
        }

        private Rectangle GetDisplaySelectionRectangle()
        {
            // Validate
            if (this.originalBitmap == null || this.pictureBox.Width == 0 || this.pictureBox.Height == 0)
            {
                return Rectangle.Empty;
            }

            // Compute Scale Factor
            float scaleX = (float)this.pictureBox.Width / this.originalBitmap.Width;
            float scaleY = (float)this.pictureBox.Height / this.originalBitmap.Height;

            // Return Rectangle
            return new Rectangle((int)(this.imageSelectionRectangle.X * scaleX), (int)(this.imageSelectionRectangle.Y * scaleY), (int)(this.imageSelectionRectangle.Width * scaleX), (int)(this.imageSelectionRectangle.Height * scaleY));
        }

        /// <summary>
        /// Initialize Selection.
        /// </summary>
        private void InitializeSelection()
        {
            // Validate Image
            if (this.originalBitmap == null)
            {
                return;
            }

            // Try Full image width
            int selectedWidth = this.originalBitmap.Width;
            int selectedHeight = (int)(selectedWidth / BannerAspectRatio);

            // Fallback on overflow
            if (selectedHeight > this.originalBitmap.Height)
            {
                selectedHeight = this.originalBitmap.Height;
                selectedWidth = (int)(selectedHeight * BannerAspectRatio);
            }

            this.imageSelectionRectangle = new Rectangle((this.originalBitmap.Width - selectedWidth) / 2, (this.originalBitmap.Height - selectedHeight) / 2, selectedWidth, selectedHeight);

            // Forces redraw
            this.pictureBox.Invalidate();
        }
    }
}

