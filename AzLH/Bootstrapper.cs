using Microsoft.Practices.Unity;
using Prism.Unity;
using System.Windows;
using AzLH.Views;

namespace AzLH {
	internal class Bootstrapper : UnityBootstrapper {
		protected override DependencyObject CreateShell() {
			return Container.Resolve<MainView>();
		}

		protected override void InitializeShell() {
			Application.Current.MainWindow.Show();
		}
	}
}
