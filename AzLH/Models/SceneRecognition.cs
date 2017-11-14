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

		// 認識パラメーターを読み込む
		private static Dictionary<string, SceneParameter[]> LoadSceneParameters() {
			// Dictionaryを準備
			var output = new Dictionary<string, SceneParameter[]>();
			// ファイルを読み込む
			MemoryStream ms = null;
			try {
				ms = new MemoryStream(Properties.Resources.scene_parameter, false);
				using (var sr = new System.IO.StreamReader(ms, Encoding.UTF8)) {
					ms = null;
					// 全体をstringに読み込む
					string json = sr.ReadToEnd();
					// Json.NETでパース
					var model = JsonConvert.DeserializeObject<Dictionary<string, SceneParameterJson[]>>(json);
					// パース結果をさらに変換
					foreach (var pair in model) {
						// LINQを用いて一発で放り込む
						output[pair.Key] = pair.Value.Select(
							p => new SceneParameter(
								Convert.ToUInt64(p.HashStr, 16),
								p.RectFloat[0],
								p.RectFloat[1],
								p.RectFloat[2],
								p.RectFloat[3]
							)
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
		// ハミング距離を計算する
		private static ulong GetHummingDistance(ulong a, ulong b) {
			return Popcnt(a ^ b);
		}
		// 画像の一部分におけるDifferenceHashを取得する
		// (rectで指定する範囲は％単位)
		private static ulong GetDifferenceHash(Bitmap bitmap, RectangleF rect) {
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
					ulong hash = GetDifferenceHash(bitmap, sceneParameter.Rect);
					if(GetHummingDistance(hash, sceneParameter.Hash) >= 20) {
						flg = false;
						break;
					}
				}
				if (flg)
					return scene.Key;
			}
			return "不明";
		}

		// シーンの認識パラメーターを表すクラス
		private class SceneParameter {
			// ハッシュ値
			public ulong Hash { get; }
			// 指定する範囲(％単位)
			public RectangleF Rect { get; }
			// コンストラクタ
			public SceneParameter(ulong hash, float x, float y, float width, float height) {
				Hash = hash;
				Rect = new RectangleF(x, y, width, height);
			}
		}
		[JsonObject("param")]
		private class SceneParameterJson {
			[JsonProperty("hash")]
			public string HashStr { get; set; }
			[JsonProperty("rect")]
			public float[] RectFloat { get; set; }
		}
	}
}
