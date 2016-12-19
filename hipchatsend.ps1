# hipchatsend.ps1
Param(
    [string]$from,
    [string]$server = "hipchat.example.jp",
    [string]$room = "Default",
    [string]$targetnick = "deton",
    [string]$notificationtoken = "XXXX",
    [string]$messagetoken = "YYYY",
    [switch]$im
)

# $from (tel:NNNN or mailto:MMMM)Ç©ÇÁñºëOÇ÷ÇÃïœä∑ï\
$tel2name = @{
	"tel:12345678" = "OSC";
}
$fromname = $tel2name[$from]

$ErrorActionPreference = "Stop"

# é©å»èêñºÇÃèÿñæèëÇ≈Ç‡ãñâ¬
# http://qiita.com/nightyknite/items/b4db8766c0b94764cd3c
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

if ($im) {
    # send private one-on-one message
    $msg = "@$targetnick IM from $fromname $from"
    Invoke-RestMethod -Uri "https://$server/v2/user/@$targetnick/message?auth_token=$messagetoken" -Method POST -ContentType "application/json;charset=utf-8" -Body "{`"notify`":true,`"message`":`"$msg`"}"
} else {
    # send room notification
    $msg = "@$targetnick RING from $fromname $from"
    Invoke-RestMethod -Uri "https://$server/v2/room/$room/notification?auth_token=$notificationtoken" -Method POST -ContentType "application/json;charset=utf-8" -Body "{`"notify`":true,`"message`":`"$msg`"}"
}
