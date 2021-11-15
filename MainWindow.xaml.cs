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
		private static readonly Regex RangeRx = new Regex(@"^(?<start>\d\d?:\d\d)[- /]+(?<end>\d\d?:\d\d\b|now|!)", RegexOptions.Compiled);
		private static readonly Regex TargetRx = new Regex(@"^(?<hours>\d+)(?::(?<minutes>\d\d))?$", RegexOptions.Compiled);

		public MainWindow()
		{
			InitializeComponent();
			Times.Text = Settings.Default.Times;
			Target.Text = Settings.Default.Target;
		}

		private void Total_Click(object sender, RoutedEventArgs e)
		{
			TimeSpan previous = default;
			TimeSpan pause = default;
			TimeSpan pauseSum = TimeSpan.Zero;
			try {
				string[] lines = Times.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
				var sum = TimeSpan.Zero;
				var sb = new StringBuilder();
				Match match;
				foreach (string line in lines) {
					if (string.IsNullOrWhiteSpace(line) || line.StartsWith("<")) {
						continue;
					}
					if (line.StartsWith("#") && pauseSum != TimeSpan.Zero) {
						sb.AppendLine($"<   💤 total {pauseSum:h'h 'mm}");
						sb.AppendLine();
						sb.AppendLine(line);
						previous = default;
						pauseSum = TimeSpan.Zero;
						continue;
					}
					string range = line;
					int p = range.IndexOf('Δ');
					if (p == -1) {
						p = range.IndexOf('d');
						if (p != -1) range = range.Replace('d', 'Δ');
					}
					if (p == 0) {
						if (TimeSpan.TryParse(range.Substring(1).TrimEnd(), out var delta)) {
							sum += delta;
						}
						sb.AppendLine(range);
					}
					else {
						// line format: hh:mm-hh:mm  or hh:mm-now
						match = RangeRx.Match(range);
						if (match.Success) {
							string range1 = range;
							string start = match.Groups["start"].Value;
							string end = match.Groups["end"].Value;
							
							if (end == "now" || end == "!") end = DateTime.Now.ToString("h:mm");
							if (TimeSpan.TryParse(start, out var from) && TimeSpan.TryParse(end, out var to)) {
								if (from > to) to += TimeSpan.FromHours(12);
								TimeSpan diff = to - from;
								sum += diff;
								range1 = p == -1 ? range : range.Substring(0, p);
								if (previous != default) {
									pause = previous - from;
									pauseSum += pause;
									if (pause != default) sb.AppendLine($"<   {pause:h'h 'mm}");
								}
								previous = to;
								range1 = range1.TrimEnd() + $" Δ {diff:h'h 'mm}";
							}
							sb.AppendLine(range1);
						}
						else {
							if (!range.EndsWith(" <error")) sb.AppendLine(range + " <error");
							else sb.AppendLine(range);
						}
					}
				}
				TotalSoFar.Content = sum.ToString("c");
				match = TargetRx.Match(Target.Text);
				if (match.Success) {
					int hours = int.TryParse(match.Groups["hours"].Value, out int h) ? h : 0;
					int minutes = int.TryParse(match.Groups["minutes"].Value, out int m) ? m : 0;
					var target = new TimeSpan(hours, minutes, 0);
					var ttg = target - sum;
					var ttgContent = ttg.ToString("c");
					if (sum > target) ttgContent += " over";
					TTG.Content = ttgContent;
					var eta = DateTime.Now + ttg;
					if (eta > DateTime.Now.Date.AddDays(1)) {
						ETA.Content = eta.ToString("ddd HH:mm");
					} else {
						ETA.Content = eta.ToString("HH:mm");
					}
				}
				else {
					TTG.Content = "error";
				}
				Times.Text = sb.ToString();
			}
			catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine(ex);
			}
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
