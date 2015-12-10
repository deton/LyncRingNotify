# Lync着信バイブレーション

Microsoft Lync(Skype for Business)で電話がかかってきた時に、
バイブレーションで通知するモノです。

## 特徴
* 通知はbatファイル実行なので、通知方法はカスタマイズ可能。
 * USB接続振動モータ
 * USB接続LED。blink(1)等
 * CDトレイを開く(eject)
 * IRCに通知。別のLinuxマシンを使っていても、IRC通知に気付ける。
   この通知に反応して、Microsoft Exchangeサーバから予定を取得して
   チャンネルに発言する、
   [予定通知ボット](https://github.com/deton/ExchangeAppointmentBot)
   との連携も可能。
* USB接続バイブレーション用デバイス

## 背景
会社の内線電話が、構内PHSからLyncになったのですが、
着信音に気付かないことがあったので、
USB接続振動モータを動かすようにしてみました。

(あまりスピーカ音量を大きくすると、何かの操作をした際に音が出て気になるので
音量を小さめにしているのも、気付きにくい一因)

Lync用ハンドセットにバイブレーション機能があればよかったのですが、
配られたUSBハンドセットには付いてないようだったので。

IMを受信した際も、別のLinuxマシンを使っていると多くの場合は気付かないので、
同様にバイブレーションを行うようにしてみました。
通話とは別の振動パターンにしています。

## 構成
* Lync着信検知プログラムLyncRingNotify
	* 通知実行batファイルnotifier.bat
		* USB接続バイブレーション用デバイスusbvibration
		* CDトレイ制御PowerShell script: [eject.ps1](https://gallery.technet.microsoft.com/scriptcenter/EjectClose-CDDVD-drive-56d39361)
		* IRC通知PowerShell script: ircsend.ps1

LyncRingNotifyがLyncRingNotifyと同じディレクトリにあるnotifier.batを呼んで、
notifier.batがusbvibrationデバイスを制御します。

notifier.batの第1引数は以下のいずれか。

* im: IM着信時
* tel: 電話着信時
* off: 通知オフ

第2引数は電話の場合の発信者。
tel:xxxxxxまたはsip:taro@example.jp

usbvibrationの制御は簡単なので直接notifier.batから実行していますが、
CDトレイ制御やIRC通知はPowerShell scriptを実行しています。

## Lync着信検知プログラムLyncRingNotify
Lync SDKを使っています。

Lyncの着信検知は、
Google検索やgithub検索で見つかるコードを参考にして作っています。

## USB接続バイブレーション用デバイスusbvibration
スマホやAndroidタブレットには普通に内蔵されているので
PC用にもあるといいかもと思って作成。

blink(1)のようなUSB接続LEDはあるのでバイブレーションもあるといいかもと思って作成。

振動モータを使っています。
振動モータ用回路は、
[『Prototyping Lab――「作りながら考える」ためのArduino実践レシピ』](http://www.oreilly.co.jp/books/9784873114538/)
にあるものそのままです。

バイブレーションを止めるためのタクトスイッチを(後から思い付いて)付けました。
(LyncRingNotifyプログラム側から停止されるはずですが、念のため)

COM6等に対して、v512. 等の文字列を書き込むことで制御します。
(notifier.batから実行)

[Timer3](https://www.pjrc.com/teensy/td_libs_TimerOne.html)
でのPWMで振動モータを動かしています。
duty(0-1023)とperiod[ms]を指定可能です。
デフォルトはduty=512, period=900ms。
period指定は省略可能。

### 部品
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

## IRC通知PowerShell script: ircsend.ps1
IRCサーバに接続されるまで時間がかかる場合は、
ファイアウォールで113ポート(identd)を許可してみてください。
identdのタイムアウト待ちで時間がかかっている可能性があるので。

## CDトレイ制御PowerShell script: eject.ps1
以下のスクリプトが使えます。
ただし、Close動作は一部修正要。
EjectMedia()でなくCloseTray()にする必要があります。

https://gallery.technet.microsoft.com/scriptcenter/EjectClose-CDDVD-drive-56d39361

## 参考
* [PHS着信時電波を検出してIRC通知](https://github.com/deton/phsringnotify)
* PC用LED。[PresenceStick](https://github.com/deton/presencestick)、
  blink(1)、
  [BlinkStick](http://www.blinkstick.com/)、
  [Luxafor](http://internet.watch.impress.co.jp/docs/yajiuma/20150123_684991.html)等
* Lyncのプレゼンス状態等をUSB接続LED等に表示するものはgithubをLyncで検索するといろいろあり
