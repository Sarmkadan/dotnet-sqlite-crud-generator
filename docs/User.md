# User

Represents a user account in the application, storing authentication details, profile information, and activity tracking. This type is used by the data-access layer to persist user records in SQLite and by the business logic layer to enforce access control and profile management.

## API

### `public int Id`
Unique identifier for the user. Assigned by the database on creation and immutable thereafter.

### `public required string Username`
User's login name. Must be unique across the system and non-empty. Enforced by the database schema.

### `public required string Email`
User's contact email address. Must be unique and conform to a valid email format. Enforced by the database schema.

### `public required string PasswordHash`
Securely hashed representation of the user's password. Never stored in plain text. Updated whenever the password changes.

### `public required string FirstName`
User's legal first name. Used for display and salutation. Must be non-empty.

### `public required string LastName`
User's legal last name. Used for display and salutation. Must be non-empty.

### `public string? PhoneNumber`
Optional contact phone number. May be null if the user has not provided one.

### `public string? Bio`
Optional free-form biographical text. May be null if the user has not provided one.

### `public bool IsActive`
Indicates whether the account is currently enabled. When `false`, login attempts are rejected regardless of credentials.

### `public bool EmailVerified`
Indicates whether the user has confirmed ownership of the email address. Used to gate sensitive actions.

### `public DateTime CreatedAt`
Timestamp of when the user record was created. Set once and never modified.

### `public DateTime UpdatedAt`
Timestamp of the most recent change to the user record. Updated automatically on every mutation.

### `public DateTime? LastLoginAt`
Timestamp of the user's most recent successful login, or `null` if the user has never logged in.

### `public bool Validate()`
Validates the current state of the user object. Returns `true` if all required fields are non-empty and the object is internally consistent; otherwise returns `false`. No exceptions are thrown.

### `public string GetFullName()`
Returns the user's full name by concatenating `FirstName` and `LastName` with a single space. Never throws.

### `public void RecordLogin()`
Updates `LastLoginAt` to the current UTC time and sets `UpdatedAt` to the same value. No return value and no exceptions.

### `public void Deactivate()`
Sets `IsActive` to `false` and updates `UpdatedAt` to the current UTC time. No return value and no exceptions.

## Usage
