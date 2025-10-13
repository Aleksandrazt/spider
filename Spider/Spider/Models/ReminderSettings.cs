namespace Spider.Models
{
    /// <summary>
    /// Настройки для модуля напоминаний
    /// </summary>
    public class ReminderSettings
    {
        /// <summary>
        /// Включены ли напоминания
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Интервал напоминаний в минутах
        /// </summary>
        public int IntervalMinutes { get; set; } = 45;

        /// <summary>
        /// Заголовок окна напоминания
        /// </summary>
        public string ReminderTitle { get; set; } = "Время для разминки!";

        /// <summary>
        /// Текст напоминания
        /// </summary>
        public string ReminderMessage { get; set; } = "Пришло время сделать разминку для глаз и тела.\n\nРекомендуемые упражнения:\n• Отведите взгляд от монитора на 20 секунд\n• Посмотрите на удалённые объекты\n• Сделайте упражнения для шеи и плеч\n• Встаньте и немного походите";
    }
}