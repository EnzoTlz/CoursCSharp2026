using CoursC_2026;


var downloader = new ImageDownloader("C:/Users/Enzo/source/repos/CoursCSharp2026/CoursC#2026/images.json", "C:/Users/Enzo/source/repos/CoursCSharp2026/CoursC#2026/Images");
await downloader.DownloadAllAsync();

var resizer = new ImageResizer("C:/Users/Enzo/source/repos/CoursCSharp2026/CoursC#2026/Images");
resizer.ResizeSequential();
resizer.ResizeParallel();