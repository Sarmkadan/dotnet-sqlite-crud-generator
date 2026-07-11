#nullable enable

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Provides extension methods for <see cref="BulkTransferOptions"/> to simplify configuration
/// and enable common bulk transfer scenarios.
/// </summary>
public static class BulkTransferOptionsExtensions
{
	/// <summary>
	/// Configures the options for high-performance bulk operations with minimal overhead.
	/// </summary>
	/// <param name="options">The options to configure. Cannot be <see langword="null"/>.</param>
	/// <param name="batchSize">Number of entities per batch. Defaults to 2000.</param>
	/// <param name="maxConcurrency">Maximum concurrent operations. Defaults to 8.</param>
	/// <returns>The configured options for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public static BulkTransferOptions WithHighPerformance(
		this BulkTransferOptions options,
		int? batchSize = null,
		int? maxConcurrency = null)
	{
		ArgumentNullException.ThrowIfNull(options);

		options.BatchSize = batchSize ?? 2000;
		options.MaxConcurrency = maxConcurrency ?? 8;
		options.ValidationMode = ValidationMode.None;
		options.ProgressReportingInterval = 500;
		options.BatchTimeout = TimeSpan.FromMinutes(2);
		options.EnableCheckpointing = false;
		return options;
	}

	/// <summary>
	/// Configures the options for safe, reliable bulk operations with strict validation.
	/// </summary>
	/// <param name="options">The options to configure. Cannot be <see langword="null"/>.</param>
	/// <param name="batchSize">Number of entities per batch. Defaults to 100.</param>
	/// <param name="maxConcurrency">Maximum concurrent operations. Defaults to 2.</param>
	/// <param name="checkpointPath">Path to checkpoint file for resuming interrupted operations.</param>
	/// <returns>The configured options for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public static BulkTransferOptions WithSafety(
		this BulkTransferOptions options,
		int? batchSize = null,
		int? maxConcurrency = null,
		string? checkpointPath = null)
	{
		ArgumentNullException.ThrowIfNull(options);

		options.BatchSize = batchSize ?? 100;
		options.MaxConcurrency = maxConcurrency ?? 2;
		options.ValidationMode = ValidationMode.Strict;
		options.EnableCheckpointing = true;
		options.CheckpointFilePath = checkpointPath;
		options.MaxErrorThreshold = 10;
		options.BatchTimeout = TimeSpan.FromSeconds(15);
		return options;
	}

	/// <summary>
	/// Configures the options for balanced bulk operations suitable for most scenarios.
	/// </summary>
	/// <param name="options">The options to configure. Cannot be <see langword="null"/>.</param>
	/// <param name="batchSize">Number of entities per batch. Defaults to 500.</param>
	/// <param name="maxConcurrency">Maximum concurrent operations. Defaults to 4.</param>
	/// <returns>The configured options for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public static BulkTransferOptions WithBalancedDefaults(
		this BulkTransferOptions options,
		int? batchSize = null,
		int? maxConcurrency = null)
	{
		ArgumentNullException.ThrowIfNull(options);

		options.BatchSize = batchSize ?? 500;
		options.MaxConcurrency = maxConcurrency ?? 4;
		options.ValidationMode = ValidationMode.Lenient;
		options.EnableProgressReporting = true;
		options.ProgressReportingInterval = 100;
		options.UseTransactions = true;
		options.BatchTimeout = TimeSpan.FromSeconds(30);
		return options;
	}

	/// <summary>
	/// Disables progress reporting to reduce overhead in tight-loop scenarios.
	/// </summary>
	/// <param name="options">The options to configure. Cannot be <see langword="null"/>.</param>
	/// <returns>The configured options for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public static BulkTransferOptions WithoutProgressReporting(this BulkTransferOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		options.EnableProgressReporting = false;
		return options;
	}

	/// <summary>
	/// Enables checkpointing with the specified checkpoint file path.
	/// </summary>
	/// <param name="options">The options to configure. Cannot be <see langword="null"/>.</param>
	/// <param name="checkpointFilePath">Absolute path to the checkpoint file. Cannot be <see langword="null"/> or whitespace.</param>
	/// <returns>The configured options for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="checkpointFilePath"/> is <see langword="null"/>, empty, or whitespace.</exception>
	public static BulkTransferOptions WithCheckpointing(
		this BulkTransferOptions options,
		string checkpointFilePath)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentException.ThrowIfNullOrWhiteSpace(checkpointFilePath);

		options.EnableCheckpointing = true;
		options.CheckpointFilePath = checkpointFilePath;
		return options;
	}

	/// <summary>
	/// Sets the maximum number of allowed errors before the operation is aborted.
	/// </summary>
	/// <param name="options">The options to configure. Cannot be <see langword="null"/>.</param>
	/// <param name="maxErrors">Maximum error threshold. Use 0 to disable error limit.</param>
	/// <returns>The configured options for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public static BulkTransferOptions WithErrorThreshold(
		this BulkTransferOptions options,
		int maxErrors)
	{
		ArgumentNullException.ThrowIfNull(options);

		options.MaxErrorThreshold = maxErrors;
		return options;
	}

	/// <summary>
	/// Disables transaction usage for bulk operations when atomicity is not required.
	/// </summary>
	/// <param name="options">The options to configure. Cannot be <see langword="null"/>.</param>
	/// <returns>The configured options for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public static BulkTransferOptions WithoutTransactions(this BulkTransferOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		options.UseTransactions = false;
		return options;
	}

	/// <summary>
	/// Sets a custom batch timeout for the bulk operation.
	/// </summary>
	/// <param name="options">The options to configure. Cannot be <see langword="null"/>.</param>
	/// <param name="timeout">Batch timeout duration. Cannot be negative.</param>
	/// <returns>The configured options for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is negative.</exception>
	public static BulkTransferOptions WithBatchTimeout(
		this BulkTransferOptions options,
		TimeSpan timeout)
	{
		ArgumentNullException.ThrowIfNull(options);
		if (timeout < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout cannot be negative.");
		}

		options.BatchTimeout = timeout;
		return options;
	}

	/// <summary>
	/// Sets a custom buffer size for stream operations.
	/// </summary>
	/// <param name="options">The options to configure. Cannot be <see langword="null"/>.</param>
	/// <param name="bufferSize">Buffer size in bytes. Must be positive.</param>
	/// <returns>The configured options for method chaining.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is not positive.</exception>
	public static BulkTransferOptions WithBufferSize(
		this BulkTransferOptions options,
		int bufferSize)
	{
		ArgumentNullException.ThrowIfNull(options);
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be positive.");
		}

		options.BufferSize = bufferSize;
		return options;
	}

	/// <summary>
	/// Clones the current options to create a new independent instance.
	/// </summary>
	/// <param name="options">The options to clone. Cannot be <see langword="null"/>.</param>
	/// <returns>A deep copy of the options.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public static BulkTransferOptions Clone(this BulkTransferOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		return new BulkTransferOptions
		{
			BatchSize = options.BatchSize,
			MaxConcurrency = options.MaxConcurrency,
			EnableProgressReporting = options.EnableProgressReporting,
			ProgressReportingInterval = options.ProgressReportingInterval,
			BufferSize = options.BufferSize,
			EnableCheckpointing = options.EnableCheckpointing,
			CheckpointFilePath = options.CheckpointFilePath,
			ValidationMode = options.ValidationMode,
			MaxErrorThreshold = options.MaxErrorThreshold,
			UseTransactions = options.UseTransactions,
			BatchTimeout = options.BatchTimeout
		};
	}

	/// <summary>
	/// Determines whether the configuration represents a high-throughput scenario.
	/// </summary>
	/// <param name="options">The options to check. Cannot be <see langword="null"/>.</param>
	/// <returns>True if configured for high throughput; otherwise false.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public static bool IsHighThroughput(this BulkTransferOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		return options.BatchSize >= 1000
			&& options.MaxConcurrency >= 6
			&& options.ValidationMode == ValidationMode.None;
	}

	/// <summary>
	/// Determines whether checkpointing is enabled and properly configured.
	/// </summary>
	/// <param name="options">The options to check. Cannot be <see langword="null"/>.</param>
	/// <returns>True if checkpointing is enabled and has a file path; otherwise false.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	public static bool IsCheckpointingConfigured(this BulkTransferOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		return options.EnableCheckpointing
			&& !string.IsNullOrWhiteSpace(options.CheckpointFilePath);
	}
}