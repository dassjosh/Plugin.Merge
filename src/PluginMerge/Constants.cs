using System.Text.RegularExpressions;

namespace PluginMerge;

/// <summary>
/// Application Constants
/// </summary>
public static class Constants
{
    /// <summary>
    /// Builder definitions
    /// </summary>
    public static class Definitions
    {
        /// <summary>
        /// When writing a file defines it as a framework
        /// </summary>
        public const string Framework = "//Define:Framework";
            
        /// <summary>
        /// Files with this comment will be ignored
        /// </summary>
        public const string ExcludeFile = "//Define:ExcludeFile";        
        
        /// <summary>
        /// Files with this comment will be ignored
        /// </summary>
        public const string ExtensionFile = "//Define:ExtensionMethods";
            
        /// <summary>
        /// Files with this comment will be ordered based on the tag
        /// </summary>
        public const string OrderFile = "//Define:FileOrder=";
    }

    /// <summary>
    /// Regexs for the application
    /// </summary>
    public static class Regexs
    {
        /// <summary>
        /// Matches the Info tag
        /// </summary>
        public static readonly Regex Info = new(@"\s*//\[Info\(\s*""(?<Title>.*)""\s*,\s*""(?<Author>.*)""\s*,\s*""(?<Version>.*)""\s*\)\]$", RegexOptions.Compiled);
            
        /// <summary>
        /// Matches the description tag
        /// </summary>
        public static readonly Regex Description = new(@"\s*//\[Description\(\s*""(?<Description>.*)""\s*\)\]$", RegexOptions.Compiled);
    }

    public static class CloseCodes
    {
        public const int NoError = 0;
        public const int ArgsError = 1;
        public const int InitError = 1001;
        public const int MergeConfigNotFoundError = 2001;
        public const int MergeConfigError = 2002;
        public const int MergeFilesError = 2003;
        public const int CompileFilesError = 2004;
        public const int RenameFileNotFound = 3001;
        public const int RenameFileContainsInvalidSource = 3002;
    }
}