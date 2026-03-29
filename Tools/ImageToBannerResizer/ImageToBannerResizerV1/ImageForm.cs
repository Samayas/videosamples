using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageToBannerResizerV1
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
                Bitmap loadedBitmap = this.LoadJfifAsBitmap(dialog.FileName);

                // Assign to Picture and show
                this.pictureBox.Image = loadedBitmap;
                this.pictureBox.Size = new Size(loadedBitmap.Width, loadedBitmap.Height);

                // Forces redraw
                this.pictureBox.Invalidate();
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
    }
}

