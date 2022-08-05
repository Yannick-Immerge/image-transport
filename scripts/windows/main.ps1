Write-Output "Thanks for using the Docker Image Transport Tool."
Write-Output "Use this tool to transport docker images via a drive instead of using an external registry service."
Write-Output 'Please specify how the image should be loaded. `nChoose between [R]epository or [B]uild...'
$ImageFetchMode = (Read-Host -Prompt "Image Fetching  Mode").ToLower()
$ImageName = (Read-Host -Prompt "Image Name (without tag)")
$ImageTag = (Read-Host -Prompt "Image Tag")

if ($ImageFetchMode -eq "r"){
    $RepoUrl = (Read-Host -Prompt "Docker Repository URL")
    Get-ImageFromRepo -RepoUri $RepoUrl -QualifiedImageName ($ImageName + ":" + $ImageTag)
}
if ($ImageFetchMode -eq "b"){
    $BuildDirectory = (Read-Host -Prompt "Directory containing DOCKERFILE")
    Get-ImageFromBuild -BuildDirectory $BuildDirectory
}

Write-Output "Starting up local repository..."
Import-Module 

