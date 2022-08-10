using System.Collections;
using System.Text.RegularExpressions;

namespace ImageTT.Parsing
{
    public enum ParameterType{
        SWITCH,
        NAMED,
        POSITIONAL,
        COMMAND
    }

    public struct InstructionSchemeEntry{
        public static readonly string INSTRUCTION_SCHEME_ENTRY_REGEX
            = @"\((([a-zA-Z0-9-_]+)?(\|[a-zA-Z0-9-_]+)*)\)\s*(-[a-zA-Z])?\s*\??";

        public string? Name { get; set; }
        public string[] Values { get; set; }
        public ParameterType Type { get; set; }
        public bool Optional { get; set; }

        /// <summary>
        /// Parses 
        /// </summary>
        /// <param name="shorthand"></param>
        /// <returns></returns>
        public static InstructionSchemeEntry ParseFromShorthand(string shorthand){
            
            // Check for name
            string[] split = shorthand.Split(':');
            string? name = null;
            string content = split[0];
            if(split.Length > 1){
                name = split[0];
                content = split[1];
            }

            // Match regex
            Match first = Regex.Match(content, INSTRUCTION_SCHEME_ENTRY_REGEX);
            if(first == null)
                throw new FormatException("The shorthand format was not formatted correctly.");

            // Parse parameters
            string[] values = new string[0];
            if(first.Groups[1].Value != "")
                values = first.Groups[1].Value.Split('|');
            ParameterType type = ParameterType.COMMAND;
            if(first.Groups.Count > 4 && first.Groups[4].Value != ""){
                switch(first.Groups[4].Value.ToLower()[1]){
                    case 's':
                        type = ParameterType.SWITCH;
                        break;
                    case 'n':
                        type = ParameterType.NAMED;
                        break;
                    case 'p':
                        type = ParameterType.POSITIONAL;
                        break;
                    case 'c':
                        type = ParameterType.COMMAND;
                        break;
                }
            }
            bool optional = first.Value.EndsWith('?');

            // Create and return entry
            return new InstructionSchemeEntry() { Name = name, Values = values, Type = type, Optional = optional };
        }
    }

    public class InstructionScheme : IEnumerable<InstructionSchemeEntry>{
        private List<InstructionSchemeEntry> _positional;
        private List<InstructionSchemeEntry> _command;
        private List<InstructionSchemeEntry> _switch;
        private List<InstructionSchemeEntry> _complete;

        public InstructionScheme(){
            _positional = new List<InstructionSchemeEntry>();
            _command = new List<InstructionSchemeEntry>();
            _complete = new List<InstructionSchemeEntry>();
            _switch = new List<InstructionSchemeEntry>();
        }

        public void Add(InstructionSchemeEntry entry){
            _complete.Add(entry);
            if(entry.Type == ParameterType.COMMAND)
                _command.Add(entry);
            else if(entry.Type == ParameterType.POSITIONAL)
                _positional.Add(entry);
            else if(entry.Type == ParameterType.SWITCH)
                _switch.Add(entry);
        }

        public void Add(string entry)
            => Add(InstructionSchemeEntry.ParseFromShorthand(entry));

        public IEnumerator<InstructionSchemeEntry> GetEnumerator()
            => _complete.GetEnumerator();

        public ParsedInstruction ParseInstruction(string[] args){
            
            // Create datasets
            string[] command = new string[_command.Count];
            Dictionary<string, string?> parameters = new Dictionary<string, string?>();
            Dictionary<string, bool> switches = new Dictionary<string, bool>();
            
            // Parse parameters
            int index = 0;
            int mode = 0;
            string? name = null; 
            foreach(string arg in args){
                bool isArgName = arg.StartsWith('-');
                string s = arg;
                if(isArgName)
                    while(s[0] == '-')
                        s = s.Substring(1);
                if(mode == 0 && index == _command.Count){
                    mode = 1;
                    index = 0;
                }
                switch(mode){
                    // Search for commands
                    case 0:
                        if(isArgName)
                            throw new FormatException($"The parameter {s} cannot be specified before the command.");
                        if(!_command[index].Values.Contains(s))
                            throw new FormatException($"The command {s} is unknown.");
                        command[index] = s;
                        index ++;
                        continue;
                    //Search for positional or named parameters or switch
                    case 1:
                        if(name == null){
                            // New named argument or switch
                            if(isArgName){
                                bool notFound = true;
                                foreach(InstructionSchemeEntry entry in _complete){
                                    if(entry.Values.Contains(s)){
                                        if(entry.Type == ParameterType.SWITCH)
                                            switches.Add(entry.Name, true);
                                        else if(entry.Type == ParameterType.NAMED || entry.Type == ParameterType.POSITIONAL)
                                            name = entry.Name;
                                        notFound = false;
                                        break;
                                    }
                                }
                                if(notFound)
                                    throw new FormatException($"The parameter {s} is unknown.");
                                continue;
                            }
                            // New positional value
                            else{
                                if(index == _positional.Count)
                                        throw new FormatException($"The instruction requires no positional parameters.");
                                while(parameters.ContainsKey(_positional[index].Name))
                                    index++;
                                    if(index == _positional.Count)
                                        throw new FormatException($"The instruction requires {_positional.Count} positional parameter(s).");
                                parameters.Add(_positional[index].Name, s);
                                continue;
                            }
                        }
                        else{
                            // Value of named argument
                            if(isArgName)
                                throw new FormatException($"The value of the named parameter {name} has not been defined.");
                            if(parameters.ContainsKey(name))
                                throw new FormatException($"The parameter {name} has already been defined.");
                            parameters.Add(name, s);
                            name = null;
                            continue;
                        }
                }
            }
            if(name != null)
                throw new FormatException($"The parameter {name} has not been provided a value.");

            // Fill optional parameters
            foreach(InstructionSchemeEntry entry in _complete){
                if(entry.Type == ParameterType.NAMED || entry.Type == ParameterType.POSITIONAL){
                    if(!parameters.ContainsKey(entry.Name)){
                        if(!entry.Optional)
                            throw new FormatException($"The mandatory parameter {entry.Name} has not been assigned.");
                        parameters.Add(entry.Name, null);
                    }
                }
                else if(entry.Type == ParameterType.SWITCH){
                    if(!switches.ContainsKey(entry.Name))
                        switches.Add(entry.Name, false);
                }
            }

            // Create and return instruction
            return new ParsedInstruction(){ Command = command, Parameters = parameters, Switches = switches, Scheme = this };
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}