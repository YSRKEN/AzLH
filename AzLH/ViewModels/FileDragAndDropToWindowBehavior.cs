using AzLH.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Interactivity;

namespace AzLH.ViewModels
{
	class FileDragAndDropToWindowBehavior : Behavior<Window> {
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.DragOver += DragOver;
			AssociatedObject.Drop += Drop;
		}
		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.DragOver -= DragOver;
			AssociatedObject.Drop -= Drop;
		}
		void DragOver(object sender, DragEventArgs e)
		{
			e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
				? DragDropEffects.All
				: DragDropEffects.None;
		}
		void Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] fileNameList = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach(string fileName in fileNameList)
				{
					try
					{
						var bitmap = new Bitmap(fileName);
						string scene = SceneRecognition.JudgeGameScene(bitmap);
						var supplyValueDic = new Dictionary<string, int>();
						string otherMessage = "";
						var settings = SettingsStore.Instance;
						switch (scene)
						{
							case "母港":
								supplyValueDic["燃料"] = CharacterRecognition.GetValueOCR(bitmap, "燃料", settings.PutCharacterRecognitionFlg);
								supplyValueDic["資金"] = CharacterRecognition.GetValueOCR(bitmap, "資金", settings.PutCharacterRecognitionFlg);
								supplyValueDic["ダイヤ"] = CharacterRecognition.GetValueOCR(bitmap, "ダイヤ", settings.PutCharacterRecognitionFlg);
								break;
							case "建造":
								supplyValueDic["キューブ"] = CharacterRecognition.GetValueOCR(bitmap, "キューブ", settings.PutCharacterRecognitionFlg);
								break;
							case "建造中":
								supplyValueDic["ドリル"] = CharacterRecognition.GetValueOCR(bitmap, "ドリル", settings.PutCharacterRecognitionFlg);
								break;
							case "支援":
								supplyValueDic["勲章"] = CharacterRecognition.GetValueOCR(bitmap, "勲章", settings.PutCharacterRecognitionFlg);
								break;
							case "家具屋":
								supplyValueDic["家具コイン"] = CharacterRecognition.GetValueOCR(bitmap, "家具コイン", settings.PutCharacterRecognitionFlg);
								break;
							case "戦闘中": {
								var gauge = SceneRecognition.GetBattleBombGauge(bitmap);
								otherMessage += "読み取ったゲージ量：\n";
								otherMessage += $"　空撃→{Math.Round(gauge[0] * 100.0, 1)}％\n";
								otherMessage += $"　雷撃→{Math.Round(gauge[1] * 100.0, 1)}％\n";
								otherMessage += $"　砲撃→{Math.Round(gauge[2] * 100.0, 1)}％\n";
							}
							break;
						}
						string output = $"シーン判定結果：{scene}\n読み取った資材量：\n";
						foreach(var pair in supplyValueDic){
							output += $"　{pair.Key}→{pair.Value}\n";
						}
						output += otherMessage;
						MessageBox.Show(output, Utility.SoftwareName, MessageBoxButton.OK, MessageBoxImage.Information);
						
					}
					catch
					{

					}
				}
			}
		}
	}
}
