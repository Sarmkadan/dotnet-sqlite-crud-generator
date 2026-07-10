# WebhookHandler

A utility class for registering, managing, and dispatching webhook endpoints in applications that require asynchronous event notifications. It tracks delivery attempts, maintains statistics, and allows enabling or disabling endpoints dynamically.

## API

### `WebhookHandler`
Initializes a new instance of the `WebhookHandler` class with a specified name and optional base URL for webhook endpoints.

### `RegisterEndpoint`
Registers a new webhook endpoint with the specified name, URL, and event types.

- **Parameters**:
  - `name` (`string`): A human-readable name for the endpoint.
  - `url` (`Uri`): The destination URL for webhook notifications.
  - `eventTypes` (`string[]`): An array of event types this endpoint subscribes to.
- **Returns**: `void`
- **Throws**: `ArgumentNullException` if `name`, `url`, or `eventTypes` is `null`.
- **Throws**: `UriFormatException` if `url` is not a valid absolute URI.

### `SendWebhookAsync<T>`
Asynchronously sends a webhook payload of type `T` to all registered and active endpoints subscribed to the specified event type.

- **Type Parameters**:
  - `T`: The type of the payload to send.
- **Parameters**:
  - `eventType` (`string`): The event type to trigger the webhook.
  - `payload` (`T`): The data payload to send.
- **Returns**: `Task<bool>`: A task that represents the asynchronous operation. The result is `true` if at least one delivery attempt was made; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `eventType` or `payload` is `null`.

### `DisableEndpoint`
Disables an existing webhook endpoint by name, preventing further deliveries until re-enabled.

- **Parameters**:
  - `name` (`string`): The name of the endpoint to disable.
- **Returns**: `void`
- **Throws**: `ArgumentNullException` if `name` is `null`.
- **Throws**: `KeyNotFoundException` if no endpoint with the given name exists.

### `EnableEndpoint`
Enables a previously disabled webhook endpoint by name, allowing deliveries to resume.

- **Parameters**:
  - `name` (`string`): The name of the endpoint to enable.
- **Returns**: `void`
- **Throws**: `ArgumentNullException` if `name` is `null`.
- **Throws**: `KeyNotFoundException` if no endpoint with the given name exists.

### `GetEndpoints`
Retrieves a collection of all registered webhook endpoints.

- **Returns**: `IEnumerable<WebhookEndpoint>`: An enumerable collection of registered endpoints.

### `GetDeliveryHistory`
Retrieves a collection of all delivery attempts across all endpoints.

- **Returns**: `IEnumerable<DeliveryAttempt>`: An enumerable collection of delivery attempts.

### `GetStatistics`
Retrieves aggregated statistics about webhook deliveries.

- **Returns**: `WebhookStatistics`: An object containing delivery counts, failure counts, and other metrics.

### `Name` (property)
Gets the name of the webhook handler instance.

- **Type**: `string`
- **Access**: Read-only

### `Url` (property)
Gets the base URL for webhook endpoints managed by this handler.

- **Type**: `Uri`
- **Access**: Read-only

### `EventTypes` (property)
Gets the array of event types this handler processes.

- **Type**: `string[]`
- **Access**: Read-only

### `Active` (property)
Gets or sets whether the handler is active and can process events.

- **Type**: `bool`
- **Access**: Read-write

### `CreatedAt` (property)
Gets the timestamp when the handler was created.

- **Type**: `DateTime`
- **Access**: Read-only

### `LastDeliveryAt` (property)
Gets the timestamp of the most recent delivery attempt, if any.

- **Type**: `DateTime?`
- **Access**: Read-only

### `DeliveryCount` (property)
Gets the total number of delivery attempts made by this handler.

- **Type**: `int`
- **Access**: Read-only

### `FailureCount` (property)
Gets the total number of failed delivery attempts made by this handler.

- **Type**: `int`
- **Access**: Read-only

### `DeliveryId` (property)
Gets the unique identifier for a specific delivery attempt.

- **Type**: `Guid`
- **Access**: Read-only

### `WebhookName` (property)
Gets the name of the webhook endpoint associated with a delivery attempt.

- **Type**: `string`
- **Access**: Read-only

### `EventType` (property)
Gets the event type associated with a delivery attempt.

- **Type**: `string`
- **Access**: Read-only

### `AttemptedAt` (property)
Gets the timestamp when a delivery attempt was made.

- **Type**: `DateTime`
- **Access**: Read-only

## Usage

### Registering and Sending a Webhook
