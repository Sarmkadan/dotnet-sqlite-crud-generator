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
    /// <param name="path">The directory path to create.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or consists only of whitespace.</exception>
    /// <exception cref="IOException">Failed to create directory.</exception>
    public static bool CreateDirectoryIfNotExists(this string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

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
    /// <param name="filePath">The file path to delete.</param>
    /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or consists only of whitespace.</exception>
    /// <exception cref="IOException">Failed to delete file.</exception>
    public static bool DeleteFileIfExists(this string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

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
    /// <param name="path">The directory path to delete.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or consists only of whitespace.</exception>
    /// <exception cref="IOException">Failed to delete directory.</exception>
    public static bool DeleteDirectoryIfExists(this string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

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
    /// <param name="filePath">The file path.</param>
    /// <returns>The file extension without leading dot, or empty string if no extension.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or consists only of whitespace.</exception>
    public static string GetExtensionWithoutDot(this string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var extension = Path.GetExtension(filePath);
        return extension.StartsWith(".")
            ? extension.Substring(1)
            : extension;
    }

    /// <summary>
    /// Checks if a file has a specific extension.
    /// Case-insensitive comparison.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="extensions">Extensions to check against (without leading dots).</param>
    /// <returns>True if the file has one of the specified extensions.</returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="filePath"/> is <see langword="null"/>.
    ///   <paramref name="extensions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or consists only of whitespace.</exception>
    public static bool HasExtension(this string filePath, params string[] extensions)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(extensions);

        var currentExtension = Path.GetExtension(filePath)
            .TrimStart('.')
            .ToLowerInvariant();

        return extensions.Any(ext => ext.TrimStart('.').ToLowerInvariant() == currentExtension);
    }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file size in bytes, or 0 if file doesn't exist or cannot be accessed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or consists only of whitespace.</exception>
    public static long GetFileSize(this string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            if (!File.Exists(filePath))
                return 0;

            var info = new FileInfo(filePath);
            return info.Length;
        }
        catch (FileNotFoundException)
        {
            return 0;
        }
        catch (DirectoryNotFoundException)
        {
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            return 0;
        }
        catch (IOException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets the file size in human-readable format (B, KB, MB, GB).
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A human-readable file size string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or consists only of whitespace.</exception>
    public static string GetFileSizeFormatted(this string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var bytes = filePath.GetFileSize();
        return FormatBytes(bytes);
    }

    /// <summary>
    /// Enumerates all files in a directory recursively, optionally filtering by extension.
    /// </summary>
    /// <param name="path">The directory path to search.</param>
    /// <param name="extensions">Extensions to filter by (without leading dots).</param>
    /// <returns>An enumerable of file paths.</returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="path"/> is <see langword="null"/>.
    ///   <paramref name="extensions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or consists only of whitespace.</exception>
    public static IEnumerable<string> GetFilesRecursively(this string path, params string[] extensions)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(extensions);

        if (!Directory.Exists(path))
            return Enumerable.Empty<string>();

        try
        {
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);

            if (extensions.Length == 0)
                return files;

            return files.Where(f => f.HasExtension(extensions));
        }
        catch (DirectoryNotFoundException)
        {
            return Enumerable.Empty<string>();
        }
        catch (UnauthorizedAccessException)
        {
            return Enumerable.Empty<string>();
        }
        catch (IOException)
        {
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Checks if a path is a valid absolute path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is an absolute path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or consists only of whitespace.</exception>
    public static bool IsAbsolutePath(this string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

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
    /// <param name="basePath">The base path.</param>
    /// <param name="segments">Path segments to combine.</param>
    /// <returns>The combined path.</returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="basePath"/> is <see langword="null"/>.
    ///   <paramref name="segments"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="basePath"/> is empty or consists only of whitespace.</exception>
    public static string CombinePaths(this string basePath, params string[] segments)
    {
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
        ArgumentNullException.ThrowIfNull(segments);

        var parts = new[] { basePath }.Concat(segments).ToArray();
        return Path.Combine(parts);
    }

    /// <summary>
    /// Checks if a directory has any files.
    /// </summary>
    /// <param name="directoryPath">The directory path to check.</param>
    /// <returns>True if the directory is empty or doesn't exist.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="directoryPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="directoryPath"/> is empty or consists only of whitespace.</exception>
    public static bool IsEmpty(this string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        try
        {
            if (!Directory.Exists(directoryPath))
                return true;

            return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
        }
        catch (DirectoryNotFoundException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
        catch (IOException)
        {
            return true;
        }
    }

    /// <summary>
    /// Copies a directory recursively.
    /// </summary>
    /// <param name="sourcePath">The source directory path.</param>
    /// <param name="destinationPath">The destination directory path.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="sourcePath"/> is <see langword="null"/>.
    ///   <paramref name="destinationPath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="sourcePath"/> is empty or consists only of whitespace.
    ///   <paramref name="destinationPath"/> is empty or consists only of whitespace.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">Source directory not found.</exception>
    /// <exception cref="IOException">Failed to copy directory.</exception>
    public static void CopyDirectory(this string sourcePath, string destinationPath, bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(destinationPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

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
        catch (Exception ex) when (ex is not DirectoryNotFoundException and not IOException)
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