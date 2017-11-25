﻿using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AzLH.Models {
	using dSize = System.Drawing.Size;
	using bitmapArray = List<byte>;
	static class CharacterRecognition {
		// OCRする際にリサイズするサイズ
		private static readonly dSize ocrTemplateSize1 = new dSize(32, 32);
		// OCRする際にマッチングさせる元のサイズ
		private static readonly dSize ocrTemplateSize2 = new dSize(ocrTemplateSize1.Width + 2, ocrTemplateSize1.Height + 2);
		// OCRする際に引き伸ばす際の短辺長
		private static readonly int ocrStretchSize = 64;
		// 各資材における認識パラメーター
		public static Dictionary<string, SupplyParameter> SupplyParameters { get; } = LoadSupplyParameters();
		// OCRする際に使用する画像
		private static readonly Mat templateSource = MakeTemplateSource();
		private static readonly List<bitmapArray> templateSource2 = MakeTemplateSource2();

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
								pair.Value.Threshold,
								pair.Value.Name
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
		// OCR用の画像データを生成する
		private static Mat MakeTemplateSource() {
			string templateChar = "0123456789 ";
			var output = new Bitmap(ocrTemplateSize2.Width * (templateChar.Length * 2 - 1), ocrTemplateSize2.Height);
			// 適当なバッファに文字を出力しつつ、切り取ってテンプレートとする
			for(int k = 0; k < templateChar.Length; ++k) {
				var canvas = new Bitmap(ocrStretchSize * 2, ocrStretchSize * 2);
				using (var g = Graphics.FromImage(canvas)) {
					g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, canvas.Width, canvas.Height));
					g.DrawString(templateChar.Substring(k, 1), new Font("Impact", ocrStretchSize), new SolidBrush(Color.Black), new PointF(0, 0));
				}
				var canvas2 = new Bitmap(ocrTemplateSize2.Width, ocrTemplateSize2.Height);
				using (var g = Graphics.FromImage(canvas2)) {
					// 事前にcanvas3を赤色に塗りつぶす
					g.FillRectangle(Brushes.Red, 0, 0, canvas2.Width, canvas2.Height);
					// 切り取られる位置・大きさ
					var srcRect = GetTrimmingRectangle(canvas);
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(1, 1, ocrTemplateSize1.Width, ocrTemplateSize1.Height);
					g.DrawImage(canvas, desRect, srcRect, GraphicsUnit.Pixel);
				}
				using (var g = Graphics.FromImage(output)) {
					// 切り取られる位置・大きさ
					var srcRect = new Rectangle(0, 0, canvas2.Width, canvas2.Height);
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(k * 2 * ocrTemplateSize2.Width, 0, ocrTemplateSize2.Width, ocrTemplateSize2.Height);
					g.DrawImage(canvas2, desRect, srcRect, GraphicsUnit.Pixel);
				}
			}
			return BitmapConverter.ToMat(output);
		}
		private static List<bitmapArray> MakeTemplateSource2(){
			string templateChar = "0123456789 ";
			var output = new List<bitmapArray>();
			// 適当なバッファに文字を出力しつつ、切り取ってテンプレートとする
			for (int k = 0; k < templateChar.Length; ++k)
			{
				var canvas = new Bitmap(ocrStretchSize * 2, ocrStretchSize * 2);
				using (var g = Graphics.FromImage(canvas))
				{
					g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, canvas.Width, canvas.Height));
					g.DrawString(templateChar.Substring(k, 1), new Font("Impact", ocrStretchSize), new SolidBrush(Color.Black), new PointF(0, 0));
				}
				var canvas2 = new Bitmap(ocrTemplateSize1.Width, ocrTemplateSize1.Height);
				using (var g = Graphics.FromImage(canvas2))
				{
					// 切り取られる位置・大きさ
					var srcRect = GetTrimmingRectangle(canvas);
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(0, 0, ocrTemplateSize1.Width, ocrTemplateSize1.Height);
					g.DrawImage(canvas, desRect, srcRect, GraphicsUnit.Pixel);
				}
				output.Add(BitmapToMonoArray(canvas2));
			}
			return output;
		}
		// 画像を配列化する
		// 参考→
		// 　https://dobon.net/vb/dotnet/graphics/drawnegativeimage.html#lockbits
		// 　http://blueclouds.blog.so-net.ne.jp/2011-04-29
		private static bitmapArray BitmapToMonoArray(Bitmap bitmap){
			var bmpArr = new bitmapArray();
			var bitmapData = bitmap.LockBits(
				new Rectangle(System.Drawing.Point.Empty, bitmap.Size),
				System.Drawing.Imaging.ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb
			);
			//ピクセルデータをバイト型配列で取得する
			var pointer = bitmapData.Scan0;
			byte[] pixels = new byte[bitmapData.Stride * bitmap.Height];
			System.Runtime.InteropServices.Marshal.Copy(pointer, pixels, 0, pixels.Length);
			//すべてのピクセルのデータを取得する
			for (int y = 0; y < bitmapData.Height; ++y){
				for (int x = 0; x < bitmapData.Width; ++x){
					//ピクセルデータでのピクセル(x,y)の開始位置を計算する
					int pos = y * bitmapData.Stride + x * 4;
					byte b = pixels[pos];
					byte g = pixels[pos + 1];
					byte r = pixels[pos + 2];
					byte a = pixels[pos + 3];
					byte grayColor = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
					bmpArr.Add(grayColor);
					continue;
				}
			}
			bitmap.UnlockBits(bitmapData);
			return bmpArr;
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
			if (!SupplyParameters.ContainsKey(supplyType))
				return -1;
			// ％指定をピクセル指定に直す
			var cropRect = SupplyParameters[supplyType].Rect;
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
			if (debugFlg) canvas.Save($"debug\\digit-{supplyType}-step1.png");
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
				if (SupplyParameters[supplyType].InverseFlg)
					Cv2.BitwiseNot(mat2, mat2);
				if (debugFlg) mat2.SaveImage($"debug\\digit-{supplyType}-step2.png");
				Cv2.Threshold(mat2, mat2, SupplyParameters[supplyType].Threshold, 255, ThresholdTypes.Binary);
				canvas = mat2.ToBitmap();
			}
			if (debugFlg) canvas.Save($"debug\\digit-{supplyType}-step3.png");
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
				if (debugFlg) canvas2.Save($"debug\\digit-{supplyType}-step4-{k + 1}-1.png");
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
				if (debugFlg) canvas3.Save($"debug\\digit-{supplyType}-step4-{k + 1}-2.png");
				// マッチングを行う
				using (var image = BitmapConverter.ToMat(canvas3)) {
					// テンプレートを読み込み、そこから検索を行う
					var resultSize = new OpenCvSharp.Size(templateSource.Width - image.Width + 1, templateSource.Height - image.Height + 1);
					using (var resultImage = new Mat(resultSize, MatType.CV_32F)) {
						Cv2.MatchTemplate(templateSource, image, resultImage, TemplateMatchModes.SqDiff);
						OpenCvSharp.Point minPosition, maxPosition;
						Cv2.MinMaxLoc(resultImage, out minPosition, out maxPosition);
						int xpos = minPosition.X;
						// 検索した座標から、数字を特定する
						int matchNumber = (int)Math.Round(1.0 * xpos / ocrTemplateSize2.Width / 2, 0);
						matchNumber = (matchNumber < 0 ? 0 : matchNumber >= 10 ? 0 : matchNumber);
						digit.Add(matchNumber);
					}
				}
			}
			// 結果を数値化する
			int retVal = 0;
			foreach (int x in digit) {
				retVal *= 10;
				retVal += x;
			}
			return retVal;
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
			// DBのテーブル名
			public string Name { get; }
			// コンストラクタ
			public SupplyParameter(int r, int g, int b, float x, float y, float width, float height,
				bool mainSupplyFlg, bool secondaryAxisFlg, bool inverseFlg, int threshold, string name) {
				Color = Color.FromArgb(r, g, b);
				Rect = new RectangleF(x, y, width, height);
				MainSupplyFlg = mainSupplyFlg;
				SecondaryAxisFlg = secondaryAxisFlg;
				InverseFlg = inverseFlg;
				Threshold = threshold;
				Name = name;
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
			public string Name { get; set; }
		}
	}
}
