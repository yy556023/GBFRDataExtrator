namespace GBFRDataExtrator;

/// <summary>
/// Process result statistics
/// </summary>
public class ProcessResult
{
    /// <summary>
    /// Number of successfully processed files
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed files
    /// </summary>
    public int FailCount { get; set; }

    /// <summary>
    /// Total number of files
    /// </summary>
    public int TotalCount => SuccessCount + FailCount;
}
