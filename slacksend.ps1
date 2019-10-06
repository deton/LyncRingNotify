# slacksend.ps1
Param(
    [string]$from,
    [string]$url = "https://slack.com/api/chat.postMessage",
    [string]$room = "#general",
    [string]$targetnick = "deton",
    [string]$proxy = "http://proxy.example.jp:3128/",
    [string]$token = "xoxp-XXXXXX",
    [switch]$im
)

# $from (tel:NNNN or sip:MMMM)‚©‚ç–¼‘O‚Ö‚Ì•ÏŠ·•\
$tel2name = @{
    "tel:12345678" = "OSC";
    "sip:taro@example.jp" = "‘¾˜Y";
}
$fromname = $tel2name[$from]

Add-Type -AssemblyName System.Web

$ErrorActionPreference = "Stop"

function convert_text($s) {
    $enc = [System.Text.Encoding]::GetEncoding('UTF-8')
    $utf8Bytes = [System.Text.Encoding]::UTF8.GetBytes($s)
    return $enc.GetString($utf8Bytes)
}

if ($im) {
    # private one-on-one message
    $msg = "@$targetnick IM from $fromname $from"
    $channel = "@$targetnick"
} else {
    # room notification
    $msg = "@$targetnick RING from $fromname $from"
    $channel = "@$targetnick"
    #$channel = convert_text($room)
}
$cmsg = convert_text($msg)
$text = [System.Web.HttpUtility]::UrlEncode($cmsg)
$chan = [System.Web.HttpUtility]::UrlEncode($channel)
Invoke-RestMethod -Proxy $proxy -Uri "https://slack.com/api/chat.postMessage?token=$token&channel=$chan&text=$text" -Method GET
