﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace AzLH.Models {
	using DSize = System.Drawing.Size;
	internal static class ScreenShotProvider {
		#region 定数宣言
		// 取得できるゲーム画面の最小サイズ
		private static readonly DSize MinGameWindowSize = new DSize(1280, 720);
		// 探索用ステップ数
		private const int GameWindowSearchStepCount = 5;
		// 探索用ステップ数に基づく取得間隔
		private static readonly DSize GameWindowSearchStep = new DSize(
			MinGameWindowSize.Width / (GameWindowSearchStepCount + 1),
			MinGameWindowSize.Height / (GameWindowSearchStepCount + 1));
		// 取得できるゲーム画面の最大サイズ
		private static readonly DSize MaxGameWindowSize = new DSize(2560, 1440);
		// 枠検出に利用する定数
		private static readonly List<DSize> GameWindowSizeList = InitializeGameWindowSizeList();
		#endregion

		// nullではない場合、スクリーンショットを取得可能ということになる
		public static Rectangle? GameWindowRect { get; set; }
		// nullではない場合、特殊なスクショ取得メソッドを使用可能ということになる
		public static IntPtr? GameWindowHandle { get; set; }

		// 枠検出に利用する定数を初期化
		private static List<DSize> InitializeGameWindowSizeList() {
			var list = new List<DSize>();
			for(int i = MinGameWindowSize.Width; i <= MaxGameWindowSize.Width; ++i) {
				int xSize = i + 1;
				int ySize1 = (i * MinGameWindowSize.Height / MinGameWindowSize.Width) - 1;
				int ySize2 = ySize1 + 1;
				int ySize3 = ySize2 + 1;
				list.Add(new DSize(xSize, ySize1));
				list.Add(new DSize(xSize, ySize2));
				list.Add(new DSize(xSize, ySize3));
			}
			return list;
		}
		// スクリーンショットを保存する
		public static Bitmap GetScreenshot(bool forTwitterFlg = false, bool specialScreenShotMethodFlg = false) {
			// スクショを取得する
			Bitmap screenShot;
			if (specialScreenShotMethodFlg) {
				screenShot = GetGameWindowBitmap((Rectangle)GameWindowRect, (IntPtr)GameWindowHandle);
			} else {
				screenShot = GetScreenBitmap((Rectangle)GameWindowRect);
			}
			// Twitter用に左上を僅かに透過させる
			if (forTwitterFlg) {
				var color = screenShot.GetPixel(0, 0);
				int alphaValue = (color.A == 0 ? 1 : color.A - 1);
				screenShot.SetPixel(0, 0, Color.FromArgb(alphaValue, color.R, color.G, color.B));
			}
			return screenShot;
		}
		// スクリーンショットを保存できるかを判定する(ズレチェック)
		public static bool CanGetScreenshot() {
			if (GameWindowRect == null)
				return false;
			try {
				// 通常より一回り大きな範囲を取得する
				using (var bitmap = GetScreenBitmap(new Rectangle(
					GameWindowRect.Value.X - 1,
					GameWindowRect.Value.Y - 1,
					GameWindowRect.Value.Width + 2,
					GameWindowRect.Value.Height + 2
				))) {
					// 左上の色(つまり枠の色)を取得する
					var baseColor = bitmap.GetPixel(0, 0);
					for (int k = 0; k < bitmap.Width; k += GameWindowSearchStep.Width) {
						if (bitmap.GetPixel(k, 0) != baseColor) {
							return false;
						}
						if (bitmap.GetPixel(k, bitmap.Height - 1) != baseColor) {
							return false;
						}
					}
					for (int k = 0; k < bitmap.Height; k += GameWindowSearchStep.Height) {
						if (bitmap.GetPixel(0, k) != baseColor) {
							return false;
						}
						if (bitmap.GetPixel(bitmap.Width - 1, k) != baseColor) {
							return false;
						}
					}
				}
			}
			catch {
				return false;
			}
			return true;
		}

		// スクリーンショットを取得する
		// 引数なし→仮想画面全体のスクリーンショットを取得する
		// 引数あり→仮想画面の指定した範囲を切り取ったスクリーンショットを取得する
		public static Bitmap GetScreenBitmap() {
			//System.Drawing.dllの参照を追加しておくのがポイント
			//https://social.msdn.microsoft.com/Forums/vstudio/en-US/7a3d2cee-2e72-420d-b596-d51f7002a07e/wpf-screen-capture-with-rectangle
			int left = (int)SystemParameters.VirtualScreenLeft;
			int top = (int)SystemParameters.VirtualScreenTop;
			int width = (int)SystemParameters.VirtualScreenWidth;
			int height = (int)SystemParameters.VirtualScreenHeight;
			var virtualScreenBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			using (var bitmapGraphics = Graphics.FromImage(virtualScreenBitmap)) {
				bitmapGraphics.CopyFromScreen(left, top, 0, 0, virtualScreenBitmap.Size);
			}
			return virtualScreenBitmap;
		}
		public static Bitmap GetScreenBitmap(Rectangle rect) {
			var virtualScreenBitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
			using (var bitmapGraphics = Graphics.FromImage(virtualScreenBitmap)) {
				bitmapGraphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, virtualScreenBitmap.Size);
			}
			return virtualScreenBitmap;
		}
		// 特殊なスクショ取得メソッド
		public static Bitmap GetGameWindowBitmap(Rectangle rect, IntPtr handle) {
			var hSrcDC = NativeMethods.GetWindowDC(handle);
			var bitmap = new Bitmap(rect.Width, rect.Height);
			using (var g = Graphics.FromImage(bitmap)) {
				var hDestDC = g.GetHdc();
				NativeMethods.BitBlt(hDestDC, 0, 0, bitmap.Width, bitmap.Height,
					hSrcDC, rect.X, rect.Y, NativeMethods.SRCCOPY);
				g.ReleaseHdc();
			}
			NativeMethods.ReleaseDC(handle, hSrcDC);
			bitmap.Save(@"debug\bitblt.bmp");
			return bitmap;
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
		public static Task<List<Rectangle>> GetGameWindowPositionAsync() {
			return Task.Run(() => GetGameWindowPosition());
		}
		public static List<Rectangle> GetGameWindowPosition(Bitmap bitmap) {
			// 上辺の候補を検索する
			// 1. GameWindowSearchStep.Width ピクセルごとに画素を読み取る(Y=yとY=y+1)
			// 2. 以下の2配列の中で、「A1～A{GameWindowSearchStepCount}は全部同じ色」かつ
			//    「B1～B{GameWindowSearchStepCount}のどれかはAxと違う色」である箇所を見つける
			//   Y=y  [..., A1, A2, .., Ax, ...]
			//   Y=y+1[..., B1, B2, .., Bx, ...]
			var topList = new List<KeyValuePair<int, int>>();
			{
				// 事前計算
				//横幅÷ステップ幅
				int stepCount = bitmap.Width / GameWindowSearchStep.Width;
				//探索するステップの右端
				int maxSearchWidth = bitmap.Width  - (GameWindowSearchStepCount * GameWindowSearchStep.Width);
				//探索する縦範囲
				int maxSearchHeight = bitmap.Height - MinGameWindowSize.Height - 1;
				for (int y = 0; y < maxSearchHeight; ++y) {
					// KMP法を応用した検索方法
					for (int x = 0; x < maxSearchWidth; x += GameWindowSearchStep.Width) {
						// 基準となる色(A1)を取得する
						var baseColor = bitmap.GetPixel(x, y);
						int nextPos = 0;
						// 基準色がGameWindowSearchStepCount回連続しているかを調べる
						for (int k = 1, x2 = x + GameWindowSearchStep.Width;
							k < GameWindowSearchStepCount; ++k, x2 += GameWindowSearchStep.Width) {
							if (bitmap.GetPixel(x2, y) != baseColor) {
								nextPos = k;
								break;
							}
						}
						// 連続していたらBx側のチェックを行う。そうでない場合はうまく探索箇所を飛ばす
						if (nextPos == 0) {
							// Bx側をチェックする
							bool bxFlg = false;
							for (int k = 0, x2 = x; k < GameWindowSearchStepCount; ++k, x2 += GameWindowSearchStep.Width) {
								if (bitmap.GetPixel(x2, y + 1) != baseColor) {
									bxFlg = true;
									break;
								}
							}
							if (bxFlg) {
								topList.Add(new KeyValuePair<int, int>(y, x));
							}
						}
						else {
							x += (nextPos - 1) * GameWindowSearchStep.Width;
						}
					}
				}
			}
			// 上辺の候補の左側を軽く走査し、左辺となりうる辺を持ちうるかを調査する
			var pointList = new List<KeyValuePair<int, int>>();
			foreach(var top in topList) {
				// ただの置き換え
				int y = top.Key;
				// 左上座標のX座標値の限界
				int searchLimitX = Math.Max(0, top.Value - GameWindowSearchStep.Width);
				// 枠の基準となる色
				var baseColor = bitmap.GetPixel(top.Value, y);
				// 探索開始
				for (int x = top.Value; x >= searchLimitX; --x) {
					// 左上座標の候補がbaseColorと色を違えた場合はその時点で検索を終了する
					if (bitmap.GetPixel(x, y) != baseColor)
						break;
					// 左上座標から縦方向に見ていって、baseColorと色を違えた場合は次の候補に移る
					{
						bool safeFlg = true;
						for (int k = 0, y2 = y + 1; k < MinGameWindowSize.Height;
							k += GameWindowSearchStep.Height, y2 += GameWindowSearchStep.Height) {
							if (bitmap.GetPixel(x, y2) != baseColor) {
								safeFlg = false;
								break;
							}
						}
						if (!safeFlg)
							continue;
					}
					// 左辺候補の1ピクセル内側におけるチェック
					{
						bool safeFlg = false;
						for (int k = 0, y2 = y + 1; k < MinGameWindowSize.Height;
							k += GameWindowSearchStep.Height, y2 += GameWindowSearchStep.Height) {
							if (bitmap.GetPixel(x + 1, y2) != baseColor) {
								safeFlg = true;
								break;
							}
						}
						if (!safeFlg)
							continue;
					}
					// 候補を追加する
					pointList.Add(new KeyValuePair<int, int>(x, y));
				}
			}
			//稀にpointListの内容がダブることがあるので修正
			//参考→http://www.atmarkit.co.jp/ait/articles/1703/29/news027.html
			pointList = pointList.GroupBy(p => p).Select(group => group.First()).ToList();
			// 上辺・左辺から決まる各候補について、Rectとしての条件を満たせるかをチェックする
			var rectList = new List<Rectangle>();
			foreach (var point in pointList) {
				int left = point.Key;
				int top = point.Value;
				// 枠の基準色を決める
				var baseColor = bitmap.GetPixel(left, top);
				// まず、MinGameWindowSizeまで大丈夫かを検証する
				{
					// 上辺
					bool flg = true;
					for (int k = 1; k <= MinGameWindowSize.Width; k += GameWindowSearchStep.Width) {
						if (bitmap.GetPixel(left + k, top) != baseColor) {
							flg = false;
							break;
						}
						if (!flg)
							continue;
					}
				}
				{
					// 左辺
					bool flg = true;
					for (int k = 1; k <= MinGameWindowSize.Height; k += GameWindowSearchStep.Height) {
						if (bitmap.GetPixel(left, top + k) != baseColor) {
							flg = false;
							break;
						}
						if (!flg)
							continue;
					}
				}
				// 次に、MinGameWindowSize～MaxGameWindowSizeまで各サイズについて検証する
				foreach (var frameSize in GameWindowSizeList) {
					// サイズが大きすぎないかをチェックする
					if (left + frameSize.Width >= bitmap.Width)
						break;
					if (top + frameSize.Height >= bitmap.Height)
						break;
					// 下端チェック
					{
						bool flg = true;
						for (int k = 1; k < frameSize.Width; k += GameWindowSearchStep.Width) {
							if (bitmap.GetPixel(left + k, top + frameSize.Height) != baseColor) {
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
						for (int k = 1; k < frameSize.Height; k += GameWindowSearchStep.Height) {
							if (bitmap.GetPixel(left + frameSize.Width, top + k) != baseColor) {
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
						for (int k = 0; k <= frameSize.Width; ++k) {
							if (bitmap.GetPixel(left + k, top) != baseColor) {
								flg = false;
								break;
							}
							if (bitmap.GetPixel(left + k, top + frameSize.Height) != baseColor) {
								flg = false;
								break;
							}
						}
						if (!flg)
							continue;
						for (int k = 0; k <= frameSize.Height; ++k) {
							if (bitmap.GetPixel(left, top + k) != baseColor) {
								flg = false;
								break;
							}
							if (bitmap.GetPixel(left + frameSize.Width, top + k) != baseColor) {
								flg = false;
								break;
							}
						}
						if (!flg)
							continue;
					}
					// 候補を追加
					// (virtualScreenLeft/Topは仮想画面の左上座標)
					int virtualScreenLeft = (int)SystemParameters.VirtualScreenLeft;
					int virtualScreenTop = (int)SystemParameters.VirtualScreenTop;
					rectList.Add(new Rectangle(virtualScreenLeft + left + 1, virtualScreenTop + top + 1, frameSize.Width - 1, frameSize.Height - 1));
					break;
				}
			}
			return rectList;
		}

		// ゲーム画面を表示しているエミュレーターのウィンドウハンドルを取得する
		public static void TryGetGameWindowHandle() {
			// 通常のスクショすら取得できない場合は無理
			if (!GameWindowRect.HasValue) {
				GameWindowHandle = null;
				return;
			}
			// 「中央の位置」を計算する
			int centerPointX = GameWindowRect.Value.X + GameWindowRect.Value.Width / 2;
			int centerPointY = GameWindowRect.Value.Y + GameWindowRect.Value.Height / 2;
			// そこからウィンドウハンドルを取得する
			var handle = NativeMethods.WindowFromPoint(
				new NativeMethods.POINT {
					x = centerPointX,
					y = centerPointY
				}
			);
			if(handle == IntPtr.Zero) {
				GameWindowHandle = null;
				return;
			}
			GameWindowHandle = handle;
			// 取得したハンドルを元に、ゲームウィンドウのrectを取得する
			var rect = new NativeMethods.RECT();
			if(NativeMethods.GetWindowRect(handle, ref rect) == 0){
				GameWindowHandle = null;
				return;
			}
			// ゲームウィンドウのrectからスクリーンショットを取得し、そこを基準にしたゲーム画面の位置を算出する
			// 1ピクセル大きめに取得するのは、GetWindowRectで取得したrectがゲーム画面そのものズバリである際、
			// そこにゲーム画面らしきものがあるとGetGameWindowPositionメソッドが判断できなくなるため
			var gameWindowBitmap = GetScreenBitmap(
				new Rectangle(rect.left - 1, rect.top - 1, rect.right - rect.left + 2, rect.bottom - rect.top + 2)
			);
			var gameRect = GetGameWindowPosition(gameWindowBitmap);
			if(gameRect.Count != 1) {
				GameWindowHandle = null;
				return;
			}
			//1ピクセル大きめに取得したので修正
			GameWindowRect = new Rectangle(gameRect[0].Left - 1, gameRect[0].Top - 1, gameRect[0].Width, gameRect[0].Height);
		}
	}
	internal static partial class NativeMethods
	{
		public const int SRCCOPY = 0xCC0020;
		public const int CAPTUREBLT = 0x40000000;
		[StructLayout(LayoutKind.Sequential)]
		public struct POINT {
			public int x;
			public int y;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT {
			public int left;
			public int top;
			public int right;
			public int bottom;
		}
		[DllImport("user32.dll")]
		public static extern IntPtr WindowFromPoint(POINT point);
		[DllImport("user32.dll")]
		public static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);
		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowDC(IntPtr hwnd);
		[DllImport("gdi32.dll")]
		public static extern int BitBlt(IntPtr hDestDC,
			int x,
			int y,
			int nWidth,
			int nHeight,
			IntPtr hSrcDC,
			int xSrc,
			int ySrc,
			int dwRop
		);
		[DllImport("user32.dll")]
		public static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);
	}
}
