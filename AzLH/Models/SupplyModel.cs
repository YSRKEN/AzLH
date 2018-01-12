using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Windows;
using OxyPlot;
using System;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace AzLH.Models {
	class SupplyModel : BindableBase {
		// グラフデータ
		private PlotModel supplyGraphModel;
		public PlotModel SupplyGraphModel {
			get => supplyGraphModel;
			set {
				SetProperty(ref supplyGraphModel, value);
			}
		}

		// コンストラクタ
		public SupplyModel() {
			SetSettings();
			RedrawSupplyGraph();
		}

		// 設定内容を画面に反映する
		private void SetSettings() {
			if (SettingsStore.MemoryWindowPositionFlg) {
				WindowPositionLeft = SettingsStore.SupplyWindowRect[0];
				WindowPositionTop = SettingsStore.SupplyWindowRect[1];
				WindowPositionWidth = SettingsStore.SupplyWindowRect[2];
				WindowPositionHeight = SettingsStore.SupplyWindowRect[3];
			}
			AutoOpenWindowFlg = SettingsStore.AutoSupplyWindowFlg;
		}
		// 表示する資材のモードを切り替える
		public void ChangeSupplyMode() {
			if (showSupplyMode == 0)
				showSupplyMode = 1;
			else
				showSupplyMode = 0;
			RaisePropertyChanged(nameof(SupplyModeButtonColor));
			RaisePropertyChanged(nameof(SupplyModeButtonContent));
			RedrawSupplyGraph();
		}
		// グラフ画像を保存する
		public void SaveSupplyGraph() {
			var pngExporter = new OxyPlot.Wpf.PngExporter {
				Width = (int)SupplyGraphModel.PlotArea.Width,
				Height = (int)SupplyGraphModel.PlotArea.Height,
				Background = OxyColors.White
			};
			var bitmapSource = pngExporter.ExportToBitmap(SupplyGraphModel);
			// BitmapSourceを保存する
			var sfd = new SaveFileDialog {
				// ファイルの種類を設定
				Filter = "画像ファイル(*.png)|*.png|全てのファイル (*.*)|*.*"
			};
			// ダイアログを表示
			if ((bool)sfd.ShowDialog()) {
				using (var stream = new FileStream(sfd.FileName, FileMode.Create)) {
					var encoder = new PngBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
					encoder.Save(stream);
				}
			}
		}
		// グラフを再描画する
		// 参考→https://qiita.com/maruh/items/035a39a2a01102ce248f
		public void RedrawSupplyGraph() {
			try {
				// Y軸および第二Y軸に表示するデータを集めておく
				// Y軸→「表示する資材のモードと同じ」かつ「第二Y軸ではない」資材名
				// 第二Y軸→「表示する資材のモードと同じ」かつ「第二Y軸である」資材名
				var AxisYList = CharacterRecognition.SupplyParameters
					.Where(p => p.Value.MainSupplyFlg == (showSupplyMode == 0) && !p.Value.SecondaryAxisFlg)
					.ToDictionary(p => p.Key, q => SupplyStore.GetSupplyData(q.Key));
				var AxisY2List = CharacterRecognition.SupplyParameters
					.Where(p => p.Value.MainSupplyFlg == (showSupplyMode == 0) && p.Value.SecondaryAxisFlg)
					.ToDictionary(p => p.Key, q => SupplyStore.GetSupplyData(q.Key));
				// グラフに必要な数値を算出する
				//横軸
				var maxYDateTime = AxisYList.Max(p => p.Value.Max(q => q.Key));
				var maxY2DateTime = AxisY2List.Max(p => p.Value.Max(q => q.Key));
				var maxDateTime = (maxYDateTime > maxY2DateTime ? maxYDateTime : maxY2DateTime);
				var minDateTime = maxDateTime.AddDays(-NowGraphPeriodInfo.Days);
				//縦軸
				int maxYValue = AxisYList.Max(p => p.Value.Where(q => minDateTime <= q.Key).Max(r => r.Value));
				SpecialCeiling(ref maxYValue, out int intervalY);
				string axisYStr = "";
				for (int i = 0; i < AxisYList.Count; ++i) {
					if (i != 0)
						axisYStr += "・";
					axisYStr += AxisYList.Keys.ToList()[i];
				}
				int maxY2Value = AxisY2List.Max(p => p.Value.Where(q => minDateTime <= q.Key).Max(r => r.Value));
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
					StringFormat = NowGraphPeriodInfo.StringFormat,
					MajorGridlineStyle = LineStyle.Solid,
					MajorGridlineColor = OxyColors.LightGray,
					MajorStep = NowGraphPeriodInfo.MajorStep,
					MinorStep = NowGraphPeriodInfo.MinorStep,
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
				SupplyGraphModel = newSupplyGraphModel;
			}
			catch {}
		}
		// グラフ表示に適した適当な数値に切り上げる
		private void SpecialCeiling(ref int maxValue, out int interval) {
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
	}
}
