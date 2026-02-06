namespace GBFRDataExtrator;

/// <summary>
/// Game configuration settings
/// </summary>
public class GameSettings
{
    /// <summary>
    /// Path to game's data.i file
    /// </summary>
    public string DataIndexPath { get; set; } = string.Empty;

    /// <summary>
    /// Path to game's data folder
    /// </summary>
    public string DataFolderPath { get; set; } = string.Empty;
}
