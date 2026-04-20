
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using Image = SixLabors.ImageSharp.Image;


namespace CoursC_2026
{
    class ImageResizer
    {
        private readonly int[] _resolutions = { 1080, 720, 480 };
        private readonly string _inputFolder;

        public ImageResizer(string inputFolder)
        {
            _inputFolder = inputFolder;
        }

        public void ResizeSequential()
        {
            var outputDir = Path.Combine(Environment.CurrentDirectory, "output_images_sequentiel");
            Directory.CreateDirectory(outputDir);

            var files = Directory.GetFiles(_inputFolder);
            Console.WriteLine($"Dossier images : {Path.GetFullPath(_inputFolder)}");
            Console.WriteLine($"Fichiers trouvés : {files.Length}");

            var sw = Stopwatch.StartNew();

            foreach (var file in files)
                foreach (int height in _resolutions)
                    Resize(file, outputDir, height);

            sw.Stop();
            Console.WriteLine($"SEQUENCE : {sw.ElapsedMilliseconds} ms");
        }

        public void ResizeParallel()
        {
            var outputDir = Path.Combine(Environment.CurrentDirectory, "output_images_parrallele");
            Directory.CreateDirectory(outputDir);

            var files = Directory.GetFiles(_inputFolder);
            var sw = Stopwatch.StartNew();

            Parallel.ForEach(files, file =>
            {
                foreach (int height in _resolutions)
                    Resize(file, outputDir, height);
            });

            sw.Stop();
            Console.WriteLine($"PARRALLELE : {sw.ElapsedMilliseconds} ms");
        }

        private void Resize(string file, string outputDir, int height)
        {
            var image = Image.Load(file);
            int width = (int)((double)image.Width / image.Height * height);
            image.Mutate(x => x.Resize(width, height));
            var outFile = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(file)}_{height}p{Path.GetExtension(file)}");
            image.Save(outFile);
        }
    }
}
