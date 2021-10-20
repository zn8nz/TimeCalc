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
		private static readonly Regex RangeRx = new Regex(@"^(?<start>\d\d?:\d\d)[- /]+(?<end>\d\d?:\d\d\b|now)", RegexOptions.Compiled);
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
				string[] lines = Times.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				var sum = TimeSpan.Zero;
				var sb = new StringBuilder();
				Match match;
				foreach (string line in lines) {
					string range = line;
					if (string.IsNullOrWhiteSpace(range) || range.StartsWith("#")) {
						sb.AppendLine(range);
						continue;
					}
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
							if (end == "now") end = DateTime.Now.ToString("h:mm");
							if (TimeSpan.TryParse(start, out var from) && TimeSpan.TryParse(end, out var to)) {
								if (from > to) to += TimeSpan.FromHours(12);
								TimeSpan diff = to - from;
								sum += diff;
								range1 = p == -1 ? range : range.Substring(0, p);
								range1 = range1.TrimEnd() + $" Δ {diff:c}";
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
					var content = (target - sum).ToString("c");
					if (sum > target) content += " over";
					TTG.Content = content;
				}
				else {
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
