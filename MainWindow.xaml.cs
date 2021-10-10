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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimeCalc
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public const string TimeFormat = "h':'mm";
		public MainWindow()
		{
			InitializeComponent();
			Times.Text = Settings.Default.Times;
			Target.Text = Settings.Default.Target.ToString(TimeFormat);
		}

		private void Total_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string[] times = Times.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				TimeSpan sum = TimeSpan.Zero;
				foreach (var range in times)
				{
					string s = range.Trim();
					int p = s.IndexOf(' ');
					TimeSpan from, to;
					if (p != -1 && TimeSpan.TryParse(s.Substring(0, p), out from) && TimeSpan.TryParse(s.Substring(p + 1), out to))
					{
						sum += to - from;
					}
				}
				TotalSoFar.Content = sum.ToString(TimeFormat);
				TimeSpan target;
				if (TimeSpan.TryParse(Target.Text, out target))
				{
					var content = (target - sum).ToString(TimeFormat);
					if (sum > target) content += " over";
					TTG.Content = content;
				}
			}
			catch { }
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
			Save();
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			Save();
		}

		private void Save()
		{
			Settings.Default.Times = Times.Text;
			TimeSpan ts;
			if (TimeSpan.TryParse(Target.Text, out ts))
			{
				Settings.Default.Target = ts;
			}
			Settings.Default.Save();
		}
	}
}
