# FileSystemExtensions

Provides a set of static utility methods for common file system operations such as directory and file manipulation, path handling, and size calculations. Designed to simplify recurring file system tasks with robust error handling and consistent behavior across different environments.

## API

### `public static bool CreateDirectoryIfNotExists(string path)`

Creates the specified directory if it does not already exist.

- **Parameters**
  - `path`: The absolute or relative path of the directory to create.
- **Return Value**
  - `true` if the directory was created or already exists; `false` if creation failed (e.g., due to permissions or invalid path).
- **Exceptions**
  - Throws `ArgumentNullException` if `path` is `null`.
  - Throws `ArgumentException` if `path` is empty or contains invalid characters.

---

### `public static bool DeleteFileIfExists(string filePath)`

Deletes the specified file if it exists.

- **Parameters**
  - `filePath`: The path of the file to delete.
- **Return Value**
  - `true` if the file existed and was deleted, or if the file did not exist; `false` if deletion failed (e.g., due to permissions or locked file).
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.
  - Throws `ArgumentException` if `filePath` is empty or invalid.

---

### `public static bool DeleteDirectoryIfExists(string directoryPath)`

Deletes the specified directory and all its contents if it exists.

- **Parameters**
  - `directoryPath`: The path of the directory to delete.
- **Return Value**
  - `true` if the directory existed and was deleted, or if the directory did not exist; `false` if deletion failed (e.g., due to permissions or non-empty directory).
- **Exceptions**
  - Throws `ArgumentNullException` if `directoryPath` is `null`.
  - Throws `ArgumentException` if `directoryPath` is empty or invalid.

---

### `public static string GetExtensionWithoutDot(string filePath)`

Extracts the file extension from a file path, excluding the leading dot.

- **Parameters**
  - `filePath`: The path of the file.
- **Return Value**
  - The file extension without the dot (e.g., `"txt"` for `"file.txt"`), or an empty string if no extension exists.
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.

---

### `public static bool HasExtension(string filePath)`

Determines whether the specified file path has a file extension.

- **Parameters**
  - `filePath`: The path of the file.
- **Return Value**
  - `true` if the file path has a non-empty extension; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.

---

### `public static long GetFileSize(string filePath)`

Gets the size in bytes of the specified file.

- **Parameters**
  - `filePath`: The path of the file.
- **Return Value**
  - The size of the file in bytes, or `-1` if the file does not exist or cannot be accessed.
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.

---

### `public static string GetFileSizeFormatted(string filePath)`

Gets the size of the specified file formatted as a human-readable string (e.g., "1.23 KB").

- **Parameters**
  - `filePath`: The path of the file.
- **Return Value**
  - A formatted string representing the file size with appropriate unit (B, KB, MB, GB), or `"N/A"` if the file does not exist or cannot be accessed.
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.

---

### `public static IEnumerable<string> GetFilesRecursively(string directoryPath)`

Recursively retrieves all files under the specified directory.

- **Parameters**
  - `directoryPath`: The root directory to search.
- **Return Value**
  - An enumerable of absolute file paths found recursively. Returns an empty sequence if the directory does not exist.
- **Exceptions**
  - Throws `ArgumentNullException` if `directoryPath` is `null`.
  - Throws `ArgumentException` if `directoryPath` is empty or invalid.

---

### `public static bool IsAbsolutePath(string path)`

Determines whether the specified path is an absolute path.

- **Parameters**
  - `path`: The path to evaluate.
- **Return Value**
  - `true` if the path is absolute; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `path` is `null`.

---

### `public static string CombinePaths(params string[] paths)`

Combines multiple path segments into a single path using the platform-specific separator.

- **Parameters**
  - `paths`: One or more path segments to combine.
- **Return Value**
  - The combined path. Returns `null` if `paths` is `null` or empty.
- **Exceptions**
  - Throws `ArgumentNullException` if any element in `paths` is `null`.

---

### `public static bool IsEmpty(string directoryPath)`

Determines whether the specified directory is empty (i.e., contains no files or subdirectories).

- **Parameters**
  - `directoryPath`: The path of the directory to check.
- **Return Value**
  - `true` if the directory exists and is empty; `false` otherwise.
- **Exceptions**
  - Throws `ArgumentNullException` if `directoryPath` is `null`.
  - Throws `ArgumentException` if `directoryPath` is empty or invalid.

---
### `public static void CopyDirectory(string sourceDir, string destinationDir)`

Recursively copies the contents of the source directory to the destination directory. If the destination directory does not exist, it will be created.

- **Parameters**
  - `sourceDir`: The source directory to copy.
  - `destinationDir`: The destination directory where contents will be copied.
- **Exceptions**
  - Throws `ArgumentNullException` if `sourceDir` or `destinationDir` is `null`.
  - Throws `ArgumentException` if either path is empty or invalid.
  - Throws `DirectoryNotFoundException` if `sourceDir` does not exist.
  - Throws `UnauthorizedAccessException` if access to a file or directory is denied.

## Usage
