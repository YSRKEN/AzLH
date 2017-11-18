# AzurLane Helper II

The AzurLane Helper II by C#

# 概要

- 当ツールは、スマートフォン用オンラインゲーム『[アズールレーン](http://www.azurlane.jp)』のプレイを支援するものです
- パソコン上に、『アズールレーン』の画面が表示されている必要があります。別途Androidエミュレータ等をご用意ください
- 動作環境：.NET Framework 4.6.2以上がインストールされているWindows OS
- 開発環境：Microsoft Visual Studio Community 2017
- 動作確認OS：Windows 10(64bit)
- 動作確認エミュレータ：Nox App Player

## 警告

- 前述の動作確認環境以外での動作は確認していません
- 当ツールにゲーム画面を認識させるため、次の条件を確認してください
 - エミュレーター画面の縁・内部に他のウィンドウが掛かってない
 - エミュレーター画面の縁がディスプレイの境界上に掛かっていない
 - エミュレーターで表示するスマホの画面サイズが1280x720以上

## 使い方

　[help.md](./help/help.md) を参照してください。

## 注意書き

- このソフトで生じたいかなる損害についても責任を負いません
- このソフトは [MIT License](https://ja.osdn.net/projects/opensource/wiki/licenses%2FMIT_license) です

## 更新履歴

　[history.md](./help/history.md) を参照してください。

## 謝辞

　本ソフトウェアを作成するために様々な技術・ライブラリ・ソフトウェアを使用しました。特に以下の方々に感謝いたします。

- ゲーム画面の位置を検出する処理に、 [kanahiron](https://github.com/kanahiron/) さんが考案したルーチンを採用しました
- Readmeやヘルプファイルなどの表示に、[tatesuke](https://github.com/tatesuke) さんの「 [かんたんMarkdown](https://github.com/tatesuke/KanTanMarkdown) 」を使用しました

## 使用したライブラリ

- [Prism.Unity](https://www.nuget.org/packages/Prism.Unity/)
 - MVVMによるソフトウェア構築に使用
- [ReactiveProperty](https://www.nuget.org/packages/ReactiveProperty/4.0.0-pre4)
 - 同上
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)
 - JSONファイルの読み書きに使用
- [System.Data.SQLite.Core](https://www.nuget.org/packages/System.Data.SQLite.Core/)
 - 資材データベースの読み書きに使用
- [OpenCvSharp3-AnyCPU](https://www.nuget.org/packages/OpenCvSharp3-AnyCPU/)
 - 画像中の文字認識に使用
- [OxyPlot.Wpf](https://www.nuget.org/packages/OxyPlot.Wpf/)
 - 資材量のグラフ表示に使用
