namespace AzLH.Models {
	static class Utility {
		// 時刻を短い表示形式("12:34:56")で返す
		public static string GetTimeStrShort() {
			var dateTime = System.DateTime.Now;
			return dateTime.ToString("HH:mm:ss");
		}
		// 時刻を長い表示形式("2017-01-23 12-34-56-789")で返す
		public static string GetTimeStrLong() {
			var dateTime = System.DateTime.Now;
			return dateTime.ToString("yyyy-MM-dd HH-mm-ss-fff");
		}
		// Rectangleを文字列()で返す
		public static string GetRectStr(System.Drawing.Rectangle rect) {
			return $"({rect.X},{rect.Y}) {rect.Width}x{rect.Height}";
		}
	}
}
