# WebhookHandlerExtensions

Provides extension methods for retrieving and analyzing webhook endpoint data, delivery attempts, and success metrics within the SQLite-based CRUD generator project.

## API

### `GetActiveEndpointsForEvent`

Returns all webhook endpoints that are currently active and configured to receive events of a specified type.

- **Parameters**
  - `eventType` (string): The event type identifier to filter endpoints.
- **Return value**
  - `IEnumerable<WebhookEndpoint>`: An enumerable of active endpoints matching the event type.
- **Exceptions**
  - Throws `ArgumentNullException` if `eventType` is null.

### `GetDeliveryAttemptsByEvent`

Retrieves all delivery attempts for a specific event, ordered by attempt time.

- **Parameters**
  - `eventId` (Guid): The unique identifier of the event.
- **Return value**
  - `IEnumerable<DeliveryAttempt>`: An enumerable of delivery attempts for the event.
- **Exceptions**
  - Throws `ArgumentException` if `eventId` is empty.

### `GetStatisticsForEndpoint`

Calculates and returns success statistics for a given webhook endpoint.

- **Parameters**
  - `endpointId` (Guid): The unique identifier of the endpoint.
- **Return value**
  - `WebhookStatistics?`: A statistics object containing success rate, total attempts, and last attempt time, or null if no attempts exist.
- **Exceptions**
  - Throws `ArgumentException` if `endpointId` is empty.

### `GetSuccessRate`

Computes the success rate (0.0 to 1.0) for a specific endpoint and event type combination.

- **Parameters**
  - `endpointId` (Guid): The unique identifier of the endpoint.
  - `eventType` (string): The event type to evaluate.
- **Return value**
  - `double`: A value between 0.0 and 1.0 representing the success rate, or 0.0 if no attempts exist.
- **Exceptions**
  - Throws `ArgumentException` if `endpointId` is empty.
  - Throws `ArgumentNullException` if `eventType` is null.

## Usage
