using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
		private static readonly Regex RangeRx = new Regex(@"^(?<start>\d\d?:\d\d)[- /]+(?<end>\d\d?:\d\d\b)", RegexOptions.Compiled);
		private static readonly Regex TargetRx = new Regex(@"^(?<hours>\d+)(?::(?<minutes>\d\d))?$", RegexOptions.Compiled);

		public MainWindow()
		{
			InitializeComponent();
			Times.Text = Settings.Default.Times;
			Target.Text = Settings.Default.Target;
		}

		private void Total_Click(object sender, RoutedEventArgs e)
		{
			try {
				string[] times = Times.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				var sum = TimeSpan.Zero;
				var sb = new StringBuilder();
				Match match;
				foreach (var range in times) {
					match = RangeRx.Match(range);
					if (match.Success) {
						sb.AppendLine(range);
						string start = match.Groups["start"].Value;
						string end = match.Groups["end"].Value;
						if (TimeSpan.TryParse(start, out var from) && TimeSpan.TryParse(end, out var to)) {
							if (from > to) to += TimeSpan.FromHours(12);
							sum += to - from;
						}
					}
					else {
						if (!range.EndsWith(" <error")) sb.AppendLine(range + " <error");
						else sb.AppendLine(range);
					}
				}
				TotalSoFar.Content = sum.ToString("c");
				match = TargetRx.Match(Target.Text);
				if (match.Success) {
					int hours = int.TryParse(match.Groups["hours"].Value, out int h) ? h : 0;
					int minutes = int.TryParse(match.Groups["minutes"].Value, out int m) ? m : 0;
					var target = new TimeSpan(hours, minutes, 0);
					var content = (target - sum).ToString("c");
					if (sum > target) content += " over";
					TTG.Content = content;
				} else {
					TTG.Content = "error";
				}
				Times.Text = sb.ToString();
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
			Settings.Default.Target = Target.Text;
			Settings.Default.Save();
		}
	}
}
