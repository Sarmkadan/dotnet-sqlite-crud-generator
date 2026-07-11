# StringExtensions

Provides common string manipulation and transformation utilities for converting between naming conventions, generating slugs, repeating strings, and other text-processing tasks.

## API

### `ToPascalCase`
Converts a string to PascalCase (capitalizing the first letter of each word and removing word separators).
- **Parameters**:
  - `input` (string): The string to convert.
- **Returns**: The PascalCase version of the input string.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `ToCamelCase`
Converts a string to camelCase (lowercasing the first letter of the first word and capitalizing subsequent words).
- **Parameters**:
  - `input` (string): The string to convert.
- **Returns**: The camelCase version of the input string.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `ToSnakeCase`
Converts a string to snake_case (lowercase words separated by underscores).
- **Parameters**:
  - `input` (string): The string to convert.
- **Returns**: The snake_case version of the input string.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `ToKebabCase`
Converts a string to kebab-case (lowercase words separated by hyphens).
- **Parameters**:
  - `input` (string): The string to convert.
- **Returns**: The kebab-case version of the input string.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `Pluralize`
Pluralizes a singular noun (basic pluralization rules).
- **Parameters**:
  - `input` (string): The singular noun to pluralize.
- **Returns**: The pluralized version of the input string.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `Truncate`
Truncates a string to a specified maximum length, optionally appending an ellipsis.
- **Parameters**:
  - `input` (string): The string to truncate.
  - `maxLength` (int): The maximum length of the output string.
  - `ellipsis` (string, optional): The string to append if truncation occurs. Defaults to `"…"`.
- **Returns**: The truncated string, or the original string if it is shorter than `maxLength`.
- **Throws**:
  - `ArgumentNullException` if `input` is `null`.
  - `ArgumentOutOfRangeException` if `maxLength` is negative.

### `IsNullOrWhiteSpace`
Determines whether a string is `null`, empty, or consists only of whitespace characters.
- **Parameters**:
  - `input` (string): The string to check.
- **Returns**: `true` if the string is `null`, empty, or whitespace; otherwise, `false`.

### `RemoveWhitespace`
Removes all whitespace characters from a string.
- **Parameters**:
  - `input` (string): The string to process.
- **Returns**: A new string with all whitespace removed.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `ToSlug`
Converts a string to a URL-friendly slug (lowercase, kebab-case, with special characters removed).
- **Parameters**:
  - `input` (string): The string to convert.
- **Returns**: The slug version of the input string.
- **Throws**: `ArgumentNullException` if `input` is `null`.

### `Repeat`
Repeats a string a specified number of times.
- **Parameters**:
  - `input` (string): The string to repeat.
  - `count` (int): The number of times to repeat the string.
- **Returns**: A new string composed of the input repeated `count` times.
- **Throws**:
  - `ArgumentNullException` if `input` is `null`.
  - `ArgumentOutOfRangeException` if `count` is negative.

### `MatchesPattern`
Checks whether a string matches a specified regular expression pattern.
- **Parameters**:
  - `input` (string): The string to check.
  - `pattern` (string): The regular expression pattern to match against.
- **Returns**: `true` if the input matches the pattern; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException` if `input` or `pattern` is `null`.
  - `ArgumentException` if `pattern` is not a valid regular expression.

### `FirstWord`
Extracts the first word from a string, delimited by whitespace or common separators.
- **Parameters**:
  - `input` (string): The string to process.
- **Returns**: The first word of the input string, or `null` if the input is `null` or empty.
- **Throws**: `ArgumentNullException` if `input` is `null`.

## Usage
