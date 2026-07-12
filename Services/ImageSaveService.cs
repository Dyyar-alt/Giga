#if ANDROID
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using AndroidX.Core.Content;
using AndroidApplication = Android.App.Application;
using AndroidEnvironment = Android.OS.Environment;
using AndroidPath = Android.Graphics.Path;
#endif

namespace Giga.Services
{
    public interface IImageSaveService
    {
        Task<string?> SaveImageAsync(byte[] imageData, string format, string? fileName = null);
        Task<bool> SaveImageWithPickerAsync(byte[] imageData, string format);
        string[] GetSupportedFormats();
    }

    public class ImageSaveService : IImageSaveService
    {
        private readonly IApiKeyService _apiKeyService;
        private const string FolderName = "Gigachat";

        public ImageSaveService(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        public string[] GetSupportedFormats() => new[] { "png", "jpg", "gif" };

        public async Task<string?> SaveImageAsync(byte[] imageData, string format, string? fileName = null)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Нет данных изображения");
                    return null;
                }

                format = format.TrimStart('.').ToLower();
                if (!GetSupportedFormats().Contains(format))
                {
                    format = "png";
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"gigachat_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
                }
                else if (!fileName.Contains('.'))
                {
                    fileName = $"{fileName}.{format}";
                }

#if ANDROID
                return await SaveAndroidAsync(imageData, fileName);
#else
                return await SaveWindowsAsync(imageData, fileName);
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
                return null;
            }
        }

#if WINDOWS || MACCATALYST || IOS
        private async Task<string?> SaveWindowsAsync(byte[] imageData, string fileName)
        {
            try
            {
                // Используем полное имя System.Environment
                string picturesPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
                string gigachatPath = System.IO.Path.Combine(picturesPath, FolderName);

                if (!Directory.Exists(gigachatPath))
                {
                    Directory.CreateDirectory(gigachatPath);
                    System.Diagnostics.Debug.WriteLine($"Создана папка: {gigachatPath}");
                }

                string filePath = System.IO.Path.Combine(gigachatPath, fileName);
                await File.WriteAllBytesAsync(filePath, imageData);

                System.Diagnostics.Debug.WriteLine($"Изображение сохранено: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения на Windows: {ex.Message}");
                return null;
            }
        }
#endif

#if ANDROID
        private async Task<string?> SaveAndroidAsync(byte[] imageData, string fileName)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    return await SaveAndroidQPlusAsync(imageData, fileName);
                }
                else
                {
                    return await SaveAndroidLegacyAsync(imageData, fileName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения на Android: {ex.Message}");
                return null;
            }
        }

        private async Task<string?> SaveAndroidQPlusAsync(byte[] imageData, string fileName)
        {
            try
            {
                // Используем алиас AndroidApplication
                var context = AndroidApplication.Context;
                var resolver = context.ContentResolver;

                string mimeType = GetMimeType(fileName);

                var contentValues = new ContentValues();
                contentValues.Put(MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(MediaStore.IMediaColumns.MimeType, mimeType);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    // Используем алиас AndroidEnvironment
                    contentValues.Put(MediaStore.IMediaColumns.RelativePath, $"{AndroidEnvironment.DirectoryPictures}/{FolderName}");
                }

                var uri = resolver.Insert(MediaStore.Images.Media.ExternalContentUri, contentValues);

                if (uri == null)
                {
                    System.Diagnostics.Debug.WriteLine("Не удалось создать URI для сохранения");
                    return null;
                }

                using (var outputStream = resolver.OpenOutputStream(uri))
                {
                    if (outputStream != null)
                    {
                        await outputStream.WriteAsync(imageData, 0, imageData.Length);
                        await outputStream.FlushAsync();
                        System.Diagnostics.Debug.WriteLine($"Изображение сохранено через MediaStore: {fileName}");
                        return uri.ToString();
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения на Android Q+: {ex.Message}");
                return null;
            }
        }

        private async Task<string?> SaveAndroidLegacyAsync(byte[] imageData, string fileName)
        {
            try
            {
                // Используем алиас AndroidEnvironment
                var picturesPath = AndroidEnvironment.GetExternalStoragePublicDirectory(
                    AndroidEnvironment.DirectoryPictures)?.AbsolutePath;

                if (string.IsNullOrEmpty(picturesPath))
                {
                    System.Diagnostics.Debug.WriteLine("Не удалось получить путь к Pictures");
                    return null;
                }

                string gigachatPath = System.IO.Path.Combine(picturesPath, FolderName);

                if (!Directory.Exists(gigachatPath))
                {
                    Directory.CreateDirectory(gigachatPath);
                    System.Diagnostics.Debug.WriteLine($"Создана папка: {gigachatPath}");
                }

                string filePath = System.IO.Path.Combine(gigachatPath, fileName);

                await File.WriteAllBytesAsync(filePath, imageData);

                ScanFile(filePath);

                System.Diagnostics.Debug.WriteLine($"Изображение сохранено: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения на Android (legacy): {ex.Message}");
                return null;
            }
        }

        private void ScanFile(string filePath)
        {
            try
            {
                // Используем алиас AndroidApplication
                var context = AndroidApplication.Context;
                var intent = new Intent(Intent.ActionMediaScannerScanFile);
                var uri = Android.Net.Uri.FromFile(new Java.IO.File(filePath));
                intent.SetData(uri);
                context.SendBroadcast(intent);
                System.Diagnostics.Debug.WriteLine($"Файл просканирован для галереи");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сканирования: {ex.Message}");
            }
        }
#endif

        private string GetMimeType(string fileName)
        {
            var ext = System.IO.Path.GetExtension(fileName)?.ToLower();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "image/png"
            };
        }

        public async Task<bool> SaveImageWithPickerAsync(byte[] imageData, string format)
        {
            try
            {
                var formats = GetSupportedFormats();
                var result = await Microsoft.Maui.Controls.Application.Current!.MainPage!.DisplayActionSheet(
                    "Выберите формат",
                    "Отмена",
                    null,
                    formats);

                if (string.IsNullOrEmpty(result) || result == "Отмена")
                    return false;

                var savedPath = await SaveImageAsync(imageData, result);
                if (!string.IsNullOrEmpty(savedPath))
                {
                    await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                        "Успешно!",
                        $"Изображение сохранено в формате {result.ToUpper()}",
                        "ОК");
                    return true;
                }
                else
                {
                    await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        "Не удалось сохранить изображение",
                        "ОК");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
        }
    }
}