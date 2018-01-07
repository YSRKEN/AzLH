using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace AzLH.Models {
	static class SceneRecognition {
		// 各シーンにおける認識パラメーター
		private static Dictionary<string, SceneParameter[]> sceneParameters = LoadSceneParameters();
		// 戦闘中の各種ゲージの種類数
		public static int GaugeTypeCount { get; } = 3;
		// 戦闘中の各種ゲージが光っているかを判定するためのRect
		private static RectangleF[] gaugeChargeRect = new RectangleF[] {
			new RectangleF(65.63f, 84.72f, 1.406f, 1.250f),
			new RectangleF(79.14f, 82.22f, 0.7813f, 1.389f),
			new RectangleF(94.14f, 83.61f, 0.8594f, 1.111f),
		};
		// 戦闘中の各種ゲージの中央座標のPoint
		private static PointF[] gaugeChargePoint = new PointF[] {
			new PointF(65.586f, 84.375f),
			new PointF(79.180f, 84.375f),
			new PointF(92.695f, 84.375f),
		};
		// 戦闘中の各種ゲージの円の半径(横・縦の比率)
		private static SizeF gaugeChargeR = new SizeF(4.6875f, 8.3333f);

		// 認識パラメーターを読み込む
		private static Dictionary<string, SceneParameter[]> LoadSceneParameters() {
			// Dictionaryを準備
			var output = new Dictionary<string, SceneParameter[]>();
			// ファイルを読み込む
			MemoryStream ms = null;
			try {
				ms = new MemoryStream(Properties.Resources.scene_parameter, false);
				using (var sr = new StreamReader(ms, Encoding.UTF8)) {
					ms = null;
					// 全体をstringに読み込む
					string json = sr.ReadToEnd();
					// Json.NETでパース
					var model = JsonConvert.DeserializeObject<Dictionary<string, SceneParameterJson[]>>(json);
					// パース結果をさらに変換
					foreach (var pair in model) {
						// LINQを用いて一発で放り込む
						output[pair.Key] = pair.Value.Select<SceneParameterJson, SceneParameter>(
							p => {
								var rect = new RectangleF(p.RectFloat[0], p.RectFloat[1], p.RectFloat[2], p.RectFloat[3]);
								ulong param = Convert.ToUInt64(p.ParamStr, 16);
								switch (p.TypeStr) {
								case "DifferenceHash":
									return new SceneParameterDH { Rect = rect, Hash = param };
								case "AverageColor":
									return new SceneParameterAC { Rect = rect, Color = Color.FromArgb((0xFF << 24) | (int)param) };
								default:
									return new SceneParameterDH();
								}
							}
						).ToArray();
					}
				}
			}
			finally {
				if (ms != null)
					ms.Dispose();
			}
			return output;
		}
		// ビットカウント
		// 参考→http://developer.cybozu.co.jp/takesako/2006/11/binary_hacks.html
		private static ulong Popcnt(ulong x) {
			x = ((x & 0xaaaaaaaaaaaaaaaa) >> 1) + (x & 0x5555555555555555);
			x = ((x & 0xcccccccccccccccc) >> 2) + (x & 0x3333333333333333);
			x = ((x & 0xf0f0f0f0f0f0f0f0) >> 4) + (x & 0x0f0f0f0f0f0f0f0f);
			x = ((x & 0xff00ff00ff00ff00) >> 8) + (x & 0x00ff00ff00ff00ff);
			x = ((x & 0xffff0000ffff0000) >> 16) + (x & 0x0000ffff0000ffff);
			x = ((x & 0xffffffff00000000) >> 32) + (x & 0x00000000ffffffff);
			return x;
		}
		// 画像の一部分の平均色を取得する
		// (rectで指定する範囲は％単位)
		private static Color GetAverageColor(Bitmap bitmap, RectangleF rect) {
			// ％指定をピクセル指定に直す
			int px = (int)(bitmap.Width * rect.X / 100 + 0.5);
			int py = (int)(bitmap.Height * rect.Y / 100 + 0.5);
			int wx = (int)(bitmap.Width * rect.Width / 100 + 0.5);
			int wy = (int)(bitmap.Height * rect.Height / 100 + 0.5);
			// 画素値の平均を取る
			ulong rSum = 0, gSum = 0, bSum = 0;
			for(int y = py; y < py + wy; ++y) {
				for (int x = px; x < px + wx; ++x) {
					var color = bitmap.GetPixel(x, y);
					rSum += color.R;
					gSum += color.G;
					bSum += color.B;
				}
			}
			int rAve = (int)(1.0 * rSum / wx / wy + 0.5);
			int gAve = (int)(1.0 * gSum / wx / wy + 0.5);
			int bAve = (int)(1.0 * bSum / wx / wy + 0.5);
			rAve = (rAve < 0 ? 0 : rAve > 255 ? 255 : rAve);
			gAve = (gAve < 0 ? 0 : gAve > 255 ? 255 : gAve);
			bAve = (bAve < 0 ? 0 : bAve > 255 ? 255 : bAve);
			return Color.FromArgb(rAve, gAve, bAve);
		}
		// 色間の距離を取得する(単純にR・G・Bの差の二乗の合計を出しているだけ)
		private static int GetColorDistance(Color a, Color b) {
			int rDiff = a.R - b.R;
			int gDiff = a.G - b.G;
			int bDiff = a.B - b.B;
			return rDiff * rDiff + gDiff * gDiff + bDiff * bDiff;
		}

		// ハミング距離を計算する
		public static ulong GetHummingDistance(ulong a, ulong b) {
			return Popcnt(a ^ b);
		}
		// 画像の一部分におけるDifferenceHashを取得する
		// (rectで指定する範囲は％単位)
		public static ulong GetDifferenceHash(Bitmap bitmap, RectangleF rect) {
			// 以下の3つの作業を同時に行う
			// ・画像を切り取る
			// ・横9ピクセル縦8ピクセルにリサイズする
			// ・グレースケール化を実行する
			// 参考→http://www.r-nakai.com/archives/30
			var canvas = new Bitmap(9, 8);
			using (var g = Graphics.FromImage(canvas)) {
				// 切り取られる位置・大きさ
				// (％指定をピクセル指定に直している)
				var srcRect = new Rectangle(
					(int)Math.Round(bitmap.Width * rect.X / 100),
					(int)Math.Round(bitmap.Height * rect.Y / 100),
					(int)Math.Round(bitmap.Width * rect.Width / 100),
					(int)Math.Round(bitmap.Height * rect.Height / 100)
				);
				// 貼り付ける位置・大きさ
				var desRect = new Rectangle(0, 0, canvas.Width, canvas.Height);
				// グレースケール変換用のマトリックスを設定
				var cm = new ColorMatrix(
					new float[][]{
						new float[]{0.299f, 0.299f, 0.299f, 0 ,0},
						new float[]{0.587f, 0.587f, 0.587f, 0, 0},
						new float[]{0.114f, 0.114f, 0.114f, 0, 0},
						new float[]{0, 0, 0, 1, 0},
						new float[]{0, 0, 0, 0, 1}
					}
				);
				var ia = new ImageAttributes();
				ia.SetColorMatrix(cm);
				// 描画
				g.DrawImage(
					bitmap, desRect, srcRect.X, srcRect.Y,
					srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, ia
				);
			}
			// 隣接ピクセルとの比較結果を符号化する
			// (グレースケール化したので各画素のR値＝G値＝B値。
			// なのでとりあえずR値を使用している)
			ulong hash = 0;
			for (int y = 0; y < 8; ++y) {
				for (int x = 0; x < 8; ++x) {
					hash <<= 1;
					if (canvas.GetPixel(x, y).R > canvas.GetPixel(x + 1, y).R)
						hash |= 1;
				}
			}
			return hash;
		}
		// どのシーンかを判定する("不明"＝判定不可)
		public static string JudgeGameScene(Bitmap bitmap) {
			foreach(var scene in sceneParameters) {
				bool flg = true;
				foreach(var sceneParameter in scene.Value) {
					if(sceneParameter is SceneParameterDH) {
						ulong hash = GetDifferenceHash(bitmap, ((SceneParameterDH)sceneParameter).Rect);
						if (GetHummingDistance(hash, ((SceneParameterDH)sceneParameter).Hash) >= 20) {
							flg = false;
							break;
						}
					} else if(sceneParameter is SceneParameterAC) {
						var color = GetAverageColor(bitmap, ((SceneParameterAC)sceneParameter).Rect);
						if(GetColorDistance(color, ((SceneParameterAC)sceneParameter).Color) >= 50) {
							flg = false;
							break;
						}
					}
				}
				if (flg) {
					// 特定のシーンの時だけ判定を追加する
					switch (scene.Key) {
					case "戦闘中": {
							foreach(var rect in gaugeChargeRect){
								var centerColor = GetAverageColor(bitmap, rect);
								if (GetColorDistance(centerColor, Color.FromArgb(247, 251, 247)) >= 500
								&& GetColorDistance(centerColor, Color.FromArgb(189, 186, 189)) >= 500) {
									return "不明";
								}
							}
						}
						break;
					}
					return scene.Key;
				}
			}
			return "不明";
		}
		// 戦闘中の画面にて、空爆・魚雷・砲撃のゲージ量を読み取って返す
		public static double[] GetBattleBombGauge(Bitmap bitmap) {
			var gauge = new double[GaugeTypeCount];
			for(int ti = 0; ti < GaugeTypeCount; ++ti) {
				// 読み取れなかった際の割合を0.0とする
				gauge[ti] = 0.0;
				// ゲージが最大まで溜まっている場合は、常に1を返す
				// (そうでない場合、(189,186,189)になる)
				var centerColor = GetAverageColor(bitmap, gaugeChargeRect[ti]);
				if(GetColorDistance(centerColor, Color.FromArgb(247, 251, 247)) < 500) {
					gauge[ti] = 1.0;
					continue;
				}
				// そうでない場合は、ゲージがどこまでどこまで読み取れるかを返す
				for(int intPer = 999; intPer >= 0; --intPer) {
					// doublePerは[0, 1)の範囲内で、ゲージの割合を表す
					double doublePer = 0.001 * intPer;
					// doublePerを座標変換して円形にしつつ、ピクセル座標まで落とし込む
					double radianAngle = doublePer * Math.PI * 2;
					double xOffset = Math.Sin(radianAngle);
					double yOffset = -Math.Cos(radianAngle);
					double xPoint = gaugeChargePoint[ti].X + xOffset * gaugeChargeR.Width;
					double yPoint = gaugeChargePoint[ti].Y + yOffset * gaugeChargeR.Height;
					int xPixelPoint = (int)Math.Round(xPoint * bitmap.Width / 100, 0);
					int yPixelPoint = (int)Math.Round(yPoint * bitmap.Height / 100, 0);
					var pixelColor = bitmap.GetPixel(xPixelPoint, yPixelPoint);
					if(GetColorDistance(pixelColor, Color.FromArgb(255,215,66)) < 50) {
						// doublePerが特定の範囲内にある場合、ゲージ下にある「数字」と
						// 重なってしまうため測定精度が下がる。そこで、度数法だと140°～220°、
						// [0, 1)に正規化すると0.38～0.61までの範囲にあるなら「無視」するようにした
						if(380 <= intPer && intPer <= 610)
							gauge[ti] = -1.0;
						else
							gauge[ti] = doublePer;
						break;
					}
				}
			}
			return gauge;
		}

		// シーンの認識パラメーターを表すクラス
		private interface SceneParameter { }
		private struct SceneParameterDH : SceneParameter {
			// 指定する範囲(％単位)
			public RectangleF Rect { get; set; }
			// パラメーター
			public ulong Hash { get; set; }
		}
		private struct SceneParameterAC : SceneParameter {
			// 指定する範囲(％単位)
			public RectangleF Rect { get; set; }
			// パラメーター
			public Color Color { get; set; }
		}
		[JsonObject("param_pair")]
		private class SceneParameterJson {
			[JsonProperty("type")]
			public string TypeStr { get; set; }
			[JsonProperty("rect")]
			public float[] RectFloat { get; set; }
			[JsonProperty("param")]
			public string ParamStr { get; set; }
		}
	}
}
