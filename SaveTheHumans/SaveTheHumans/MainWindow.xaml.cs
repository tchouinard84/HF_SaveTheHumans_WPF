using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SaveTheHumans
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random random = new Random();
        private DispatcherTimer enemyTimer = new DispatcherTimer();
        private DispatcherTimer targetTimer = new DispatcherTimer();
        private bool humanCaptured = false;

        public MainWindow()
        {
            InitializeComponent();

            enemyTimer.Tick += enemyTimer_Tick;
            enemyTimer.Interval = TimeSpan.FromSeconds(2);

            targetTimer.Tick += targetTimer_Tick;
            targetTimer.Interval = TimeSpan.FromSeconds(.1);
        }

        private void targetTimer_Tick(object sender, EventArgs e)
        {
            progressBar.Value += 1;
            MaybeEndTheGame();
        }

        private void MaybeEndTheGame()
        {
            if (progressBar.Value < progressBar.Maximum) { return; }
            if (playArea.Children.Contains(gameOverText)) { return; }
            EndTheGame();
        }

        private void EndTheGame()
        {
            enemyTimer.Stop();
            targetTimer.Stop();
            humanCaptured = false;
            startButton.Visibility = Visibility.Visible;
            playArea.Children.Add(gameOverText);
        }

        private void enemyTimer_Tick(object sender, EventArgs e)
        {
            AddEnemy();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void StartGame()
        {
            human.IsHitTestVisible = true;
            humanCaptured = false;
            progressBar.Value = 0;
            startButton.Visibility = Visibility.Collapsed;
            playArea.Children.Clear();
            playArea.Children.Add(target);
            playArea.Children.Add(human);
            enemyTimer.Start();
            targetTimer.Start();
        }

        private void AddEnemy()
        {
            var enemy = new ContentControl();
            enemy.Template = Resources["EnemyTemplate"] as ControlTemplate;
            AnimateEnemy(enemy, 0, playArea.ActualWidth - 100, "(Canvas.Left)");
            AnimateEnemy(enemy, random.Next((int) playArea.ActualHeight - 100),
                random.Next((int) playArea.ActualHeight - 100), "(Canvas.Top)");
            playArea.Children.Add(enemy);
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, string propertyToAnimate)
        {
            var storyBoard = new Storyboard() { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            var animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(random.Next(4, 6)))
            };
            Storyboard.SetTarget(animation, enemy);
            Storyboard.SetTargetProperty(animation, new PropertyPath(propertyToAnimate));
            storyBoard.Children.Add(animation);
            storyBoard.Begin();
        }

        private void human_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!enemyTimer.IsEnabled) { return; }
            humanCaptured = true;
            human.IsHitTestVisible = false;
        }
    }
}
