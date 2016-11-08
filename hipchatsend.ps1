# hipchatsend.ps1
Param(
    [string]$from,
    [string]$server = "hipchat.example.jp",
    [string]$room = "Default",
    [string]$targetnick = "deton",
    [string]$apitoken = "XXXX",
    [switch]$im
)

$ErrorActionPreference = "Stop"

if ($im) {
    $msg = "@$targetnick IM from $from"
} else {
    $msg = "@$targetnick RING from $from"
}

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

Invoke-RestMethod -Uri "https://$server/v2/room/$room/notification?auth_token=$apitoken" -Method POST -ContentType application/json -Body "{`"notify`":true,`"message`":`"$msg`"}"
