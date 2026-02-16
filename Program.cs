using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace GBFRDataExtrator;

public class Program
{
    private static GameSettings _gameSettings = null!;
    private static ToolSettings _toolSettings = null!;

    public static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("=== GBFR Data Extractor ===");
        Console.WriteLine();

        // Load configuration
        if (!LoadConfiguration())
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        // Get input path
        string? inputPath = GetInputPath();
        if (inputPath == null)
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        // Calculate all required paths
        var paths = CalculatePaths(inputPath);

        // Validate paths
        if (!ValidatePaths(paths))
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        DisplayPathInfo(paths);

        // Ensure target subfolder exists
        EnsureDirectoryExists(paths.TargetSubFolder);

        // Read file list
        string[] fileList = ReadFileList(inputPath);
        if (fileList.Length == 0)
        {
            Console.WriteLine("File list is empty!");
            return;
        }

        Console.WriteLine($"Found {fileList.Length} files to extract");
        Console.WriteLine();

        // Process all files
        var result = ProcessFiles(fileList, paths);

        // Display results
        DisplayResults(result);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Load configuration from appsettings.json
    /// </summary>
    private static bool LoadConfiguration()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            _gameSettings = configuration.GetSection("GameSettings").Get<GameSettings>()
                ?? throw new InvalidOperationException("Failed to load GameSettings from configuration");

            _toolSettings = configuration.GetSection("ToolSettings").Get<ToolSettings>()
                ?? throw new InvalidOperationException("Failed to load ToolSettings from configuration");

            Console.WriteLine("Configuration loaded successfully");
            Console.WriteLine();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            Console.WriteLine("Please ensure appsettings.json exists and is properly formatted.");
            return false;
        }
    }

    /// <summary>
    /// Get and validate user input path
    /// </summary>
    private static string? GetInputPath()
    {
        Console.Write("Enter the full path to modded_filelist.txt: ");
        string? inputPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
        {
            Console.WriteLine("Error: File does not exist!");
            return null;
        }

        return inputPath;
    }

    /// <summary>
    /// Calculate all required paths
    /// </summary>
    private static PathConfig CalculatePaths(string inputPath)
    {
        string inputDirectory = Path.GetDirectoryName(inputPath)!;
        string projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(inputDirectory)!)!;

        // Get parent folder name (e.g., gbfr.powerup.narmaya)
        string modFolderName = Path.GetFileName(inputDirectory)!;

        // Replace gbfr with gbfrelink
        string targetFolderName = modFolderName.Replace("gbfr.", "gbfrelink.");

        string originalFolder = Path.Combine(projectRoot, "original");
        string targetSubFolder = Path.Combine(originalFolder, targetFolderName);

        return new PathConfig
        {
            ProjectRoot = projectRoot,
            GbfrDataTools = _toolSettings.GBFRDataToolsPath,
            DataIndex = _gameSettings.DataIndexPath,
            DataFolder = _gameSettings.DataFolderPath,
            OriginalFolder = originalFolder,
            TargetSubFolder = targetSubFolder
        };
    }

    /// <summary>
    /// Validate all required paths exist
    /// </summary>
    private static bool ValidatePaths(PathConfig paths)
    {
        if (!File.Exists(paths.GbfrDataTools))
        {
            Console.WriteLine($"Error: Cannot find GBFRDataTools.exe at {paths.GbfrDataTools}");
            return false;
        }

        if (!File.Exists(paths.DataIndex))
        {
            Console.WriteLine($"Error: Cannot find data.i at {paths.DataIndex}");
            return false;
        }

        if (!Directory.Exists(paths.DataFolder))
        {
            Console.WriteLine($"Error: Cannot find data folder at {paths.DataFolder}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Display path information
    /// </summary>
    private static void DisplayPathInfo(PathConfig paths)
    {
        Console.WriteLine($"Project Root: {paths.ProjectRoot}");
        Console.WriteLine($"Original Folder: {paths.OriginalFolder}");
        Console.WriteLine($"Target Subfolder: {paths.TargetSubFolder}");
        Console.WriteLine();
    }

    /// <summary>
    /// Ensure directory exists, create if not
    /// </summary>
    private static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Console.WriteLine($"Created folder: {directoryPath}");
        }
    }

    /// <summary>
    /// Read file list
    /// </summary>
    private static string[] ReadFileList(string filePath)
    {
        return [.. File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())];
    }

    /// <summary>
    /// Process all files
    /// </summary>
    private static ProcessResult ProcessFiles(string[] fileList, PathConfig paths)
    {
        int successCount = 0;
        int failCount = 0;

        foreach (string relativePath in fileList)
        {
            Console.WriteLine($"Processing: {relativePath}");

            var result = ProcessSingleFile(relativePath, paths);

            switch (result)
            {
                case ProcessFileResult.Success:
                    successCount++;
                    break;
                case ProcessFileResult.Failed:
                    failCount++;
                    break;
                default:
                    break;
            }

            Console.WriteLine();
        }

        return new ProcessResult { SuccessCount = successCount, FailCount = failCount };
    }

    /// <summary>
    /// Process single file: extract and Move
    /// </summary>
    private static ProcessFileResult ProcessSingleFile(string relativePath, PathConfig paths)
    {
        try
        {
            // Check if target file already exists (with GBFR/data prefix)
            string targetRelativePath = Path.Combine("GBFR", "data", relativePath);
            string targetFilePath = Path.Combine(paths.TargetSubFolder, targetRelativePath);

            // Check if file should be skipped (original or converted file exists)
            if (ShouldSkipFile(targetFilePath))
            {
                Console.WriteLine($"  Skipped: File or converted file already exists");
                return ProcessFileResult.Skipped;
            }

            // Execute extraction
            if (!ExtractFile(relativePath, paths))
            {
                return ProcessFileResult.Failed;
            }

            // Move file
            if (!MoveExtractedFile(relativePath, paths))
            {
                return ProcessFileResult.Failed;
            }

            return ProcessFileResult.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
            return ProcessFileResult.Failed;
        }
    }

    /// <summary>
    /// Check if file should be skipped (original or converted file exists)
    /// </summary>
    private static bool ShouldSkipFile(string targetFilePath)
    {
        // Check if original file exists
        if (File.Exists(targetFilePath))
        {
            return true;
        }

        // Check if converted file exists based on extension
        string extension = Path.GetExtension(targetFilePath).ToLowerInvariant();

        if (extension == ".msg")
        {
            // Check for .json file
            string jsonPath = Path.ChangeExtension(targetFilePath, ".json");
            if (File.Exists(jsonPath))
            {
                return true;
            }
        }
        else if (extension == ".bxm")
        {
            // Check for .bxm.xml file
            string xmlPath = targetFilePath + ".xml";
            if (File.Exists(xmlPath))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Execute GBFRDataTools.exe to extract file
    /// </summary>
    private static bool ExtractFile(string relativePath, PathConfig paths)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = paths.GbfrDataTools,
            Arguments = $"extract -i \"{paths.DataIndex}\" -f \"{relativePath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        if (process == null)
        {
            Console.WriteLine("  Extraction failed: Unable to start process");
            return false;
        }

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"  Extraction failed: {error}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Move extracted file to target subfolder with GBFR/data prefix
    /// </summary>
    private static bool MoveExtractedFile(string relativePath, PathConfig paths)
    {
        string extractedFilePath = Path.Combine(paths.DataFolder, relativePath);

        if (!File.Exists(extractedFilePath))
        {
            Console.WriteLine($"  Warning: Cannot find extracted file at {extractedFilePath}");
            return false;
        }

        // Add GBFR/data prefix to the target path
        string targetRelativePath = Path.Combine("GBFR", "data", relativePath);
        string targetFilePath = Path.Combine(paths.TargetSubFolder, targetRelativePath);
        string targetDirectory = Path.GetDirectoryName(targetFilePath)!;

        EnsureDirectoryExists(targetDirectory);

        File.Move(extractedFilePath, targetFilePath, overwrite: true);
        Console.WriteLine($"  Success: Copied to {targetFilePath}");

        return true;
    }

    /// <summary>
    /// Display processing results
    /// </summary>
    private static void DisplayResults(ProcessResult result)
    {
        Console.WriteLine("=== Processing Complete ===");
        Console.WriteLine($"Success: {result.SuccessCount} file(s)");
        Console.WriteLine($"Failed: {result.FailCount} file(s)");
        Console.WriteLine();
    }
}
