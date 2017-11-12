using System.Windows;
namespace AzLH.Views {
	/// <summary>
	/// GameScreenSelectView.xaml の相互作用ロジック
	/// </summary>
	public partial class GameScreenSelectView : Window {
		public GameScreenSelectView() {
			InitializeComponent();
			MouseLeftButtonDown += (o, e) => DragMove();
		}
	}
}
