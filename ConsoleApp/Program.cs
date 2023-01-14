using Flaminco.ProDownloader.Extensions;
using Flaminco.ProDownloader.Utilities;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new ServiceCollection();

services.AddProDownloader();

ServiceProvider provider = services.BuildServiceProvider();


FileProfileCreator? profileCreator = provider.GetService<FileProfileCreator>();

profileCreator.Initializer("https://raw.githubusercontent.com/AhmedAbuelnour/MBs/master/4MB.txt", @"C:\Users\Ahmed\source\repos\Flaminco\ConsoleApp", "text.txt");


var profiles = profileCreator.CreateSegmentProfilesAsync(8);


List<ServerSegmentProfile> segments = new List<ServerSegmentProfile>();

await foreach (var profile in profiles)
{
    FileDownloader FileDownloader = provider.GetService<FileDownloader>();

    segments.Add(profile);

    await FileDownloader.DownloadAsync(profile, (e) =>
    {
        Console.WriteLine($"Current Percentage: {e.CurrentPercentage}");
        Console.WriteLine($"Downloaded Progress: {e.DownloadedProgress}");
        Console.WriteLine($"DownloadSpeed: {e.DownloadSpeed}");
    });
}

//await profileCreator.ReconstructSegmentProfilesAsync(segments);

//await fileProfileDownloader.DownloadAsync(profile, (e) =>
//    {
//        Console.WriteLine($"Current Percentage: {e.CurrentPercentage}");
//        Console.WriteLine($"Downloaded Progress: {e.DownloadedProgress}");
//        Console.WriteLine($"DownloadSpeed: {e.DownloadSpeed}");
//    });



////FileDownloader fileDownloader = new FileDownloader(profileCreator, "https://raw.githubusercontent.com/AhmedAbuelnour/MBs/master/4MB.txt", "test.tet", @"C:\Users\Ahmed\source\repos\Flaminco\ConsoleApp");



/// <summary>
/// Reconstract the segments from the temp files that got created.
/// </summary>
/// <returns>A complete file in the directory that you specified in the constructor.</returns>

