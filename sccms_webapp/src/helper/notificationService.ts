// src/helper/notificationService.ts

import * as signalR from "@microsoft/signalr"
import { SD_BASE_URL } from "../utility/SD"

class NotificationService {
    private connection!: signalR.HubConnection

    public async startConnection() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${SD_BASE_URL}/notificationHub`, {
                // Kết nối tới đường dẫn đã map trên server
                accessTokenFactory: () => localStorage.getItem("token") || "",
                transport: signalR.HttpTransportType.WebSockets,
            })
            .withAutomaticReconnect()
            .build()

        try {
            await this.connection.start()
        } catch (err) {}
    }

    public onReceiveNotification(callback: (message: string, link: string) => void) {
        if (this.connection) {
            this.connection.on("ReceiveNotification", callback)
        } else {
            console.error("Connection is not initialized.")
        }
    }

    public stopConnection() {
        if (this.connection) {
            this.connection.stop()
        }
    }
}

export default new NotificationService()
