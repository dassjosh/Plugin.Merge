namespace PluginMerge.Writer;

/// <summary>
/// Represents a code write
/// Creates the final output code
/// </summary>
public class CodeWriter
{
    private int _indent;
    private readonly StringBuilder _writer;
    private readonly PluginData _pluginData;
    private readonly CodeStyleConfig _style;
    private readonly string _pluginNameReplacement;
    private bool _isInMultilineComment;

    /// <summary>
    /// Constructor for the code writer
    /// </summary>
    /// <param name="pluginData"></param>
    /// <param name="config"></param>
    public CodeWriter(PluginData pluginData, MergeConfig config)
    {
        _writer = new StringBuilder();
        _pluginData = pluginData;
        _style = config.CodeStyle;
        _pluginNameReplacement = $"{config.PluginName}.";
    }

    public void WriteRequiredPreprocessorDirectives(List<PreprocessorDirectiveConfig> preprocessorDirectives)
    {
        _writer.AppendLine($"#if ({string.Join(" && ", preprocessorDirectives.Select(pd => pd.Directive))})");
    }
    
    public void WritePreprocessorDirectiveError(List<PreprocessorDirectiveConfig> preprocessorDirectives)
    {
        for (int index = 0; index < preprocessorDirectives.Count; index++)
        {
            PreprocessorDirectiveConfig directive = preprocessorDirectives[index];
            if (index < preprocessorDirectives.Count - 1)
            {
                _writer.AppendLine($"#elif !{directive.Directive}");
            }
            else
            {
                _writer.AppendLine("#else");
            }

            _writer.AppendLine($"#error {directive.Message}");
        }
    }
    
    public void WriteEndPreprocessorDirectives()
    {
        _writer.AppendLine("#endif");
    }
    
    /// <summary>
    /// Writes references to the code
    /// </summary>
    /// <param name="references"></param>
    public void WriteReferences(IEnumerable<string> references)
    {
        foreach (string define in references.OrderBy(u => u))
        {
            _writer.Append(Constants.OxideDefinitions.Reference);
            _writer.Append(define);
            _writer.AppendLine();
        }
    }
    
    /// <summary>
    /// Writes requires to the code
    /// </summary>
    /// <param name="requires"></param>
    public void WriteRequires(IEnumerable<string> requires)
    {
        foreach (string define in requires.OrderBy(u => u))
        {
            _writer.Append(Constants.OxideDefinitions.Requires);
            _writer.Append(define);
            _writer.AppendLine();
        }
    }
    
    /// <summary>
    /// Writes defines to the code
    /// </summary>
    /// <param name="defines"></param>
    public void WriteDefines(IEnumerable<string> defines)
    {
        foreach (string define in defines.OrderBy(u => u))
        {
            _writer.Append("#define ");
            _writer.Append(define);
            _writer.AppendLine();
        }
    }

    /// <summary>
    /// Writes usings to the code
    /// </summary>
    /// <param name="usings"></param>
    public void WriteUsings(IEnumerable<string> usings)
    {
        bool didWrite = false;
        foreach (string @using in usings)
        {
            didWrite = true;
            WriteIndent();
            _writer.AppendLine(@using);
        }

        if (didWrite)
        {
            WriteLine();
        }
    }

    public void WriteUsing(string @using)
    {
        _writer.Append("using ");
        _writer.Append(@using);
        _writer.Append(';');
        _writer.AppendLine();
    }
    
    public void WriteUsingAlias(string type, string typeNamespace)
    {
        _writer.Append("using ");
        _writer.Append(type);
        _writer.Append(" = ");
        _writer.Append(typeNamespace);
        _writer.Append(';');
        _writer.AppendLine();
    }

    /// <summary>
    /// Writes namespace to the code
    /// </summary>
    /// <param name="namespace"></param>
    public void WriteNamespace(string @namespace, bool isFileScoped)
    {
        _writer.Append("namespace ");
        _writer.Append(@namespace);
        if (isFileScoped)
        {
            _writer.Append(';');
            _writer.AppendLine();
        }
    }

    /// <summary>
    /// Writes the start of the class to the code
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isPartial"></param>
    public void WriteStartClass(string name, List<string> baseTypes, bool isPartial = false)
    {
        WriteIndent();
        _writer.Append("public ");
        if (isPartial)
        {
            _writer.Append("partial ");
        }

        _writer.Append("class ");
        _writer.Append(name);

        if (baseTypes != null && baseTypes.Count != 0)
        {
            _writer.Append(" : ");
            _writer.Append(string.Join(", ", baseTypes));
        }

        WriteLine();
    }
        
    /// <summary>
    /// Writes a comment to the code
    /// </summary>
    /// <param name="comment"></param>
    public void WriteComment(string comment)
    {
        WriteIndent();
        _writer.Append("//");
        _writer.Append(comment);
        _writer.AppendLine();
    }

    public void WriteDefinition(string definition)
    {
        WriteIndent();
        _writer.Append(definition);
        WriteLine();
    }

    /// <summary>
    /// Writes the framework info to the code if is framework
    /// </summary>
    public void WriteFramework()
    {
        WriteDefinition(Constants.Definitions.Framework);
    }

    /// <summary>
    /// Write the plugin info attribute to the code
    /// </summary>
    /// <param name="comment"></param>
    public void WriteInfoAttribute(bool comment)
    {
        string title = _pluginData.Title;
        string author = _pluginData.Author;
        string version = _pluginData.Version;
            
        WriteIndent();
        if (comment)
        {
            _writer.Append("//");
        }
            
        _writer.Append($"[Info({title}, {author}, {version})]");
        WriteLine();
    }
        
    /// <summary>
    /// Write the plugin description attribute to the code
    /// </summary>
    /// <param name="comment"></param>
    public void WriteDescriptionAttribute(bool comment)
    {
        string description = _pluginData.Description;
            
        WriteIndent();
            
        if (comment)
        {
            _writer.Append("//");
        }
            
        _writer.Append($"[Description({description})]");
        WriteLine();
    }

    /// <summary>
    /// Writes a code line
    /// </summary>
    /// <param name="line"></param>
    public void WriteCode(ReadOnlySpan<char> line)
    {
        if (!_style.KeepComments)
        {
            if (line.StartsWith("//"))
            {
                return;
            }
            
            if (line.StartsWith("/*"))
            {
                _isInMultilineComment = true;
                return;
            }

            if (line.EndsWith("*/"))
            {
                _isInMultilineComment = false;
                return;
            }

            if (_isInMultilineComment)
            {
                return;
            }
        }

        bool isRegion = line.StartsWith("#region") || line.StartsWith("#endregion");
        if (!_style.WriteFileRegion && isRegion)
        {
            return;
        }

        if (line.Contains('}') && !line.Contains('{'))
        {
            _indent -= _style.IndentMultiplier;
        }

        WriteIndent();

        if (!isRegion && line.Contains(_pluginNameReplacement, StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<char> remaining = line;
            while (true)
            {
                int index = remaining.IndexOf(_pluginNameReplacement, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    break;
                }
                
                _writer.Append(remaining[..index]);
                remaining = remaining[(index + _pluginNameReplacement.Length)..];
            }
            
            _writer.Append(remaining);
        }
        else
        {
            _writer.Append(line);
        }
        
        WriteLine();
        if (line.Contains('{') && !line.Contains('}'))
        {
            _indent += _style.IndentMultiplier;
        }
    }

    /// <summary>
    /// Writes the opening bracket
    /// </summary>
    public void WriteStartBracket()
    {
        WriteIndent();
        _writer.Append('{');
        WriteLine();
        _indent += _style.IndentMultiplier;
    }
        
    /// <summary>
    /// Writes the start of a region
    /// </summary>
    /// <param name="name"></param>
    public void WriteStartRegion(string name)
    {
        if (!_style.WriteFileRegion)
        {
            return;
        }
        
        WriteIndent();
        _writer.Append("#region ");
        _writer.Append(name);
        WriteLine();
    }

    /// <summary>
    /// Writes a closing bracket
    /// </summary>
    public void WriteEndBracket()
    {
        _indent -= _style.IndentMultiplier;
        WriteIndent();
        _writer.Append('}');
        WriteLine();
    }
        
    /// <summary>
    /// Writes an end region
    /// </summary>
    public void WriteEndRegion()
    {
        if (!_style.WriteFileRegion)
        {
            return;
        }
        
        WriteIndent();
        _writer.Append("#endregion");
        WriteLine();
    }

    /// <summary>
    /// Write a line indent
    /// </summary>
    public void WriteIndent()
    {
        if (_indent > 0)
        {
            _writer.Append(_style.IndentCharacter, _indent * _style.IndentAmount);
        }
    }
        
    /// <summary>
    /// Writes a new line to the code
    /// </summary>
    public void WriteLine()
    {
        _writer.Append(_style.NewLine);
    }

    public string GetCode()
    {
        return _writer.ToString();
    }
}