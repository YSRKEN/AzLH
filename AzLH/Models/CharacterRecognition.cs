using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AzLH.Models {
	static class CharacterRecognition {
		// 燃料：～10000　通常資材左
		// 資金：～50000　通常資材右
		// キューブ：～100　特殊資材左
		// ドリル：～100　特殊資材左
		// 勲章：～100　特殊資材左
		// ダイヤ：～1000　特殊資材右
		// 家具コイン：～1000　特殊資材右

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

		// 資材量を読み取る(-1＝読み取り不可)
		public static int GetValueOCR(Bitmap bitmap, string supplyType) {
			// 対応している資材名出ない場合は無視する
			if (!supplyParameters.ContainsKey(supplyType))
				return -1;
			// OCR処理を行う(スタブ)
			var rect = supplyParameters[supplyType].Rect;
			return 0;
		}

		// 資材の認識パラメーターを表すクラス
		private class SupplyParameter {
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
		private class SceneParameterJson {
			public int[] Color { get; set; }
			public float[] RectFloat { get; set; }
			public bool MainSupplyFlg { get; set; }
			public bool SecondaryAxisFlg { get; set; }
			public bool InverseFlg { get; set; }
			public int Threshold { get; set; }
		}
	}
}
