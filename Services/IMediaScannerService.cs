namespace Giga.Services
{
    public interface IMediaScannerService
    {

        // Обновляет медиа-галерею, чтобы новый файл стал виден пользователю.
        // На Android сканирует файл через MediaScanner.
        // На других платформах ничего не делает.
        
        void ScanFile(string filePath);
    }
}
