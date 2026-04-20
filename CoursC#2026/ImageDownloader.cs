using System.Text.Json;


namespace CoursC_2026
{
    internal class ImageDownloader
    {
        private readonly string _jsonFile;
        private readonly string _downloadDir;

        public ImageDownloader(string jsonFile, string downloadDir)
        {
            _jsonFile = jsonFile;
            _downloadDir = downloadDir;
            Directory.CreateDirectory(_downloadDir);
        }

        public async Task DownloadAllAsync()
        {
            var urls = JsonSerializer.Deserialize<string[]>(File.ReadAllText(_jsonFile))!;
            using var client = new HttpClient();

            for (int i = 0; i < urls.Length; i++)
            {
                var bytes = await client.GetByteArrayAsync(urls[i]);
                var outFile = Path.Combine(_downloadDir, $"image_{i + 1}.jpg");
                File.WriteAllBytes(outFile, bytes);
            }
        }
    }
}
