using AzLH.Models;
using System.ComponentModel;
using System.Windows;

namespace AzLH.Views {
	/// <summary>
	/// SupplyView.xaml の相互作用ロジック
	/// </summary>
	public partial class SupplyView : Window {
		public SupplyView() {
			MouseLeftButtonDown += (o, e) => DragMove();
			InitializeComponent();
		}
		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			SettingsStore.ShowSupplyWindowFlg = false;
		}
	}
}
