namespace GBFRDataExtrator;

/// <summary>
/// Path configuration
/// </summary>
public class PathConfig
{
    /// <summary>
    /// Project root directory
    /// </summary>
    public string ProjectRoot { get; set; } = string.Empty;

    /// <summary>
    /// Full path to GBFRDataTools.exe
    /// </summary>
    public string GbfrDataTools { get; set; } = string.Empty;

    /// <summary>
    /// Path to game's data.i file
    /// </summary>
    public string DataIndex { get; set; } = string.Empty;

    /// <summary>
    /// Path to game's data folder
    /// </summary>
    public string DataFolder { get; set; } = string.Empty;

    /// <summary>
    /// Path to original folder
    /// </summary>
    public string OriginalFolder { get; set; } = string.Empty;

    /// <summary>
    /// Path to target subfolder (original/{ModName})
    /// </summary>
    public string TargetSubFolder { get; set; } = string.Empty;
}
