namespace ImageTT.Parsing
{
    public struct ParsedInstruction{
        public string[] Command { get; set; }
        public Dictionary<string, string?> Parameters { get; set; } 
        public Dictionary<string, bool> Switches { get; set; } 
        public InstructionScheme Scheme { get; set; }
    }
}