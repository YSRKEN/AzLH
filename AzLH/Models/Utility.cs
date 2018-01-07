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
				var response = await client.GetStringAsync("https://raw.githubusercontent.com/YSRKEN/AzLH/master/version.txt");
				return response;
			}
			catch {
				return "";
			}
		}
		// BitmapについてOCRした結果を表示する
		public static void ShowOcrInfo(Bitmap bitmap) {
			string scene = SceneRecognition.JudgeGameScene(bitmap);
			var supplyValueDic = new Dictionary<string, int>();
			string otherMessage = "";
			var settings = SettingsStore.Instance;
			switch (scene) {
			case "母港":
				supplyValueDic["燃料"] = CharacterRecognition.GetValueOCR(bitmap, "燃料", settings.PutCharacterRecognitionFlg);
				supplyValueDic["資金"] = CharacterRecognition.GetValueOCR(bitmap, "資金", settings.PutCharacterRecognitionFlg);
				supplyValueDic["ダイヤ"] = CharacterRecognition.GetValueOCR(bitmap, "ダイヤ", settings.PutCharacterRecognitionFlg);
				break;
			case "建造":
				supplyValueDic["キューブ"] = CharacterRecognition.GetValueOCR(bitmap, "キューブ", settings.PutCharacterRecognitionFlg);
				break;
			case "建造中":
				supplyValueDic["ドリル"] = CharacterRecognition.GetValueOCR(bitmap, "ドリル", settings.PutCharacterRecognitionFlg);
				break;
			case "支援":
				supplyValueDic["勲章"] = CharacterRecognition.GetValueOCR(bitmap, "勲章", settings.PutCharacterRecognitionFlg);
				break;
			case "家具屋":
				supplyValueDic["家具コイン"] = CharacterRecognition.GetValueOCR(bitmap, "家具コイン", settings.PutCharacterRecognitionFlg);
				break;
			case "戦闘中": {
					var gauge = SceneRecognition.GetBattleBombGauge(bitmap);
					otherMessage += "読み取ったゲージ量：\n";
					otherMessage += $"　空撃→{Math.Round(gauge[0] * 100.0, 1)}％\n";
					otherMessage += $"　雷撃→{Math.Round(gauge[1] * 100.0, 1)}％\n";
					otherMessage += $"　砲撃→{Math.Round(gauge[2] * 100.0, 1)}％\n";
				}
				break;
			case "委託": {
					long time1 = CharacterRecognition.GetTimeOCR(bitmap, "委託1", settings.PutCharacterRecognitionFlg);
					long time2 = CharacterRecognition.GetTimeOCR(bitmap, "委託2", settings.PutCharacterRecognitionFlg);
					long time3 = CharacterRecognition.GetTimeOCR(bitmap, "委託3", settings.PutCharacterRecognitionFlg);
					long time4 = CharacterRecognition.GetTimeOCR(bitmap, "委託4", settings.PutCharacterRecognitionFlg);
					otherMessage += "読み取った秒数：\n";
					otherMessage += $"　1つ目→{time1}\n";
					otherMessage += $"　2つ目→{time2}\n";
					otherMessage += $"　3つ目→{time3}\n";
					otherMessage += $"　4つ目→{time4}\n";
				}
				break;
			case "戦術教室": {
					long time1 = CharacterRecognition.GetTimeOCR(bitmap, "戦術教室1", settings.PutCharacterRecognitionFlg);
					long time2 = CharacterRecognition.GetTimeOCR(bitmap, "戦術教室2", settings.PutCharacterRecognitionFlg);
					otherMessage += "読み取った秒数：\n";
					otherMessage += $"　1つ目→{time1}\n";
					otherMessage += $"　2つ目→{time2}\n";
				}
				break;
			}
			string output = $"シーン判定結果：{scene}\n";
			if (supplyValueDic.Count > 0) {
				output += "読み取った資材量：\n";
				foreach (var pair in supplyValueDic) {
					output += $"　{pair.Key}→{pair.Value}\n";
				}
			}
			output += otherMessage;
			MessageBox.Show(output, Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
	internal static class NativeMethods {
		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);
	}
}
