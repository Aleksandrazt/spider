using System.Windows;

namespace Spider.Services
{
    /// <summary>
    /// Заглушка для сервиса записи видео (функция в разработке)
    /// </summary>
    public class VideoRecorderService : IDisposable
    {
        private bool _isRecording = false;
        private bool _isPaused = false;

        public bool IsRecording => _isRecording;
        public bool IsPaused => _isPaused;

        /// <summary>
        /// Начинает запись видео (заглушка)
        /// </summary>
        public void StartRecording(Rect captureArea, string outputPath, int frameRate = 30, int quality = 90)
        {
            if (_isRecording)
                throw new InvalidOperationException("Recording is already in progress");

            _isRecording = true;
            _isPaused = false;
            
            // Заглушка - просто устанавливаем флаг записи
            System.Diagnostics.Debug.WriteLine($"Video recording started (placeholder): {outputPath}");
        }

        /// <summary>
        /// Приостанавливает запись (заглушка)
        /// </summary>
        public void PauseRecording()
        {
            if (!_isRecording)
                throw new InvalidOperationException("Recording is not in progress");

            _isPaused = true;
            System.Diagnostics.Debug.WriteLine("Video recording paused (placeholder)");
        }

        /// <summary>
        /// Возобновляет запись (заглушка)
        /// </summary>
        public void ResumeRecording()
        {
            if (!_isRecording)
                throw new InvalidOperationException("Recording is not in progress");

            _isPaused = false;
            System.Diagnostics.Debug.WriteLine("Video recording resumed (placeholder)");
        }

        /// <summary>
        /// Останавливает запись (заглушка)
        /// </summary>
        public void StopRecording()
        {
            if (!_isRecording)
                return;

            _isRecording = false;
            _isPaused = false;
            
            System.Diagnostics.Debug.WriteLine("Video recording stopped (placeholder)");
        }

        public void Dispose()
        {
            if (_isRecording)
            {
                StopRecording();
            }
            
            GC.SuppressFinalize(this);
        }
    }
}