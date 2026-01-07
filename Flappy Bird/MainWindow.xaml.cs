using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FlappyBirdWPF
{
	public partial class MainWindow : Window
	{
		DispatcherTimer gameTimer = new DispatcherTimer();

		double birdY = 200;
		double birdVelocity = 0;
		const double gravity = 0.6;
		double jumpStrength = -10;

		bool isGameOver = false;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Focus();

			Canvas.SetLeft(Bird, 100);
			Canvas.SetTop(Bird, birdY);

			gameTimer.Interval = TimeSpan.FromMilliseconds(20);
			gameTimer.Tick += GameLoop;
			gameTimer.Start();
		}

		private void GameLoop(object sender, EventArgs e)
		{
			if (isGameOver) return;

			birdVelocity += gravity;
			birdY += birdVelocity;
			Canvas.SetTop(Bird, birdY);

			CheckBoundaries();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space && !isGameOver)
				birdVelocity = jumpStrength;
		}

		void CheckBoundaries()
		{
			if (birdY < 0 || birdY > GameCanvas.ActualHeight - Bird.Height)
				GameOver();
		}


		void GameOver()
		{
			isGameOver = true;
			gameTimer.Stop();

			GameOverText.Visibility = Visibility.Visible;
			Canvas.SetLeft(GameOverText, 250);
			Canvas.SetTop(GameOverText, 150);
		}
	}
}
