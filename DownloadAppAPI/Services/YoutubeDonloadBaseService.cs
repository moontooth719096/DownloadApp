using NAudio.Lame;
using NAudio.Wave;

namespace DownloadAppAPI.Services
{
    public class YoutubeDonloadBaseService
    {
        public void FileCheck(string folderPath)
        {
            // 检查文件夹是否存在
            if (!Directory.Exists(folderPath))
                // 如果文件夹不存在，则创建新文件夹
                Directory.CreateDirectory(folderPath);
        }
        public void ConvertToMp3(Stream sourceFile, string outputFilePath)
        {
            // Load the source audio file
            using (var reader = new WaveFileReader(sourceFile))
            {
                // Create the output stream
                using (var writer = new LameMP3FileWriter(outputFilePath, reader.WaveFormat, LAMEPreset.STANDARD))
                {
                    // Copy the audio data from the source file to the MP3 output stream
                    reader.CopyTo(writer);
                }
            }
        }

        public void ConvertToMp3(string inputFilePath, string outputFilePath)
        {
            string folderPath = inputFilePath;
            DirectoryInfo directory = new DirectoryInfo(folderPath);
            FileInfo[] files = directory.GetFiles();

            foreach (FileInfo file in files)
            {
                //Console.WriteLine("檔案名稱: " + file.Name);
                //Console.WriteLine("創建時間: " + file.CreationTime);
                //Console.WriteLine("檔案大小: " + file.Length + " bytes");
                //// 其他操作...
                //using (var inputStream = new FileStream(file.FullName, FileMode.Open,
                //           FileAccess.Write, FileShare.ReadWrite))
                //{
                //    using (var reader = new WaveFileReader(inputStream))
                //    {
                //        using (var writer = new LameMP3FileWriter(inputFilePath, reader.WaveFormat, 128))
                //        {
                //            reader.CopyTo(writer);
                //        }
                //    }
                //}
                using (var reader = new MediaFoundationReader(file.FullName))
                {
                    WaveFileWriter.CreateWaveFile(outputFilePath+ file.Name, reader);
                }
            }

        }
        //public Stream GetAudioStream(AudioOnlyStreamInfo audioStreamInfo)
        //{
        //    using (var webClient = new WebClient())
        //    {
        //        // Download the audio stream as a byte array
        //        byte[] audioBytes = webClient.DownloadData(audioStreamInfo.Url);

        //        // Create a MemoryStream from the byte array
        //        Stream audioStream = new MemoryStream(audioBytes);

        //        return audioStream;
        //    }
        //}
       
    }
}
