using System.Reflection;
using System.Text.RegularExpressions;
using System.IO.Compression;
using ImageTT.Parsing;

namespace ImageTT{
    public class ImageTT{
        private InstructionScheme _loadScheme;
        private InstructionScheme _pushScheme;
        private InstructionScheme _runScheme;
        private HttpClient _client;

        public ImageTT(){
            _loadScheme = new InstructionScheme(){
                {"(load)"},
                {"local-image:(l|local-image) -P"},
                {"host-image:(h|host-image) -P ?"},
                {"build-path:(build-path) -N ?"},
                {"build-image:(b|build) -S ?"},
            };
            _pushScheme = new InstructionScheme(){
                {"(push)"},
                {"local-image:(l|local-image) -P"},
                {"host-image:(h|host-image) -P"}
            };
            _runScheme = new InstructionScheme(){
                {"(run)"},
                {"local-image:(l|local-image) -P"},
                {"container-name:(n|container-name) -P ?"},
                {"register-daemon:(register-deamon) -S ?"},
            };

            _client = new HttpClient();
            //_registry = new LocalImageRegistry();
            //_registry.Serve();
        }

        public async Task InitializeAsync(){
            await CheckEnvironmentAsync();
        }

        private async Task CheckEnvironmentAsync(){
            // Acquire app root
            Assembly? entry = Assembly.GetEntryAssembly();
            if(entry == null)
                throw new InvalidOperationException("Please use the ImageTT tool as intended.");
            string assembly = entry.Location;

            // Check relative structure
            DirectoryInfo? root = Directory.GetParent(assembly);
            if(root == null)
                throw new DirectoryNotFoundException("Could not find root directory.");
            bool libFound = false;
            bool regFound = false;
            foreach(DirectoryInfo c in root.GetDirectories()){
                if(c.Name == "lib")
                    libFound = true;
                if(c.Name == "res")
                    regFound = true;
            }

            // Generate missing resources
            Task? generateLib = null;
            if(!libFound)
                generateLib = GenerateLibAsync(Path.Combine(root.FullName, "lib"));
            if(!regFound)
                GenerateReg(Path.Combine(root.FullName, "reg"));

            if(generateLib != null)
                await generateLib;
        }

        private async Task GenerateLibAsync(string path){
            // Create lib dir and install latest docker
            DirectoryInfo lib = Directory.CreateDirectory(path);
            string version = await GetLatestDockerVersionAsync();
            await InstallDockerVersionAsync(version, lib.FullName);
            ExpandDockerVersion(version, lib.FullName);
        }

        private void ExpandDockerVersion(string version, string libraryPath){
            ZipFile.ExtractToDirectory(Path.Combine(libraryPath, "docker.zip"), 
                                       Path.Combine(libraryPath, $"docker-{version}"));
        }

        private void GenerateReg(string path){
            Directory.CreateDirectory(path);   
        }

        private async Task<string> GetLatestDockerVersionAsync(){
            // Search index page for docker versions
            Task<string> msg = _client.GetStringAsync("https://download.docker.com/win/static/stable/x86_64");
            MatchCollection matches = Regex.Matches(await msg, "docker-([0-9]+.[0-9]+.[0-9]+)");
            
            //Find latest version
            int[] latest = new int[3] { 0, 0, 0 };
            foreach(Match m in matches){
                string[] vS = m.Groups[1].Value.Split('.');
                int[] v = new int[3];
                for(int i = 0; i < 3; i++)
                    v[i] = int.Parse(vS[i]);
                for(int i = 0; i < 3; i++){
                    if(v[i] == latest[i])
                        continue;
                    if(v[i] > latest[i])
                        latest = v;
                    break;
                }
            }

            //Return version
            return $"{latest[0]}.{latest[1]}.{latest[2]}";
        }

        private async Task InstallDockerVersionAsync(string version, string libraryPath){
            //Create file
            string dockerArchivePath = Path.Combine(libraryPath, "docker.zip");
            if(File.Exists(dockerArchivePath))
                File.Delete(dockerArchivePath);
            FileStream dockerArchiveStream = File.Create(dockerArchivePath);

            //Asynchronously fetch archive and write to file
            Stream remote = await _client.GetStreamAsync($"https://download.docker.com/win/static/stable/x86_64/docker-{version}.zip");
            Task copyLocal = remote.CopyToAsync(dockerArchiveStream);
            
            //Write to version file
            string versionPath = Path.Combine(libraryPath, "docker.version");
            if(File.Exists(versionPath))
                File.Delete(versionPath);
            StreamWriter write = File.CreateText(versionPath);
            write.WriteLine($"version={version}");
            write.Flush();
            write.Close();
            
            await copyLocal;
            dockerArchiveStream.Flush();
            dockerArchiveStream.Close();
            remote.Close();
        }
    }
}