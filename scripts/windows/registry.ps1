function Start-LocalRegistry {
    docker run -d -p 5000:5000 --restart=always --name "local-registry" -v "..\..\scripts\registry\data:/var/lib/registry" registry:2
}

function Stop-LocalRegistry {
    docker container stop --name "local-registry"
}