# Lync着信バイブレーション

Microsoft Lync(Skype for Business)で電話がかかってきた時に、
バイブレーションで通知するモノです。

![写真](../img/usbvibration.jpg)

(銀色の部分が振動モータ)

## 特徴
* USB接続バイブレーション用デバイス(USB接続振動モータ)
	* スマホやAndroidタブレットで使えるバイブレーション通知を、
	  PCでも使えるようになります。
* 通知用にbatファイルを実行するので、通知方法はカスタマイズ可能
	* USB接続振動モータで通知
	* USB接続LEDで通知。blink(1)等
	* CDトレイを開いて通知(eject)
	* IRC(Internet Relay Chat)に通知。
		* Lync用Windowsマシンとは別のLinuxマシンを使って作業していても、
		  Linux上IRCクライアントに通知が表示されるので
		  着信に気付きやすくなります。
		* この通知に反応して、Microsoft Exchangeサーバから
		  その人の予定を取得してチャンネルに発言する、
		  [予定通知ボット](https://github.com/deton/ExchangeAppointmentBot)
		  との連携も可能。

## 背景
会社の内線電話が、構内PHSからLyncになったのですが、
下記理由もあって着信音に気付かないことがあったので、
USB接続振動モータを動かすようにしてみました。

+ あまりスピーカ音量を大きくすると、何かの操作をした際に音が出て気になるので
  音量は小さめ
  (PHS時代はPCにはスピーカも接続してなくて普段はミュート状態。
  Lync電話の着信用に新たにスピーカを接続)
+ 近くの会議スペースの声が大きくて集中できないのでイヤホンをすると、
  今度は着信音に気付きにくくなる

Lync用ハンドセットにバイブレーション機能があればよかったのですが、
配られたUSBハンドセットには付いてないようだったので。

IMを受信した際も、別のLinuxマシンを使っていると多くの場合は気付かないので、
同様にバイブレーションを行うようにしてみました。
通話とは別の振動パターンにしています。

## 構成
* Lync着信検知プログラムLyncRingNotify.exe
	* 通知実行batファイルnotifier.bat
		* USB接続バイブレーション用デバイスusbvibration
		* IRC通知PowerShellスクリプト: ircsend.ps1
		* CDトレイ制御PowerShellスクリプト: eject.ps1

LyncRingNotify.exeが同じディレクトリにあるnotifier.batを呼んで、
notifier.batがCOM経由でusbvibrationデバイスを制御します。

notifier.batの第1引数は以下のいずれか。

* im: IM着信時
* tel: 電話着信時
* off: 通知オフ

第2引数は電話の場合の発信者。
`tel:xxxxxx`または`sip:taro@example.jp`

usbvibrationの制御は簡単なので直接notifier.batから実行していますが、
CDトレイ制御やIRC通知はPowerShellスクリプトを実行しています。

## セットアップ手順
* usbvibration
	* [A-Star 32U4 Microのドライバをインストール](https://www.pololu.com/docs/0J61/6.1)
* LyncRingNotify
	* Lyncをインストール・起動
	* .NET Framework 4 Client Profileが入っていなければインストール
	* LyncSdkRedist.msiを実行してLync SDKをセットアップ
	* notifier.batをエディタで開いてCOM変数の値を、
	  Windowsのコントロールパネル→デバイス マネージャのポートで確認した
	  usbvibrationのCOMの値に変更。
	* LyncRingNotify.exeを起動

## USB接続バイブレーション用デバイスusbvibration
blink(1)のようなUSB接続LEDはあるので
バイブレーションもあるといいかもと思って作成。

振動モータを使っています。
振動モータ用回路は、
[『Prototyping Lab――「作りながら考える」ためのArduino実践レシピ』](http://www.oreilly.co.jp/books/9784873114538/)
にあるものそのままです。

バイブレーションを止めるためのタクトスイッチを付けました。
(LyncRingNotifyプログラム側から停止されるはずですが、念のため)

COM6等に対して、v512. 等の文字列を書き込むことで制御します。
(notifier.batから実行)

[Timer3](https://www.pjrc.com/teensy/td_libs_TimerOne.html)
でのPWMで振動モータを動かしています。
duty(0-1023)とperiod[ms]を指定可能です。
デフォルトはduty=512, period=900ms。
period指定は省略可能。

### 部品
![内部写真](../img/usbvibration-inside.jpg)

+ [Pololu A-Star 32U4 Micro](https://www.switch-science.com/catalog/1748/)(Arduino互換機)
+ [振動モータ](http://www.sengoku.co.jp/mod/sgk_cart/detail.php?code=EEHD-4HSR)
+ トランジスタ KSC1815
+ 整流用ダイオード 11EQS06
+ 抵抗 2.2kΩ

+ [プラケース [F52X22X13B]](http://www.aitendo.com/product/5186)。
  ケースに開いている穴に上の振動モータがぴったりはまります
  (少しカッターで広げないと入らないくらい)。
+ はさみで切れるユニバーサル基板
+ タクトスイッチ(2本足) 1個

## Lync着信検知プログラムLyncRingNotify
Lync SDKを使っています。
使うためには、Lyncクライアントが動作している必要があります。

## IRC通知PowerShellスクリプト: ircsend.ps1
IRCサーバに接続されるまで時間がかかる場合は、
ファイアウォールで113ポート(identd)を許可してみてください。
identdのタイムアウト待ちで時間がかかっている可能性があるので。

## CDトレイ制御PowerShellスクリプト: eject.ps1
以下のスクリプトが使えます。
ただし、Close動作は一部修正要。
EjectMedia()でなくCloseTray()にする必要があります。

[Eject/Close CD/DVD drive using PowerShell](https://gallery.technet.microsoft.com/scriptcenter/EjectClose-CDDVD-drive-56d39361)

## 他の通知方法案
* ポート毎給電制御可能なUSBハブを制御して、
  USB扇風機を動かしたりUSBデスクライトを点灯する

## 参考
* [PHS着信時電波を検出してIRC通知](https://github.com/deton/phsringnotify)
* PC用LED。[PresenceStick](https://github.com/deton/presencestick)、
  [blink(1)](http://blink1.thingm.com/)、
  [BlinkStick](http://www.blinkstick.com/)、
  [Luxafor](http://internet.watch.impress.co.jp/docs/yajiuma/20150123_684991.html)等
* Lyncのプレゼンス状態等をUSB接続LEDに表示するものは多数あり。
  [Busylight](http://www.link-corp.co.jp/busylight/)、
  [Blynclight](http://www.blynclight.com/)、
  [LyncFellow](http://glueckkanja.github.io/LyncFellow/)、
  [beakn](https://github.com/jonbgallant/beakn)、
  [LyncBlink](https://github.com/benbong/LyncBlink)等
