# ircsend.ps1
Param(
    [string]$msg
)

$addr = "10.254.166.45"
$port = 6668
$ErrorActionPreference = "Stop"

function interact($client) {
    $stream = $client.GetStream()
    $enc = New-Object System.Text.AsciiEncoding

    try {
        $data = $enc.GetBytes("USER lyncring b c d`nNICK [LyncDet]`nJOIN #projA`nPRIVMSG #projA :ring from " + $msg + "`nQUIT`n")
        $stream.Write($data, 0, $data.length)
    } finally {
        $stream.Close()
    }
}

$client = New-Object System.Net.Sockets.TcpClient ($addr, $port)
Write-Verbose "Connection to $($addr) $($port) port [tcp/*] succeeded!"
interact $client
$client.Close()
