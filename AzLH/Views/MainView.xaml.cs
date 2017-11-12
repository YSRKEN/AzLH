using AzLH.Models;
using Prism.Events;
using System.Windows;

namespace AzLH.Views {
	/// <summary>
	/// MainView.xaml の相互作用ロジック
	/// </summary>
	public partial class MainView : Window {
		public MainView() {
			InitializeComponent();
			Messenger.Instance.GetEvent<PubSubEvent<string>>().Subscribe(
				s => MessageBox.Show(s, Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Information));
		}
	}
}
