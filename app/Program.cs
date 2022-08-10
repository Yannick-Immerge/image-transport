using ImageTT.Parsing;
using System.Reflection;

namespace ImageTT{

    public enum ImageTTCommand{
        LOAD,
        PUSH
    }

    public enum ImageFetchMode{
        BUILD,
        REGISTRY
    }

    public struct ImageTTSpecification{
        public ImageTTCommand Command { get; set; }
        public string? HostImageName { get; set; }
        public string? HostImagePath { get; set; }
        public string LocalImageName { get; set; }
        public ImageFetchMode? FetchMode { get; set; }
        public bool Help { get; set; }
        public int Verbosity { get; set; }

        
    }

    public class Program{

        /// <summary>
        /// Starts the Image Transportation Tool with the given parameters.<br>
        /// [Command] [-Options] snp
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args){
            Console.WriteLine("Running ImageTT..."); 
            ImageTT tool = new ImageTT();
            await tool.InitializeAsync();
            Console.WriteLine("...");
        }
    }
}