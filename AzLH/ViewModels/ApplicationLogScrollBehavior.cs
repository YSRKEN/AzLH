using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace AzLH.ViewModels {
	class ApplicationLogScrollBehavior : Behavior<TextBox> {
		protected override void OnAttached() {
			base.OnAttached();
			this.AssociatedObject.TextChanged += TextBox_Changed;
		}
		protected override void OnDetaching() {
			base.OnDetaching();
			this.AssociatedObject.TextChanged -= TextBox_Changed;
		}
		void TextBox_Changed(object sender, EventArgs e) {
			this.AssociatedObject.ScrollToEnd();
		}
	}
}
