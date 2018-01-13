using AzLH.Models;
using System.ComponentModel;
using System.Windows;

namespace AzLH.Views
{
	/// <summary>
	/// TimerView.xaml の相互作用ロジック
	/// </summary>
	public partial class TimerView : Window
	{
		public TimerView() {
			MouseLeftButtonDown += (o, e) => DragMove();
			InitializeComponent();
		}
		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			SettingsStore.ShowTimerWindowFlg = false;
		}
	}
}
