<#
.SYNOPSIS
    Executes the Image Transportation Tool for Windows.

.DESCRIPTION
    The Image Transportation Tool is a tool that uses a self-contained docker environment 
    to create a local repository bound to local storage. This repo is then used to transport images from one context
    to another without relying on any external services. The tool should be executed on one machine
    that has access to the target image in write mode. When the image is transferred to the local volume 
    it can be mounted to the second machine. There the tool should be executed in read mode to move the image 
    from the local repository the new machine's docker context.

.PARAMETER Mode
    READ or WRITE depending on the mode the tool should be executed in.

.PARAMETER ImageName
    The full name of an image (including label) with the repository prefix that lives outside of the local repo.

.PARAMETER LocalName
    The full name of an image (including label) without the repository prefix that lives inside of the local repo.

.EXAMPLE
     Invoke-ImageTransportTool Write -ImageName myrepo/myimage:1.0 -LocalName myportableimage:1.0:

.EXAMPLE
    imagett read myportableimage:1.0 otherrepo/myimage:1.0

.INPUTS
    String

.NOTES
    Author:  Yannick Bergs
    Github: http://github.com/Yannick-Immerge
#>
function Invoke-ImageTransportTool{
    
    [CmdletBinding()]
    [Alias("imagett")]
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [string]$Mode,
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [string]$LocalName,
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [string]$ImageName,
        [Parameter(Mandatory=$false)]
        [string]$BuildPath
    )

    Write-Output "Thanks for using the Docker Image Transport Tool."
    Write-Output "Use this tool to transport docker images via a drive instead of using an external registry service."
    Write-Output "First we'll check whether docker is installed correctly."
    Import-Module "./assure_install.ps1"
    Install-DockerVersion

}
function Invoke-ImageWrite{
   

    Write-Output "Please specify how the image should be loaded. `nChoose between [R]epository or [B]uild..."
    $ImageFetchMode = (Read-Host -Prompt "Image Fetching  Mode").ToLower()
    $ImageName = (Read-Host -Prompt "Image Name (with repo, without label)")
    $ImageLabel = (Read-Host -Prompt "Image Label")

    if ($ImageFetchMode -eq "r"){
        $RepoUrl = (Read-Host -Prompt "Docker Repository URL")
        Get-ImageFromRepo -RepoUri $RepoUrl -QualifiedImageName ($ImageName + ":" + $ImageLabel)
    }
    if ($ImageFetchMode -eq "b"){
        $BuildDirectory = (Read-Host -Prompt "Directory containing DOCKERFILE")
        Get-ImageFromBuild -BuildDirectory $BuildDirectory
    }

    Write-Output "Starting up local repository..."
    Import-Module ./registry.ps1
    Start-LocalRegistry

    Write-Output "Pushing image to local repository..."
    Write-Output "Please specify the name (without repo) that should identify the image in the local repo..."
    $TargetIdentifier = (Read-Host -Prompt "New Image Identifier")
    Push-ImageToLocal -SourcedImageName ($ImageName + ":" + $ImageLabel) -TargetImageName ($ImageName + ":" + $ImageLabel)

    Write-Output "Shutting down local repository..."
}

function Invoke-ImageRead{

}