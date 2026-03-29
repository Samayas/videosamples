namespace ImageToBannerResizerV2
{
    /// <summary>
    /// Program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new ImageForm());
        }
    }
}