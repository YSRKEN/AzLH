using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AzLH.Models {
	using dSize = System.Drawing.Size;
	static class CharacterRecognition {
		// 燃料：～10000　通常資材左
		// 資金：～50000　通常資材右
		// キューブ：～100　特殊資材左
		// ドリル：～100　特殊資材左
		// 勲章：～100　特殊資材左
		// ダイヤ：～1000　特殊資材右
		// 家具コイン：～1000　特殊資材右

		// OCRする際にリサイズするサイズ
		private static readonly dSize ocrTemplateSize1 = new dSize(32, 32);
		// OCRする際にマッチングさせる元のサイズ
		private static readonly dSize ocrTemplateSize2 = new dSize(ocrTemplateSize1.Width + 2, ocrTemplateSize1.Height + 2);
		// OCRする際に引き伸ばす際の短辺長
		private static readonly int ocrStretchSize = 64;
		// 各資材における認識パラメーター
		private static Dictionary<string, SupplyParameter> supplyParameters = LoadSupplyParameters();

		// 認識パラメーターを読み込む
		private static Dictionary<string, SupplyParameter> LoadSupplyParameters() {
			// Dictionaryを準備
			var output = new Dictionary<string, SupplyParameter>();
			// ファイルを読み込む
			MemoryStream ms = null;
			try {
				ms = new MemoryStream(Properties.Resources.supply_parameter, false);
				using (var sr = new StreamReader(ms, Encoding.UTF8)) {
					ms = null;
					// 全体をstringに読み込む
					string json = sr.ReadToEnd();
					// Json.NETでパース
					var model = JsonConvert.DeserializeObject<Dictionary<string, SceneParameterJson>>(json);
					// パース結果をさらに変換
					foreach (var pair in model) {
						output[pair.Key] = new SupplyParameter(
								pair.Value.Color[0],
								pair.Value.Color[1],
								pair.Value.Color[2],
								pair.Value.RectFloat[0],
								pair.Value.RectFloat[1],
								pair.Value.RectFloat[2],
								pair.Value.RectFloat[3],
								pair.Value.MainSupplyFlg,
								pair.Value.SecondaryAxisFlg,
								pair.Value.InverseFlg,
								pair.Value.Threshold
							);
					}
				}
			}
			finally {
				if (ms != null)
					ms.Dispose();
			}
			return output;
		}
		// 周囲をトリミングする
		static Rectangle GetTrimmingRectangle(Bitmap bitmap) {
			// Rangeを意図的に1スタートにしているのは、
			// FirstOrDefaultメソッドが検索して見つからなかった際、
			// Rangeが参照型ではなく値型なので0が帰ってくるから。
			// つまり、発見できず0が返ってくるのか、
			// 座標0で発見したから0が返ってくるのかが判別できない。
			// なのであえて1スタートにしている
			var rect = new Rectangle(new System.Drawing.Point(0, 0), bitmap.Size);
			var xRange = Enumerable.Range(1, bitmap.Width);
			var yRange = Enumerable.Range(1, bitmap.Height);
			// 上下左右の境界を取得する
			var borderColor = Color.FromArgb(255, 255, 255);
			// 左
			foreach (int x in xRange) {
				// borderColorと等しくない色を発見した場合、pos >= 0になる
				int pos = yRange.FirstOrDefault(y => bitmap.GetPixel(x - 1, y - 1) != borderColor);
				if (pos >= 1) {
					rect.X = x - 1;
					rect.Width -= rect.X;
					break;
				}
			}
			// 上
			foreach (int y in yRange) {
				int pos = xRange.FirstOrDefault(x => bitmap.GetPixel(x - 1, y - 1) != borderColor);
				if (pos >= 1) {
					rect.Y = y - 1;
					rect.Height -= rect.Y;
					break;
				}
			}
			// 右
			foreach (int x in xRange.Reverse()) {
				int pos = yRange.FirstOrDefault(y => bitmap.GetPixel(x - 1, y - 1) != borderColor);
				if (pos >= 1) {
					rect.Width -= bitmap.Width - x;
					break;
				}
			}
			// 下
			foreach (int y in yRange.Reverse()) {
				int pos = xRange.FirstOrDefault(x => bitmap.GetPixel(x - 1, y - 1) != borderColor);
				if (pos >= 1) {
					rect.Height -= bitmap.Height - y;
					break;
				}
			}
			return rect;
		}
		// 資材量を読み取る(-1＝読み取り不可)
		public static int GetValueOCR(Bitmap bitmap, string supplyType, bool debugFlg = false) {
			// 対応している資材名ではない場合は無視する
			if (!supplyParameters.ContainsKey(supplyType))
				return -1;
			// ％指定をピクセル指定に直す
			var cropRect = supplyParameters[supplyType].Rect;
			int px = (int)(bitmap.Width * cropRect.X / 100 + 0.5);
			int py = (int)(bitmap.Height * cropRect.Y / 100 + 0.5);
			int wx = (int)(bitmap.Width * cropRect.Width / 100 + 0.5);
			int wy = (int)(bitmap.Height * cropRect.Height / 100 + 0.5);
			// 画像を必要な部分だけクロップし、同時に認識しやすいサイズまで拡大する
			int cropWx = wx * ocrStretchSize / wy;
			int cropWy = ocrStretchSize;
			var canvas = new Bitmap(cropWx, cropWy);
			using (var g = Graphics.FromImage(canvas)) {
				// 切り取られる位置・大きさ
				var srcRect = new Rectangle(px, py, wx, wy);
				// 貼り付ける位置・大きさ
				var desRect = new Rectangle(0, 0, canvas.Width, canvas.Height);
				g.DrawImage(bitmap, desRect, srcRect, GraphicsUnit.Pixel);
			}
			if (debugFlg) canvas.Save("pic\\digit1.png");
			// ダイヤを勘定する時だけ、黄色部分を黒く塗りつぶす処理を行う
			if (supplyType == "ダイヤ") {
				for (int y = 0; y < canvas.Height; ++y) {
					for (int x = 0; x < canvas.Width; ++x) {
						var color = canvas.GetPixel(x, y);
						if (color.R > 200 && color.G > 200 && color.B < 100)
							canvas.SetPixel(x, y, Color.FromArgb(255, 255, 255));
					}
				}
			}
			// 色の反転処理・二値化処理を行う
			using (var mat1 = BitmapConverter.ToMat(canvas))
			using (var mat2 = new Mat(mat1.Size(), MatType.CV_8U)) {
				Cv2.CvtColor(mat1, mat2, ColorConversionCodes.BGR2GRAY);
				if (supplyParameters[supplyType].InverseFlg)
					Cv2.BitwiseNot(mat2, mat2);
				if (debugFlg) mat2.SaveImage("pic\\digit2.png");
				Cv2.Threshold(mat2, mat2, supplyParameters[supplyType].Threshold, 255, ThresholdTypes.Binary);
				canvas = mat2.ToBitmap();
			}
			if (debugFlg) canvas.Save("pic\\digit3.png");
			// 境界部分を認識させる
			var splitRectList = new List<Rectangle>();
			{
				// x1は文字の左端、x2は文字の右端
				for (int x1 = 0; x1 < canvas.Width; ++x1) {
					// まずは左端を検出する
					bool flg1 = false;
					for (int y = 0; y < canvas.Height; ++y) {
						if (canvas.GetPixel(x1, y).R == 0)
							flg1 = true;
					}
					if (!flg1)
						continue;
					// 次に右端を検出する
					for (int x2 = x1 + 1; x2 <= canvas.Width; ++x2) {
						// x2 == canvas.Widthの時は、探索すらせず右端認定を行う
						if(x2 == canvas.Width) {
							splitRectList.Add(new Rectangle(x1, 0, x2 - x1 + 1, canvas.Height));
							x1 = x2;
							break;
						}
						// 通常の検出を行う
						bool flg2 = true;
						for (int y = 0; y < canvas.Height; ++y) {
							if (canvas.GetPixel(x2, y).R == 0)
								flg2 = false;
						}
						if (!flg2)
							continue;
						splitRectList.Add(new Rectangle(x1, 0, x2 - x1, canvas.Height));
						x1 = x2 - 1;
						break;
					}
				}
			}
			// 各カット毎に数値認識を行う
			var digit = new List<int>();
			for(int k = 0; k < splitRectList.Count; ++k) {
				// 1つの数字分だけ取り出す
				var canvas2 = new Bitmap(splitRectList[k].Width, splitRectList[k].Height);
				using (var g = Graphics.FromImage(canvas2)) {
					// 切り取られる位置・大きさ
					var srcRect = splitRectList[k];
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(0, 0, canvas2.Width, canvas2.Height);
					g.DrawImage(canvas, desRect, srcRect, GraphicsUnit.Pixel);
				}
				if (debugFlg) canvas2.Save($"pic\\digit4-{k + 1}-1.png");
				// 認識用の大きさにリサイズする
				var canvas3 = new Bitmap(ocrTemplateSize2.Width, ocrTemplateSize2.Height);
				using (var g = Graphics.FromImage(canvas3)) {
					// 事前にcanvas3を赤色に塗りつぶす
					g.FillRectangle(Brushes.Red, 0, 0, canvas3.Width, canvas3.Height);
					// 切り取られる位置・大きさ
					var srcRect = GetTrimmingRectangle(canvas2);
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(1, 1, ocrTemplateSize1.Width, ocrTemplateSize1.Height);
					g.DrawImage(canvas2, desRect, srcRect, GraphicsUnit.Pixel);
				}
				if (debugFlg) canvas3.Save($"pic\\digit4-{k + 1}-2.png");

			}
			return 0;
		}

		// 資材の認識パラメーターを表すクラス
		public class SupplyParameter {
			// グラフ化した際の色
			public Color Color { get; }
			// 指定する範囲(％単位)
			public RectangleF Rect { get; }
			// 通常資材か？(falseなら特殊資材)
			public bool MainSupplyFlg { get; }
			// 第二軸に表示するものか？
			public bool SecondaryAxisFlg { get; }
			// 色を反転させるか？
			public bool InverseFlg { get; }
			// 判定に用いるしきい値
			public int Threshold { get; }
			// コンストラクタ
			public SupplyParameter(int r, int g, int b, float x, float y, float width, float height,
				bool mainSupplyFlg, bool secondaryAxisFlg, bool inverseFlg, int threshold) {
				Color = Color.FromArgb(r, g, b);
				Rect = new RectangleF(x, y, width, height);
				MainSupplyFlg = mainSupplyFlg;
				SecondaryAxisFlg = secondaryAxisFlg;
				InverseFlg = inverseFlg;
				Threshold = threshold;
			}
		}
		[JsonObject("param")]
		public class SceneParameterJson {
			public int[] Color { get; set; }
			public float[] RectFloat { get; set; }
			public bool MainSupplyFlg { get; set; }
			public bool SecondaryAxisFlg { get; set; }
			public bool InverseFlg { get; set; }
			public int Threshold { get; set; }
		}
	}
}
