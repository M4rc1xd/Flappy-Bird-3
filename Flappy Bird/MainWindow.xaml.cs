using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FlappyBirdWPF
{
	public partial class MainWindow : Window
	{
		DispatcherTimer gameTimer = new DispatcherTimer();
		DispatcherTimer weatherTimer = new DispatcherTimer();
		DispatcherTimer weatherEndTimer = new DispatcherTimer();

		double birdY = 200;
		double birdVelocity = 0;

		const double gravity = 0.6;
		const double rainGravity = 0.85;

		const double normalJump = -10;
		const double rainJump = -7.5;

		bool isGameOver = false;
		bool isRaining = false;
		bool isFoggy = false;

		ImageBrush pipeBrush;

		class PipePair
		{
			public Rectangle Top;
			public Rectangle Bottom;
			public bool Scored = false;
		}

		List<PipePair> pipes = new List<PipePair>();
		Random rnd = new Random();
		int pipeCounter = 0;
		int score = 0;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Focus();

			Canvas.SetLeft(Bird, 100);
			Canvas.SetTop(Bird, birdY);

			pipeBrush = new ImageBrush
			{
				ImageSource = new BitmapImage(new Uri("pack://application:,,,/pipe.png")),
				Stretch = Stretch.Fill
			};

			gameTimer.Interval = TimeSpan.FromMilliseconds(20);
			gameTimer.Tick += GameLoop;

			weatherTimer.Interval = TimeSpan.FromSeconds(20);
			weatherTimer.Tick += WeatherTick;

			weatherEndTimer.Interval = TimeSpan.FromSeconds(10);
			weatherEndTimer.Tick += EndWeather;
		}

		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			StartMenu.Visibility = Visibility.Hidden;
			StartGame();
		}

		void StartGame()
		{
			gameTimer.Stop();
			weatherTimer.Stop();
			weatherEndTimer.Stop();

			isGameOver = false;
			isRaining = false;
			isFoggy = false;

			Fog.Visibility = Visibility.Hidden;
			Fog.Opacity = 0.35;

			score = 0;
			ScoreText.Text = "0";

			birdY = 200;
			birdVelocity = 0;
			Canvas.SetTop(Bird, birdY);

			foreach (var p in pipes)
			{
				GameCanvas.Children.Remove(p.Top);
				GameCanvas.Children.Remove(p.Bottom);
			}
			pipes.Clear();

			pipeCounter = 0;

			GameOverMenu.Visibility = Visibility.Hidden;

			gameTimer.Start();
			weatherTimer.Start();
		}

		private void GameLoop(object sender, EventArgs e)
		{
			if (isGameOver) return;

			double currentGravity = isRaining ? rainGravity : gravity;
			birdVelocity += currentGravity;
			birdVelocity = Math.Min(birdVelocity, 12);

			birdY += birdVelocity;
			Canvas.SetTop(Bird, birdY);

			pipeCounter++;
			if (pipeCounter > 100)
			{
				SpawnPipe();
				pipeCounter = 0;
			}

			MovePipes();
			CheckCollision();
			CheckScore();
			CheckBoundaries();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space && !isGameOver)
				birdVelocity = isRaining ? rainJump : normalJump;
		}

		void SpawnPipe()
		{
			int gap = isRaining ? 130 : 150;
			int topHeight = rnd.Next(50, 200);
			double spawnX = 800;

			Rectangle top = new Rectangle
			{
				Width = 60,
				Height = topHeight,
				Fill = pipeBrush,
				RenderTransform = new ScaleTransform(1, -1),
				RenderTransformOrigin = new Point(0.5, 0.5)
			};

			Rectangle bottom = new Rectangle
			{
				Width = 60,
				Height = 450 - topHeight - gap,
				Fill = pipeBrush
			};

			Canvas.SetLeft(top, spawnX);
			Canvas.SetTop(top, 0);

			Canvas.SetLeft(bottom, spawnX);
			Canvas.SetTop(bottom, topHeight + gap);

			GameCanvas.Children.Add(top);
			GameCanvas.Children.Add(bottom);

			pipes.Add(new PipePair { Top = top, Bottom = bottom });
		}

		void MovePipes()
		{
			foreach (var pipe in pipes)
			{
				Canvas.SetLeft(pipe.Top, Canvas.GetLeft(pipe.Top) - 4);
				Canvas.SetLeft(pipe.Bottom, Canvas.GetLeft(pipe.Bottom) - 4);
			}
		}

		Rect GetRect(FrameworkElement e)
		{
			return new Rect(Canvas.GetLeft(e), Canvas.GetTop(e), e.Width, e.Height);
		}

		void CheckCollision()
		{
			Rect birdRect = GetRect(Bird);

			foreach (var pipe in pipes)
			{
				if (birdRect.IntersectsWith(GetRect(pipe.Top)) ||
					birdRect.IntersectsWith(GetRect(pipe.Bottom)))
				{
					GameOver();
				}
			}
		}

		void CheckBoundaries()
		{
			if (birdY < 0 || birdY > GameCanvas.ActualHeight - Bird.Height)
				GameOver();
		}

		void CheckScore()
		{
			foreach (var pipe in pipes)
			{
				if (!pipe.Scored && Canvas.GetLeft(pipe.Top) + pipe.Top.Width < 100)
				{
					score++;
					pipe.Scored = true;
					ScoreText.Text = score.ToString();
				}
			}
		}

		private void WeatherTick(object sender, EventArgs e)
		{
			if (rnd.NextDouble() > 0.5) return;

			if (rnd.Next(2) == 0)
				StartRain();
			else
				StartFog();
		}

		void StartRain()
		{
			isRaining = true;
			isFoggy = false;
			Fog.Visibility = Visibility.Visible;
			Fog.Opacity = 0.55;

			weatherEndTimer.Stop();
			weatherEndTimer.Start();
		}

		void StartFog()
		{
			isFoggy = true;
			isRaining = false;
			Fog.Visibility = Visibility.Visible;
			Fog.Opacity = 0.35;

			weatherEndTimer.Stop();
			weatherEndTimer.Start();
		}

		void EndWeather(object sender, EventArgs e)
		{
			isFoggy = false;
			isRaining = false;
			Fog.Visibility = Visibility.Hidden;
			weatherEndTimer.Stop();
		}

		void GameOver()
		{
			if (isGameOver) return;

			isGameOver = true;

			gameTimer.Stop();
			weatherTimer.Stop();
			weatherEndTimer.Stop();

			GameOverMenu.Visibility = Visibility.Visible;
		}

		private void RestartButton_Click(object sender, RoutedEventArgs e)
		{
			StartGame();
		}
	}
}
