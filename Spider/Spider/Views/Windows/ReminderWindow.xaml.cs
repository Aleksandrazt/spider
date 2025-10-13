using System.Windows;

namespace Spider.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ReminderWindow.xaml
    /// </summary>
    public partial class ReminderWindow : Window
    {
        /// <summary>
        /// Указывает, была ли нажата кнопка "Напомнить позже"
        /// </summary>
        public bool Snoozed { get; private set; }

        public ReminderWindow(string title, string message)
        {
            InitializeComponent();
            
            TitleTextBlock.Text = title;
            MessageTextBlock.Text = message;
            
            Snoozed = false;

            // Воспроизвести системный звук
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Snoozed = false;
            Close();
        }

        private void SnoozeButton_Click(object sender, RoutedEventArgs e)
        {
            Snoozed = true;
            Close();
        }
    }
}