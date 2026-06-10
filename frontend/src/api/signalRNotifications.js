import * as signalR from '@microsoft/signalr'
import { hubUrl } from '../config.js'
import { getToken } from '../lib/authStorage.js'

export function createNotificationConnection(handlers = {}) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl('/hubs/notifications'), {
      accessTokenFactory: () => getToken() ?? '',
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  connection.on('Connected', (payload) => {
    handlers.onConnected?.(payload)
  })

  connection.on('ReceiveNotification', (payload) => {
    handlers.onReceiveNotification?.(payload)
  })

  connection.on('EventRequestReviewed', (payload) => {
    handlers.onEventRequestReviewed?.(payload)
  })

  connection.on('AcademicEventChanged', (payload) => {
    handlers.onAcademicEventChanged?.(payload)
  })

  connection.onreconnecting(() => {
    handlers.onReconnecting?.()
  })

  connection.onreconnected(() => {
    handlers.onReconnected?.()
  })

  connection.onclose((error) => {
    handlers.onDisconnected?.(error)
  })

  return connection
}

export async function startNotificationConnection(connection) {
  if (!connection || connection.state === signalR.HubConnectionState.Connected) {
    return
  }
  if (connection.state === signalR.HubConnectionState.Connecting) {
    return
  }
  await connection.start()
}

export async function stopNotificationConnection(connection) {
  if (!connection) return
  if (connection.state === signalR.HubConnectionState.Disconnected) return
  await connection.stop()
}
