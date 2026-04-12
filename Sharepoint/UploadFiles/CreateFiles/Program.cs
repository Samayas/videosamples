namespace UploadFiles.CreateFiles
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Create Temp Files");

            CreateFileWithSize(AppContext.BaseDirectory + "1MB.file", 1);
            CreateFileWithSize(AppContext.BaseDirectory + "2MB.file", 2);
            CreateFileWithSize(AppContext.BaseDirectory + "4MB.file", 4);
            CreateFileWithSize(AppContext.BaseDirectory + "8MB.file", 8);
            CreateFileWithSize(AppContext.BaseDirectory + "12MB.file", 12);

            Console.WriteLine("Files Created");
        }

        private static void CreateFileWithSize(string filePath, int sizeInMegabytes)
        {
            long sizeInBytes = (long)sizeInMegabytes * 1024 * 1024;

            byte[] buffer = new byte[81920];
            Random random = new Random();
            long bytesWritten = 0;

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                while (bytesWritten < sizeInBytes)
                {
                    long remaining = sizeInBytes - bytesWritten;
                    int chunkSize = (int)Math.Min(buffer.Length, remaining);

                    random.NextBytes(buffer);
                    fileStream.Write(buffer, 0, chunkSize);
                    bytesWritten += chunkSize;
                }
            }
        }
    }
}
