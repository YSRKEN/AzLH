using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace AzLH.ViewModels {
	class ApplicationLogScrollBehavior : Behavior<TextBox> {
		protected override void OnAttached() {
			base.OnAttached();
			AssociatedObject.TextChanged += TextBox_Changed;
		}
		protected override void OnDetaching() {
			base.OnDetaching();
			AssociatedObject.TextChanged -= TextBox_Changed;
		}
		// テキストボックスの内容が変更された際、最後までスクロール操作を行う
		void TextBox_Changed(object sender, EventArgs e) {
			AssociatedObject.ScrollToEnd();
		}
	}
}
