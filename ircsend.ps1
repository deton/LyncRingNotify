# ircsend.ps1
Param(
    [string]$from,
    [string]$server = "10.254.166.45",
    [int]$port = 6667,
    [string]$channel = "#projA",
    [string]$botnick = "[LyncDet]",
    [string]$targetnick = "deton",
    [switch]$im
)

# $from (tel:NNNN or mailto:MMMM)‚©‚ç–¼‘O‚Ö‚Ì•ÏŠ·•\
$tel2name = @{
	"tel:12345678" = "OSC";
}
$fromname = $tel2name[$from]

$ErrorActionPreference = "Stop"

$client = New-Object System.Net.Sockets.TcpClient($server, $port)
$stream = $client.GetStream()
$writer = New-Object IO.StreamWriter($stream, [Text.Encoding]::ASCII)

$writer.WriteLine("USER lyncring 0 * :LyncRingNotify")
$writer.WriteLine("NICK $botnick")
if ($im) {
    $writer.WriteLine("PRIVMSG $targetnick :@$targetnick IM from $fromname $from")
} else {
    $writer.WriteLine("JOIN $channel")
    $writer.WriteLine("PRIVMSG $channel :@$targetnick RING from $fromname $from")
}
$writer.WriteLine("QUIT")
$writer.Flush()
$writer.Close()
$stream.Close()
$client.Close()
