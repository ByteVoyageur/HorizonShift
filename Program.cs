using System;
using System.Net.Http;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class WallpaperChanger
{
    // Windows API 设置壁纸
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDWININICHANGE = 0x02;

    private readonly string accessKey = "k0Xkt9QDewZkXlEFTc-TbiOYw1U3s9-Dwhr1K51Eg8k";

    static async Task Main(string[] args)
    {
        WallpaperChanger changer = new WallpaperChanger();
        await changer.FetchAndSetWallpaperAsync();
    }

    public async Task FetchAndSetWallpaperAsync()
    {
        using (var client = new HttpClient())
        {
            // 获取图片 URL
            var imageUrl = await GetImageUrlAsync(client);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                // 下载图片
                var imagePath = await DownloadImageAsync(client, imageUrl);
                if (File.Exists(imagePath))
                {
                    // 设置壁纸
                    SetWallpaper(imagePath);
                }
            }
        }
    }

    private async Task<string> GetImageUrlAsync(HttpClient client)
    {
        client.BaseAddress = new Uri("https://api.unsplash.com/");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", accessKey);

        var response = await client.GetAsync("photos/random");
        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var imageInfo = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonResponse);
            return imageInfo.GetProperty("urls").GetProperty("full").GetString();
        }
        return null;
    }

    private async Task<string> DownloadImageAsync(HttpClient client, string imageUrl)
    {
        var response = await client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
        var imagePath = Path.Combine(Path.GetTempPath(), "wallpaper.jpg");
        using (var fs = new FileStream(imagePath, FileMode.Create))
        {
            await response.Content.CopyToAsync(fs);
        }
        return imagePath;
    }

    private void SetWallpaper(string imagePath)
    {
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
    }
}

