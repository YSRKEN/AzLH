using AzLH.Models;
using OxyPlot;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Timers;
using System.Windows.Media;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace AzLH.ViewModels {
	// 表示する期間の情報
	public class GraphPeriodInfo
	{
		public double Days { get; set; }
		public string StringFormat { get; set; }
		public double MajorStep { get; set; }
		public double MinorStep { get; set; }
		public GraphPeriodInfo(double days, string stringFormat, double majorStep, double minorStep) {
			Days = days;
			StringFormat = stringFormat;
			MajorStep = majorStep;
			MinorStep = minorStep;
		}
	}

	internal class SupplyViewModel : IDisposable, INotifyPropertyChanged
	{
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		public event PropertyChangedEventHandler PropertyChanged;
		// 表示する期間の一覧(Keyが名称、Valueが期間(単位は"日"))
		private readonly Dictionary<string, GraphPeriodInfo> graphPeriodDic
			= new Dictionary<string, GraphPeriodInfo> {
			{ "6時間",  new GraphPeriodInfo(0.25, "HH:mm"   , 1.0 / 24 , 0.5 / 24)},
			{ "12時間", new GraphPeriodInfo(0.5 , "HH:mm"   , 2.0 / 24 , 1.0 / 24)},
			{ "1日",    new GraphPeriodInfo(1.0 , "HH:mm"   , 4.0 / 24 , 2.0 / 24)},
			{ "3日",    new GraphPeriodInfo(3.0, "MM/dd" , 12.0 / 24, 6.0 / 24)},
			{ "1週間",  new GraphPeriodInfo(7.0, "MM/dd" , 1.0      , 12.0 / 24)},
			{ "1ヶ月", new GraphPeriodInfo(30.0, "MM/dd" , 3.0      , 1.0)},
			{ "3ヶ月", new GraphPeriodInfo(90.0, "MM/dd" , 10.0     , 5.0)},
			{ "6ヶ月", new GraphPeriodInfo(180.0, "MM/dd", 30.0     , 15.0)},
			{ "1年", new GraphPeriodInfo(365, "MM/dd"    , 60.0     , 30.0)},
		};
		// 現在の表示する期間
		private GraphPeriodInfo NowGraphPeriodInfo
			=> graphPeriodDic[GraphPeriodList.Value[GraphPeriodIndex.Value]];
		// 表示する資材のモード
		private ReactiveProperty<int> ShowSupplyMode { get; } = new ReactiveProperty<int>(0);

		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; } = new ReactiveProperty<bool>(false);
		// 画面の位置
		public ReactiveProperty<double> WindowPositionLeft { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> WindowPositionTop { get; } = new ReactiveProperty<double>(double.NaN);
		public ReactiveProperty<double> WindowPositionWidth { get; } = new ReactiveProperty<double>(600.0);
		public ReactiveProperty<double> WindowPositionHeight { get; } = new ReactiveProperty<double>(400.0);
		// 起動時にこの画面を表示するか？
		public ReactiveProperty<bool> AutoOpenWindowFlg { get; } = new ReactiveProperty<bool>(false);
		// 表示する期間
		public ReactiveProperty<int> GraphPeriodIndex { get; } = new ReactiveProperty<int>(2);
		// 表示する期間の一覧
		public ReactiveProperty<List<string>> GraphPeriodList { get; }
		// 表示する資材のモードに対応してボタンの色・表示内容が変わる
		public ReadOnlyReactiveProperty<Brush> SupplyModeButtonColor { get; }
		public ReadOnlyReactiveProperty<string> SupplyModeButtonContent { get; }
		// グラフデータ
		public ReactiveProperty<PlotModel> SupplyGraphModel { get; } = new ReactiveProperty<PlotModel>();

		// 表示する資材のモードを切り替える
		public ReactiveCommand ChangeSupplyModeCommand { get; } = new ReactiveCommand();
		// グラフ画像を保存する
		public ReactiveCommand SaveSupplyGraphCommand { get; } = new ReactiveCommand();

		// グラフを再描画する
		private PlotModel RedrawSupplyGraph() {
			return SupplyModel.RedrawSupplyGraph(ShowSupplyMode.Value, NowGraphPeriodInfo);
		}
		// コンストラクタ
		public SupplyViewModel() {
			// 設定ファイルに記録していた情報を書き戻す
			{
				if (SettingsStore.MemoryWindowPositionFlg) {
					WindowPositionLeft.Value = SettingsStore.SupplyWindowRect[0];
					WindowPositionTop.Value = SettingsStore.SupplyWindowRect[1];
					WindowPositionWidth.Value = SettingsStore.SupplyWindowRect[2];
					WindowPositionHeight.Value = SettingsStore.SupplyWindowRect[3];
				}
				AutoOpenWindowFlg.Value = SettingsStore.AutoSupplyWindowFlg;
			}
			// 画面の位置が変更された際、自動で設定ファイルに書き戻すようにする
			WindowPositionLeft.Subscribe(x => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.SupplyWindowRect[0] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			WindowPositionTop.Subscribe(x => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.SupplyWindowRect[1] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			WindowPositionWidth.Subscribe(x => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.SupplyWindowRect[2] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			WindowPositionHeight.Subscribe(x => {
				if (!SettingsStore.MemoryWindowPositionFlg)
					return;
				SettingsStore.SupplyWindowRect[3] = x;
				SettingsStore.ChangeSettingFlg = true;
			});
			// 起動時にこの画面を表示するか？
			AutoOpenWindowFlg.Subscribe(value => {
				SettingsStore.AutoSupplyWindowFlg = value;
			});
			// 表示する資材のモードに対応してボタンの色・表示内容を変える
			SupplyModeButtonColor = ShowSupplyMode.Select(x => {
				return (Brush)(x == 0 ? new SolidColorBrush(Colors.Pink) : new SolidColorBrush(Colors.SkyBlue));
			}).ToReadOnlyReactiveProperty();
			SupplyModeButtonContent = ShowSupplyMode.Select(x => {
				return (x == 0 ? "通常資材" : "特殊資材");
			}).ToReadOnlyReactiveProperty();
			// その他staticな初期化
			GraphPeriodList = new ReactiveProperty<List<string>>(graphPeriodDic.Keys.Select(p => p).ToList());
			// 表示期間・資材モードが変更された際、グラフを再描画する
			SupplyGraphModel = GraphPeriodIndex.CombineLatest(ShowSupplyMode, (_, mode) => RedrawSupplyGraph()).ToReactiveProperty();
			// コマンドを設定
			ChangeSupplyModeCommand.Subscribe(_ => {
				ShowSupplyMode.Value = (ShowSupplyMode.Value == 0 ? 1 : 0);
				RedrawSupplyGraph();
			});
			SaveSupplyGraphCommand.Subscribe(_ => SupplyModel.SaveSupplyGraph(SupplyGraphModel.Value));
			// タイマーを指定してグラフ更新を定期実行する
			var timer = new Timer(1000 * 60 * 5);
			timer.Elapsed += (sender, e) => {
				try {
					timer.Stop();
					RedrawSupplyGraph();
				}
				finally { timer.Start(); }
			};
			timer.Start();
			// まず最初の画面更新を掛ける
			SupplyGraphModel.Value = RedrawSupplyGraph();
		}

		// Dispose処理
		public void Dispose() => Disposable.Dispose();
	}
	static class SupplyModel
	{
		// グラフ表示に適した適当な数値に切り上げる
		private static void SpecialCeiling(ref int maxValue, out int interval) {
			// とりあえず10で割っていく
			double x_ = maxValue;
			double temp = 1.0;
			while (x_ >= 10.0) {
				x_ /= 10.0;
				temp *= 10.0;
			}
			// 切り上げ処理
			x_ = Math.Ceiling(x_);
			maxValue = (int)(x_ * temp);
			interval = (int)temp;
		}
		// グラフを再描画する
		// 参考→https://qiita.com/maruh/items/035a39a2a01102ce248f
		public static PlotModel RedrawSupplyGraph(int showSupplyMode, GraphPeriodInfo graphPeriodInfo) {
			try {
				// Y軸および第二Y軸に表示するデータを集めておく
				// Y軸→「表示する資材のモードと同じ」かつ「第二Y軸ではない」資材名
				// 第二Y軸→「表示する資材のモードと同じ」かつ「第二Y軸である」資材名
				var AxisYList = CharacterRecognition.SupplyParameters
					.Where(p => p.Value.MainSupplyFlg == (showSupplyMode == 0)
						&& !p.Value.SecondaryAxisFlg)
					.ToDictionary(p => p.Key, q => SupplyStore.GetSupplyData(q.Key));
				var AxisY2List = CharacterRecognition.SupplyParameters
					.Where(p => p.Value.MainSupplyFlg == (showSupplyMode == 0)
						&& p.Value.SecondaryAxisFlg)
					.ToDictionary(p => p.Key, q => SupplyStore.GetSupplyData(q.Key));
				// グラフに必要な数値を算出する
				//横軸
				var maxYDateTime = AxisYList.Max(p => p.Value.Max(q => q.Key));
				var maxY2DateTime = AxisY2List.Max(p => p.Value.Max(q => q.Key));
				var maxDateTime = (maxYDateTime > maxY2DateTime ? maxYDateTime : maxY2DateTime);
				var minDateTime = maxDateTime.AddDays(-graphPeriodInfo.Days);
				//縦軸
				int maxYValue = AxisYList
					.Max(p => p.Value.Where(q => minDateTime <= q.Key)
					.Max(r => r.Value));
				SpecialCeiling(ref maxYValue, out int intervalY);
				string axisYStr = "";
				for (int i = 0; i < AxisYList.Count; ++i) {
					if (i != 0)
						axisYStr += "・";
					axisYStr += AxisYList.Keys.ToList()[i];
				}
				int maxY2Value = AxisY2List
					.Max(p => p.Value.Where(q => minDateTime <= q.Key)
					.Max(r => r.Value));
				SpecialCeiling(ref maxY2Value, out int intervalY2);
				string axis2YStr = "";
				for (int i = 0; i < AxisY2List.Count; ++i) {
					if (i != 0)
						axisYStr += "・";
					axis2YStr += AxisY2List.Keys.ToList()[i];
				}
				// グラフ要素を構築する
				//軸
				var newSupplyGraphModel = new PlotModel();
				newSupplyGraphModel.Axes.Add(new DateTimeAxis {
					Position = AxisPosition.Bottom,
					Minimum = DateTimeAxis.ToDouble(minDateTime),
					Maximum = DateTimeAxis.ToDouble(maxDateTime),
					StringFormat = graphPeriodInfo.StringFormat,
					MajorGridlineStyle = LineStyle.Solid,
					MajorGridlineColor = OxyColors.LightGray,
					MajorStep = graphPeriodInfo.MajorStep,
					MinorStep = graphPeriodInfo.MinorStep,
					Title = "日時"
				});
				newSupplyGraphModel.Axes.Add(new LinearAxis {
					Position = AxisPosition.Left,
					Minimum = 0.0,
					Maximum = maxYValue,
					MajorGridlineStyle = LineStyle.Solid,
					MajorGridlineColor = OxyColors.LightGray,
					MajorStep = intervalY,
					MinorTickSize = 0,
					Title = axisYStr,
					Key = "Primary"
				});
				newSupplyGraphModel.Axes.Add(new LinearAxis {
					Position = AxisPosition.Right,
					Minimum = 0.0,
					Maximum = maxY2Value,
					MajorGridlineStyle = LineStyle.Solid,
					MajorGridlineColor = OxyColors.LightGray,
					MajorStep = intervalY2,
					MinorTickSize = 0,
					Title = axis2YStr,
					Key = "Secondary"
				});
				//グラフ値
				foreach (var plotInfo in AxisYList) {
					var lineSeries = new LineSeries();
					foreach (var plotData in plotInfo.Value) {
						lineSeries.Points.Add(new DataPoint(
							DateTimeAxis.ToDouble(plotData.Key),
							plotData.Value
						));
					}
					lineSeries.YAxisKey = "Primary";
					lineSeries.Title = plotInfo.Key;
					lineSeries.Color = OxyColor.FromRgb(
						CharacterRecognition.SupplyParameters[plotInfo.Key].Color.R,
						CharacterRecognition.SupplyParameters[plotInfo.Key].Color.G,
						CharacterRecognition.SupplyParameters[plotInfo.Key].Color.B
					);
					newSupplyGraphModel.Series.Add(lineSeries);
				}
				foreach (var plotInfo in AxisY2List) {
					var lineSeries = new LineSeries();
					foreach (var plotData in plotInfo.Value) {
						lineSeries.Points.Add(new DataPoint(
							DateTimeAxis.ToDouble(plotData.Key),
							plotData.Value
						));
					}
					lineSeries.YAxisKey = "Secondary";
					lineSeries.Title = plotInfo.Key;
					lineSeries.Color = OxyColor.FromRgb(
						CharacterRecognition.SupplyParameters[plotInfo.Key].Color.R,
						CharacterRecognition.SupplyParameters[plotInfo.Key].Color.G,
						CharacterRecognition.SupplyParameters[plotInfo.Key].Color.B
					);
					newSupplyGraphModel.Series.Add(lineSeries);
				}

				// グラフの要素を画面に反映する
				newSupplyGraphModel.InvalidatePlot(true);
				return newSupplyGraphModel;
			} catch {
				return null;
			}
		}
		// グラフ画像を保存する
		public static void SaveSupplyGraph(PlotModel plotModel) {
			var pngExporter = new OxyPlot.Wpf.PngExporter {
				Width = (int)plotModel.PlotArea.Width,
				Height = (int)plotModel.PlotArea.Height,
				Background = OxyColors.White
			};
			var bitmapSource = pngExporter.ExportToBitmap(plotModel);
			// BitmapSourceを保存する
			var sfd = new SaveFileDialog {
				// ファイルの種類を設定
				Filter = "画像ファイル(*.png)|*.png|全てのファイル (*.*)|*.*"
			};
			// ダイアログを表示
			if ((bool)sfd.ShowDialog()) {
				// 保存処理
				using (var stream = new FileStream(sfd.FileName, FileMode.Create)) {
					var encoder = new PngBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
					encoder.Save(stream);
				}
			}
		}
	}
}
