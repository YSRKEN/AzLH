using AzLH.Models;
using System.ComponentModel;
using System.Windows;

namespace AzLH.Views {
	/// <summary>
	/// SupplyView.xaml の相互作用ロジック
	/// </summary>
	public partial class SupplyView : Window {
		public SupplyView() {
			InitializeComponent();
		}
		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			var settings = SettingsStore.Instance;
			settings.ShowSupplyWindowFlg = false;
		}
	}
}
