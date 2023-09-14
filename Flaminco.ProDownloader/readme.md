## Getting Started 

A Pro downloader which support segmentation, resuming, easy on memory consumtion taking advantage of System.IO.Pipelines, and easy to use ;)

### Features: 

* It Provides High Performance in downloading by using the new API System.IO.Pipelines
* It supports Resuming
* It supports File Segmentation
* Downloading Now Shows: Current %, Current Speed, How Much has been downloaded out of what is left

## Example

To implement this package is now easier than ever.

```
DownloaderClient fileDownloader = new DownloaderClient(httpClientFactory);

await fileDownloader.DownloadAsync(new DownloadOptions
{
    Url = "https://raw.githubusercontent.com/AhmedAbuelnour/MBs/master/4MB.txt",
    DownloadPath = @"D:\Downloads",
    ChunkNumbers = 2,
    SuggestedFileName = "txt4Mb.txt",
    ProgressCallback = (e) =>
    {
        Console.WriteLine($"Download Speed: {e.DownloadSpeed}");
        Console.WriteLine($"Percentage: {e.CurrentPercentage}");
        Console.WriteLine($"Downloaded Progress: {e.DownloadedProgress}");
    }
});


```
