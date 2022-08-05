function Invoke-ImageTransportTool{
    [CmdletBinding()]
    function(
        [Parameter]
        [switch]$Help
        [Parameter]
        [string]$Mode
    )

    Write-Output "Thanks for using the Docker Image Transport Tool."
    Write-Output "Use this tool to transport docker images via a drive instead of using an external registry service."
    Write-Output "First we'll check whether docker is installed correctly."
    Import-Module "./assure_install.ps1"
    Install-DockerVersion

    if(-not $PSBoundParameters.ContainsKey("Mode")){
        Write-Output "Please specify now how you intend to use the tool. `nChoose between [R]ead or [W]rite."
        $Mode = (Read-Host -Prompt "Run Mode")
    }
    
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