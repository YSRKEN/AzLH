using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzLH.Models {
	static class Utility {
		// 時刻を短い表示形式("12:34:56")で返す
		public static string GetTimeStrShort() {
			var dateTime = DateTime.Now;
			return dateTime.ToString("HH:mm:ss");
		}
		// 時刻を長い表示形式("2017-01-23 12-34-56-789")で返す
		public static string GetTimeStrLong() {
			var dateTime = DateTime.Now;
			return dateTime.ToString("yyyy-MM-dd HH-mm-ss-fff");
		}
		// Rectangleを文字列()で返す
		public static string GetRectStr(Rectangle rect) {
			return $"({rect.X},{rect.Y}) {rect.Width}x{rect.Height}";
		}
		// BitmapをImageSourceに変換する
		// 参考→http://www.nuits.jp/entry/2016/10/17/181232

		public static ImageSource ToImageSource(this Bitmap bmp) {
			var handle = bmp.GetHbitmap();
			try {
				return Imaging.CreateBitmapSourceFromHBitmap(
					handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			finally {
				NativeMethods.DeleteObject(handle);
			}
		}
	}
	internal static class NativeMethods {
		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);
	}
}
