import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr";

export const hubConfig = {
    url: "https://localhost:7296/hubs/chat",
    options: {
        skipNegotiation: true,
        transport: 1 // WebSockets only
    }
};

export const createHubConnection = (accessToken: string): HubConnection => {
    return new HubConnectionBuilder()
        .withUrl(`${hubConfig.url}?access_token=${accessToken}`, {
            skipNegotiation: hubConfig.options.skipNegotiation,
            transport: hubConfig.options.transport
        })
        .withAutomaticReconnect()
        .build();
}; 