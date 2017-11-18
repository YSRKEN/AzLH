﻿using System;
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
		void TextBox_Changed(object sender, EventArgs e) {
			AssociatedObject.ScrollToEnd();
		}
	}
}
