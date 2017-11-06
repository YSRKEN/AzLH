using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;

namespace AzLH.Models {
	using DSize = System.Drawing.Size;
	static class ScreenShotProvider {
		#region 定数宣言
		// 取得できるゲーム画面の最小サイズ
		private static readonly DSize MinGameWindowSize = new DSize(1280, 720);
		// 探索用ステップ数
		private static readonly int GameWindowSearchStepCount = 3;
		// 探索用ステップ数に基づく取得間隔
		private static readonly DSize GameWindowSearchStep = new DSize(
			MinGameWindowSize.Width / (GameWindowSearchStepCount + 1),
			MinGameWindowSize.Height / (GameWindowSearchStepCount + 1));
		// 取得できるゲーム画面の最大サイズ
		private static readonly DSize MaxGameWindowSize = new DSize(2560, 1440);
		#endregion

		// 引数なし→仮想画面全体のスクリーンショットを取得する
		// 引数あり→仮想画面の指定した範囲を切り取ったスクリーンショットを取得する
		public static Bitmap GetScreenBitmap() {
			//System.Drawing.dllの参照を追加しておくのがポイント
			//https://social.msdn.microsoft.com/Forums/vstudio/en-US/7a3d2cee-2e72-420d-b596-d51f7002a07e/wpf-screen-capture-with-rectangle
			int top = (int)SystemParameters.VirtualScreenTop;
			int left = (int)SystemParameters.VirtualScreenLeft;
			int width = (int)SystemParameters.VirtualScreenWidth;
			int height = (int)SystemParameters.VirtualScreenHeight;
			var virtualScreenBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			using (var bitmapGraphics = Graphics.FromImage(virtualScreenBitmap)) {
				bitmapGraphics.CopyFromScreen(top, left, 0, 0, virtualScreenBitmap.Size);
			}
			return virtualScreenBitmap;
		}
		public static Bitmap GetScreenBitmap(Rectangle rect) {
			var virtualScreenBitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
			using (var bitmapGraphics = Graphics.FromImage(virtualScreenBitmap)) {
				bitmapGraphics.CopyFromScreen(rect.Top, rect.Left, 0, 0, virtualScreenBitmap.Size);
			}
			return virtualScreenBitmap;
		}

		// ゲーム画面の位置を全ディスプレイから検索し、取得する
		// 引数なし→仮想画面全体のスクリーンショットから検索する
		// 引数あり→渡されたBitmapから検索する
		public static List<Rectangle> GetGameWindowPosition() {
			//仮想画面全体のスクリーンショットを取得する
			using (var virtualScreenBitmap = GetScreenBitmap()) {
				// ゲーム画面の位置と思われるrectを検索する
				return GetGameWindowPosition(virtualScreenBitmap);
			}
		}
		public static List<Rectangle> GetGameWindowPosition(Bitmap bitmap) {
			// 上端の候補を検索する
			// 1. GameWindowSearchStep.Width ピクセルごとに画素を読み取る(Y=yとY=y+1)
			// 2. 以下の2配列の中で、「A1～A{GameWindowSearchStepCount}は全部同じ色」かつ
			//    「B1～B{GameWindowSearchStepCount}のどれかは違う色」である箇所を見つける
			//   Y=y  [..., A1, A2, .., Ax, ...]
			//   Y=y+1[..., B1, B2, .., Bx, ...]
			var topList = new List<int>();
			{
				var listA = new List<Color>();
				for (int x = 0; x < bitmap.Width; x += GameWindowSearchStep.Width) {
					listA.Add(bitmap.GetPixel(x, 0));
				}
				for (int y = 1; y < bitmap.Height - MinGameWindowSize.Height - 1; ++y) {
					var listB = new List<Color>();
					for (int x = 0; x < bitmap.Width; x += GameWindowSearchStep.Width) {
						listB.Add(bitmap.GetPixel(x, y));
					}
					for (int k = 0; k < listA.Count - GameWindowSearchStepCount; ++k) {
						var clrA = listA[k];
						{
							// 全要素がlistA[k]と同じ値、ではない場合を弾く
							bool flg = true;
							for (int c = 1; c < GameWindowSearchStepCount; ++c) {
								if (listA[k + c] != clrA) {
									flg = false;
									break;
								}
							}
							if (!flg)
								continue;
						}
						var clrB = listB[k];
						{
							// 全要素がlistB[k]と同じ値、ならば弾く
							bool flg = false;
							for (int c = 1; c < GameWindowSearchStepCount; ++c) {
								if (listA[k + c] != clrB) {
									flg = true;
									break;
								}
							}
							if (!flg)
								continue;
						}
						{
							// 全要素がlistA[k]と同じ値、ならば弾く
							bool flg = false;
							for (int c = 1; c < GameWindowSearchStepCount; ++c) {
								if (listB[k + c] != clrA) {
									flg = true;
									break;
								}
							}
							if (!flg)
								continue;
						}
						topList.Add(y - 1);
						break;
					}
					listA = listB;
				}
			}
			// 左辺を検索する
			var leftList = new List<int>();
			{
				var listA = new List<Color>();
				for (int y = 0; y < bitmap.Height; y += GameWindowSearchStep.Height) {
					listA.Add(bitmap.GetPixel(0, y));
				}
				for (int x = 1; x < bitmap.Width - MinGameWindowSize.Width - 1; ++x) {
					var listB = new List<Color>();
					for (int y = 0; y < bitmap.Height; y += GameWindowSearchStep.Height) {
						listB.Add(bitmap.GetPixel(x, y));
					}
					for (int k = 0; k < listA.Count - GameWindowSearchStepCount; ++k) {
						var clrA = listA[k];
						{
							// 全要素がlistA[k]と同じ値、ではない場合を弾く
							bool flg = true;
							for (int c = 1; c < GameWindowSearchStepCount; ++c) {
								if (listA[k + c] != clrA) {
									flg = false;
									break;
								}
							}
							if (!flg)
								continue;
						}
						var clrB = listB[k];
						{
							// 全要素がlistB[k]と同じ値、ならば弾く
							bool flg = false;
							for (int c = 1; c < GameWindowSearchStepCount; ++c) {
								if (listA[k + c] != clrB) {
									flg = true;
									break;
								}
							}
							if (!flg)
								continue;
						}
						{
							// 全要素がlistA[k]と同じ値、ならば弾く
							bool flg = false;
							for (int c = 1; c < GameWindowSearchStepCount; ++c) {
								if (listB[k + c] != clrA) {
									flg = true;
									break;
								}
							}
							if (!flg)
								continue;
						}
						leftList.Add(x - 1);
						break;
					}
					listA = listB;
				}
			}
			// 上辺・左辺から決まる各候補について、Rectとしての条件を満たせるかをチェックする
			var rectList = new List<Rectangle>();
			foreach (int top in topList) {
				foreach (int left in leftList) {
					// 枠の基準色を決める
					var baseColor = bitmap.GetPixel(left, top);
					// MinGameWindowSize～MaxGameWindowSizeまで、サイズを検証する
					for (int width = MinGameWindowSize.Width; width <= MaxGameWindowSize.Width; ++width) {
						// ゲーム画面の縦の大きさを算出する
						int height = width * MinGameWindowSize.Height / MinGameWindowSize.Width;
						// サイズが大きすぎないかをチェックする
						if(left + width + 1 >= bitmap.Width)
							break;
						if (top + height + 1 >= bitmap.Height)
							break;
						// 下端チェック
						{
							bool flg = true;
							for (int k = 1; k <= width; k += GameWindowSearchStep.Width) {
								if (bitmap.GetPixel(left + k, top + height + 1) != baseColor) {
									flg = false;
									break;
								}
							}
							if (!flg)
								continue;
						}
						// 右端チェック
						{
							bool flg = true;
							for (int k = 1; k <= height; k += GameWindowSearchStep.Height) {
								if (bitmap.GetPixel(left + width + 1, top + k) != baseColor) {
									flg = false;
									break;
								}
							}
							if (!flg)
								continue;
						}
						// 最終チェック
						{
							bool flg = true;
							for(int k = 0; k <= width + 1; ++k) {
								if(bitmap.GetPixel(left + k, top) != baseColor) {
									flg = false;
									break;
								}
								if (bitmap.GetPixel(left + k, top + height + 1) != baseColor) {
									flg = false;
									break;
								}
							}
							if (!flg)
								continue;
							for (int k = 0; k <= height + 1; ++k) {
								if (bitmap.GetPixel(left, top + k) != baseColor) {
									flg = false;
									break;
								}
								if (bitmap.GetPixel(left + width + 1, top + k) != baseColor) {
									flg = false;
									break;
								}
							}
							if (!flg)
								continue;
						}
						// 候補を追加
						rectList.Add(new Rectangle(left + 1, top + 1, width, height));
						break;
					}
				}
			}
			return rectList;
		}
	}
}
