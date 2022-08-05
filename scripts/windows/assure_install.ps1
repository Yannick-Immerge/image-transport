function Install-DockerVersion{
    # Check for portable docker installation
    if (-not (Test-Path ".\docker.version")){ Out-File -InputObject "version: none" -FilePath ".\docker.version" }
    if (-not (Test-Path ".\lib")){ New-Item ".\lib" -ItemType Directory }

    # Establish installation
    $lastaction = "none"
    $action = "undetermined"
    while (($action -ne "none") -and ($action -ne $lastaction)) {
        
        # Perform current action
        Write-Output ("Performing action: " + $action)
        if ($action -eq "download-latest"){
            $v = Get-LatestVersion
            Out-File -FilePath ".\docker.version" -InputObject ("version: " + $v)
            Get-DockerVersion -Version $v
        }
        elseif ($action -eq "download-target") {
            Get-DockerVersion -Version $TargetVersion
        }
        elseif ($action -eq "expand") {
            Expand-Archive -Path (".\lib\docker-" + $TargetVersion + ".zip") -DestinationPath (".\lib\docker-" + $TargetVersion)
        }

        # Determine next action
        $lastaction = $action
        $m = (Select-String -Path ".\docker.version" -Pattern "version:\s*([0-9]+.[0-9]+.[0-9]+)").Matches
        if ($m.Count -eq 0) { 
            $action = "download-latest"
            continue 
        }
        $TargetVersion = $m.Groups[1].Value
        if (-not (Test-Path (".\lib\docker-" + $TargetVersion + ".zip"))){ 
            $action = "download-target"
            continue
        }
        if (-not (Test-Path (".\lib\docker-" + $TargetVersion))){
            $action = "expand"
            continue
        }
        $action = "none"
    }
}

function Get-LatestVersion {
    $Response = Invoke-WebRequest -URI https://download.docker.com/win/static/stable/x86_64/
    $Found = Select-String -InputObject $Response.Content -Pattern 'docker-([0-9]+).([0-9]+).([0-9]+).zip' -AllMatches
    $Current = @(0, 0, 0)
    for ($i = 0; $i -lt $Found.Matches.Count; $i++) {
        [bool]$update = $false
        [int]$v = $Found.Matches[$i].Groups[1].Value
        if ($v -lt $Current[0]){ continue }
        elseif ($v -gt $Current[0]){ $update = $true }
        [int]$p = $Found.Matches[$i].Groups[2].Value
        if ($update){ }
        elseif ($p -lt $Current[1]){ continue }
        elseif ($p -gt $Current[1]){ $update = $true }
        [int]$b = $Found.Matches[$i].Groups[3].Value
        if ($update){ }
        elseif ($b -le $Current[2]){ continue }
        elseif ($b -gt $Current[2]){ $update = $true }
        if ($update){ $Current = @($v, $p, $b) }
    }
    ("" + $Current[0] + "." + $Current[1] + "." + $Current[2])
}

function Get-DockerVersion {
    [CmdletBinding()]
    param(
        $Version
    )
    
    Write-Output ("Downloading docker version: " + $Version)
    Invoke-WebRequest -OutFile (".\lib\docker-" + $Version + ".zip") -Uri ("https://download.docker.com/win/static/stable/x86_64/docker-" + $Version + ".zip")

}