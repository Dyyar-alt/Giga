namespace Giga.Services
{
    public interface IMediaScannerService
    {
        /// <summary>
        /// Обновляет медиа-галерею, чтобы новый файл стал виден пользователю.
        /// На Android сканирует файл через MediaScanner.
        /// На других платформах ничего не делает.
        /// </summary>
        void ScanFile(string filePath);
    }
}