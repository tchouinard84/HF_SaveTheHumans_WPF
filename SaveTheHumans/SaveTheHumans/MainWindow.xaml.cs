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
        private const int SENSITIVITY = 3;
        private const double ADD_ENEMY_INTERVAL = 2;
        private const double PROGRESS_BAR_INTERVAL = .1;

        private readonly Random _random = new Random();
        private readonly DispatcherTimer _enemyTimer = new DispatcherTimer();
        private readonly DispatcherTimer _targetTimer = new DispatcherTimer();
        private bool _humanCaptured = false;
        private int _humansSaved;

        public MainWindow()
        {
            InitializeComponent();

            _enemyTimer.Tick += enemyTimer_Tick;
            _enemyTimer.Interval = TimeSpan.FromSeconds(ADD_ENEMY_INTERVAL);

            _targetTimer.Tick += targetTimer_Tick;
            _targetTimer.Interval = TimeSpan.FromSeconds(PROGRESS_BAR_INTERVAL);
        }

        private void targetTimer_Tick(object sender, EventArgs e)
        {
            progressBar.Value += 1;
            if (progressBar.Value < progressBar.Maximum) { return; }
            if (playArea.Children.Contains(gameOverText)) { return; }
            EndTheGame();
        }

        private void EndTheGame()
        {
            _enemyTimer.Stop();
            _targetTimer.Stop();
            _humanCaptured = false;
            startButton.Visibility = Visibility.Visible;
            humanCountMsg.Visibility = Visibility.Hidden;
            gameEndStatusText.Text = "You saved " + _humansSaved + " humans!";
            gameEnd.Visibility = Visibility.Visible;
            playArea.Children.Add(gameEnd);
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
            _humansSaved = 0;
            human.IsHitTestVisible = true;
            _humanCaptured = false;
            progressBar.Value = 0;
            startButton.Visibility = Visibility.Collapsed;
            currentLevel.Text = _humansSaved.ToString();
            humanCountMsg.Visibility = Visibility.Visible;
            playArea.Children.Clear();
            playArea.Children.Add(target);
            playArea.Children.Add(human);
            _enemyTimer.Start();
            _targetTimer.Start();
        }

        private void AddEnemy()
        {
            var enemy = new ContentControl { Template = Resources["EnemyTemplate"] as ControlTemplate };
            AnimateEnemy(enemy, 0, playArea.ActualWidth - 100, "(Canvas.Left)");
            AnimateEnemy(enemy, _random.Next((int) playArea.ActualHeight - 100),
                _random.Next((int) playArea.ActualHeight - 100), "(Canvas.Top)");
            playArea.Children.Add(enemy);

            enemy.MouseEnter += enemy_MouseEnter;
        }

        private void enemy_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_humanCaptured)
                EndTheGame();
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, string propertyToAnimate)
        {
            var storyBoard = new Storyboard() { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            var animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(_random.Next(4, 6)))
            };
            Storyboard.SetTarget(animation, enemy);
            Storyboard.SetTargetProperty(animation, new PropertyPath(propertyToAnimate));
            storyBoard.Children.Add(animation);
            storyBoard.Begin();
        }

        private void human_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_enemyTimer.IsEnabled) { return; }
            _humanCaptured = true;
            human.IsHitTestVisible = false;
        }

        private void target_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_targetTimer.IsEnabled) { return; }
            if (!_humanCaptured) { return; }
            NextHuman();
        }

        private void NextHuman()
        {
            progressBar.Value = 0;
            Canvas.SetLeft(target, _random.Next(100, (int) playArea.ActualWidth - 100));
            Canvas.SetTop(target, _random.Next(100, (int) playArea.ActualHeight - 100));
            Canvas.SetLeft(human, _random.Next(100, (int) playArea.ActualWidth - 100));
            Canvas.SetTop(human, _random.Next(100, (int) playArea.ActualHeight - 100));
            _humanCaptured = false;
            human.IsHitTestVisible = true;
            _humansSaved++;
            currentLevel.Text = _humansSaved.ToString();
        }

        private void playArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_humanCaptured) { return; }
            var pointerPosition = e.GetPosition(null);
            var relativePosition = grid.TransformToVisual(playArea).Transform(pointerPosition);
            if ((Math.Abs(relativePosition.X - Canvas.GetLeft(human)) > human.ActualWidth * SENSITIVITY)
                || (Math.Abs(relativePosition.Y - Canvas.GetTop(human)) > human.ActualHeight * SENSITIVITY))
            {
                _humanCaptured = false;
                human.IsHitTestVisible = true;
            }
            else
            {
                Canvas.SetLeft(human, relativePosition.X - human.ActualWidth / 2);
                Canvas.SetTop(human, relativePosition.Y - human.ActualHeight / 2);

            }
        }

        private void playArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_humanCaptured)
                EndTheGame();
        }
    }
}
