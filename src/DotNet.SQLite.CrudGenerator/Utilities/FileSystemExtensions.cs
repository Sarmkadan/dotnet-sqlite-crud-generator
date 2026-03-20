#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Extension methods for file system operations.
/// Provides utilities for safe directory/file creation, deletion, and manipulation.
/// </summary>
public static class FileSystemExtensions
{
    /// <summary>
    /// Creates a directory if it doesn't already exist.
    /// Returns true if directory was created, false if it already existed.
    /// </summary>
    public static bool CreateDirectoryIfNotExists(this string path)
    {
        if (Directory.Exists(path))
            return false;

        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to create directory: {path}", ex);
        }
    }

    /// <summary>
    /// Safely deletes a file if it exists.
    /// Returns true if file was deleted, false if it didn't exist.
    /// </summary>
    public static bool DeleteFileIfExists(this string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to delete file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Safely deletes a directory and all its contents.
    /// Returns true if directory was deleted, false if it didn't exist.
    /// </summary>
    public static bool DeleteDirectoryIfExists(this string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return false;

            Directory.Delete(path, recursive: true);
            return true;
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to delete directory: {path}", ex);
        }
    }

    /// <summary>
    /// Gets the file extension without the dot (e.g., "txt" instead of ".txt").
    /// </summary>
    public static string GetExtensionWithoutDot(this string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return extension.StartsWith(".") ? extension.Substring(1) : extension;
    }

    /// <summary>
    /// Checks if a file has a specific extension.
    /// Case-insensitive comparison.
    /// </summary>
    public static bool HasExtension(this string filePath, params string[] extensions)
    {
        var currentExtension = Path.GetExtension(filePath)
            .TrimStart('.')
            .ToLower();

        return extensions.Any(ext => ext.TrimStart('.').ToLower() == currentExtension);
    }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public static long GetFileSize(this string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return 0;

            var info = new FileInfo(filePath);
            return info.Length;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets the file size in human-readable format (B, KB, MB, GB).
    /// </summary>
    public static string GetFileSizeFormatted(this string filePath)
    {
        var bytes = filePath.GetFileSize();
        return FormatBytes(bytes);
    }

    /// <summary>
    /// Enumerates all files in a directory recursively, optionally filtering by extension.
    /// </summary>
    public static IEnumerable<string> GetFilesRecursively(this string path, params string[] extensions)
    {
        try
        {
            if (!Directory.Exists(path))
                return Enumerable.Empty<string>();

            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);

            if (extensions.Length == 0)
                return files;

            return files.Where(f => f.HasExtension(extensions));
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Checks if a path is a valid absolute path.
    /// </summary>
    public static bool IsAbsolutePath(this string path)
    {
        try
        {
            return Path.IsPathFullyQualified(path);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Combines multiple path segments safely.
    /// </summary>
    public static string CombinePaths(this string basePath, params string[] segments)
    {
        var parts = new[] { basePath }.Concat(segments).ToArray();
        return Path.Combine(parts);
    }

    /// <summary>
    /// Checks if a directory has any files.
    /// </summary>
    public static bool IsEmpty(this string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
                return true;

            return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Copies a directory recursively.
    /// </summary>
    public static void CopyDirectory(this string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

            var sourceInfo = new DirectoryInfo(sourcePath);
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            foreach (var file in sourceInfo.GetFiles())
                file.CopyTo(Path.Combine(destinationPath, file.Name), overwrite);

            foreach (var subdirectory in sourceInfo.GetDirectories())
                subdirectory.FullName.CopyDirectory(Path.Combine(destinationPath, subdirectory.Name), overwrite);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to copy directory: {sourcePath}", ex);
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
