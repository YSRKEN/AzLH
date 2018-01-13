using AzLH.Models;
using System.Drawing;
using System.Windows;
using System.Windows.Interactivity;

namespace AzLH.ViewModels
{
	class FileDragAndDropToWindowBehavior : Behavior<Window>
	{
		protected override void OnAttached() {
			base.OnAttached();
			AssociatedObject.DragOver += DragOver;
			AssociatedObject.Drop += Drop;
		}
		protected override void OnDetaching() {
			base.OnDetaching();
			AssociatedObject.DragOver -= DragOver;
			AssociatedObject.Drop -= Drop;
		}
		// ファイルがドラッグされてきた際の挙動を記述する
		void DragOver(object sender, DragEventArgs e) {
			e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
				? DragDropEffects.All
				: DragDropEffects.None;
		}
		// ファイルがドロップされると、各ファイルについてOCR処理し、その結果を返す
		void Drop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				string[] fileNameList = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (string fileName in fileNameList) {
					try {Utility.ShowOcrInfo(new Bitmap(fileName));}
					catch {}
				}
			}
		}
	}
}
