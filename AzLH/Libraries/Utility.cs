using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
		public static string GetTimeStrLong(DateTime dt) {
			return dt.ToString("yyyy-MM-dd HH-mm-ss-fff");
		}
		public static string GetTimeStrLong() {
			return GetTimeStrLong(DateTime.Now);
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
		// ソフトウェアのタイトルを返す
		public static string SoftwareName {
			get {
				var assembly = Assembly.GetExecutingAssembly();
				// AssemblyTitle
				string asmttl = ((AssemblyTitleAttribute)
					Attribute.GetCustomAttribute(assembly,
					typeof(AssemblyTitleAttribute))).Title;
				return asmttl;
			}
		}
		// ソフトウェアのバージョンを返す
		public static string SoftwareVer {
			get {
				var assembly = Assembly.GetExecutingAssembly();
				var asmver = assembly.GetName().Version;
				return $"{asmver}";
			}
		}
		// 最新版のバージョンを返す
		public static async Task<string> NewestSoftwareVerAsync() {
			try {
				var client = new HttpClient();
				string response = await client.GetStringAsync("https://raw.githubusercontent.com/YSRKEN/AzLH/master/version.txt");
				return response;
			}
			catch {
				return "";
			}
		}
		// BitmapについてOCRした結果を表示する
		public static void ShowOcrInfo(Bitmap bitmap) {
			// シーン判定を行う
			string scene = SceneRecognition.JudgeGameScene(bitmap);
			string output = $"シーン判定結果：{scene}\n";
			// 資材関係のシーンだった場合、資材量を読み取って表示内容に追記する
			if (SupplyStore.SupplyListEachScene.ContainsKey(scene)) {
				output += "読み取った資材量：\n";
				foreach (string supplyName in SupplyStore.SupplyListEachScene[scene]) {
					int supplyValue = CharacterRecognition.GetValueOCR(bitmap, supplyName, SettingsStore.PutCharacterRecognitionFlg);
					output += $"　{supplyName}→{supplyValue}\n";
				}
			}
			// その他のシーンの場合の処理
			switch (scene) {
			case "戦闘中": {
					var gauge = SceneRecognition.GetBattleBombGauge(bitmap);
					output += "読み取ったゲージ量：\n";
					output += $"　空撃→{Math.Round(gauge[0] * 100.0, 1)}％\n";
					output += $"　雷撃→{Math.Round(gauge[1] * 100.0, 1)}％\n";
					output += $"　砲撃→{Math.Round(gauge[2] * 100.0, 1)}％\n";
				}
				break;
			case "委託": {
					output += "読み取った秒数：\n";
					for(int i = 1; i <= 4; ++i) {
						long time = CharacterRecognition.GetTimeOCR(bitmap, $"委託{i}", SettingsStore.PutCharacterRecognitionFlg);
						output += $"　{i}つ目→{time}\n";
					}
				}
				break;
			case "戦術教室": {
					output += "読み取った秒数：\n";
					for (int i = 1; i <= 2; ++i) {
						long time = CharacterRecognition.GetTimeOCR(bitmap, $"戦術教室{i}", SettingsStore.PutCharacterRecognitionFlg);
						output += $"　{i}つ目→{time}\n";
					}
				}
				break;
			case "寮舎": {
					output += "読み取った秒数：\n";
					long time = CharacterRecognition.GetTimeOCR(bitmap, "食糧", SettingsStore.PutCharacterRecognitionFlg);
					output += $"　食糧→{time}\n";
				}
				break;
			}
			MessageBox.Show(output, Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
	internal static class NativeMethods {
		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);
	}
}
