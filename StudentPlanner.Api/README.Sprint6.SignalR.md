# Sprint 6 backend: real-time notifications

This backend now exposes an authenticated ASP.NET Core SignalR hub at:

```text
/hubs/notifications
```

Clients should connect with the same JWT used by the REST API. Browser SignalR transports usually pass the token as `access_token` during WebSocket/SSE negotiation, and the API accepts that token only for `/hubs/notifications`.

## Server-to-client events

The hub sends these client method calls:

- `Connected` — sent to the caller after a successful hub connection.
- `ReceiveNotification` — toast-ready notification payload.
- `EventRequestReviewed` — sent to the manager who submitted a reviewed event request.
- `AcademicEventChanged` — sent to affected users when an admin-approved request creates, updates, or deletes a faculty event.

## Admin approval trigger

When an admin calls one of the existing moderation endpoints:

```text
POST /api/admin/event-requests/{id}/approve
POST /api/admin/event-requests/{id}/reject
PATCH /api/admin/event-requests/{id}/status
```

the backend commits the moderation result and then publishes SignalR notifications. Notification publishing is deliberately non-blocking for the moderation workflow: if WebSocket delivery fails, the event request review still stays committed and the error is logged.

## Recipients

- The submitting manager receives `ReceiveNotification` and `EventRequestReviewed` on approve and reject.
- For approved create requests, users assigned to the event faculty receive `ReceiveNotification` and `AcademicEventChanged`.
- For approved update/delete requests, users subscribed to the target academic event receive `ReceiveNotification` and `AcademicEventChanged`.

## Notification preferences

Users can toggle real-time notifications on or off. When disabled, the server skips sending all SignalR messages to that user. The preference is stored in the `NotificationsEnabled` column of the `AspNetUsers` table and can be updated via:

```text
PATCH /api/Auth/notifications
{ "enabled": false }
```

## Minimal frontend/client connection example

```js
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7001/hubs/notifications', {
    accessTokenFactory: () => localStorage.getItem('token') ?? ''
  })
  .withAutomaticReconnect()
  .build();

connection.on('ReceiveNotification', notification => {
  console.log('toast notification', notification);
});

connection.on('EventRequestReviewed', review => {
  console.log('event request reviewed', review);
});

connection.on('AcademicEventChanged', change => {
  console.log('academic event changed', change);
});

await connection.start();
```
