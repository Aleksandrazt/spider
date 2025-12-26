namespace Spider.Models
{
    /// <summary>
    /// Модель для экспорта всех данных приложения
    /// </summary>
    public class ExportData
    {
        /// <summary>
        /// Версия формата экспорта
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Дата экспорта
        /// </summary>
        public DateTime ExportDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Категории с их данными
        /// </summary>
        public List<Category> Categories { get; set; } = new();

        /// <summary>
        /// Команды
        /// </summary>
        public List<Command> Commands { get; set; } = new();

        /// <summary>
        /// Docker проекты с их образами
        /// </summary>
        public List<DockerProject> DockerProjects { get; set; } = new();
    }
}

