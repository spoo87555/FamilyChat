import { Container, Typography, Box, List, ListItem, ListItemText, CircularProgress, TextField, Button } from "@mui/material";
import { useMsal } from "@azure/msal-react";
import { apiRequest } from "../config/authConfig";
import { createHubConnection } from "../config/hubConfig";
import { useState, useEffect, useRef } from "react";
import { useParams } from "react-router-dom";
import * as signalR from "@microsoft/signalr";

interface Message {
    id: string;
    content: string;
    createdAt: string;
    editedAt: string | null;
    isEdited: boolean;
    chatId: string;
    chat: any | null;
    senderId: string;
    sender: {
        id: string;
        email: string;
        firstName: string;
        lastName: string;
        createdAt: string;
        lastLoginAt: string | null;
        isActive: boolean;
        deviceToken: string | null;
    };
}

export const ChatMessages = () => {
    const { chatId } = useParams<{ chatId: string }>();
    const { instance, accounts } = useMsal();
    const [messages, setMessages] = useState<Message[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [newMessage, setNewMessage] = useState("");
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const messagesEndRef = useRef<HTMLDivElement>(null);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    };

    const setupSignalRConnection = async () => {
        if (!accounts[0] || !chatId) return;

        try {
            const response = await instance.acquireTokenSilent({
                ...apiRequest,
                account: accounts[0]
            });

            const hubConnection = createHubConnection(response.accessToken);

            hubConnection.on("ReceiveMessage", (message: Message) => {
                console.log("Raw message received:", message);
                if (message.chatId === chatId) {
                    setMessages(prevMessages => [...prevMessages, message]);
                    scrollToBottom();
                }
            });

            await hubConnection.start();
            await hubConnection.invoke("JoinChat", chatId);
            setConnection(hubConnection);
        } catch (error) {
            console.error("Failed to setup SignalR connection:", error);
            setError("Failed to establish real-time connection. Please refresh the page.");
        }
    };

    const fetchMessages = async () => {
        if (!accounts[0] || !chatId) return;

        try {
            const response = await instance.acquireTokenSilent({
                ...apiRequest,
                account: accounts[0]
            });

            const apiResponse = await fetch(`https://localhost:7296/api/Messages/chat/${chatId}`, {
                headers: {
                    'Authorization': `Bearer ${response.accessToken}`
                }
            });

            if (!apiResponse.ok) {
                throw new Error(`HTTP error! status: ${apiResponse.status}`);
            }

            const data = await apiResponse.json();
            setMessages(data);
            setError(null);
            scrollToBottom();
        } catch (error) {
            console.error("Failed to fetch messages:", error);
            setError("Failed to load messages. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    const sendMessage = async () => {
        if (!accounts[0] || !chatId || !newMessage.trim()) return;

        try {
            const response = await instance.acquireTokenSilent({
                ...apiRequest,
                account: accounts[0]
            });

            const apiResponse = await fetch(`https://localhost:7296/api/Messages/chat/${chatId}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${response.accessToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newMessage)
            });

            if (!apiResponse.ok) {
                throw new Error(`HTTP error! status: ${apiResponse.status}`);
            }

            setNewMessage("");
        } catch (error) {
            console.error("Failed to send message:", error);
            setError("Failed to send message. Please try again.");
        }
    };

    useEffect(() => {
        fetchMessages();
        setupSignalRConnection();

        return () => {
            if (connection) {
                connection.stop();
            }
        };
    }, [chatId, accounts]);

    return (
        <Container maxWidth="md">
            <Box sx={{ mt: 4, height: '80vh', display: 'flex', flexDirection: 'column' }}>
                <Typography variant="h4" component="h1" gutterBottom align="center">
                    Chat Messages
                </Typography>

                {loading ? (
                    <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                        <CircularProgress />
                    </Box>
                ) : error ? (
                    <Typography color="error" align="center" sx={{ mt: 2 }}>
                        {error}
                    </Typography>
                ) : (
                    <>
                        <List sx={{ 
                            flexGrow: 1, 
                            overflow: 'auto',
                            bgcolor: 'background.paper',
                            borderRadius: 1,
                            p: 2
                        }}>
                            {messages.map((message) => (
                                <ListItem 
                                    key={message.id}
                                    sx={{ 
                                        mb: 1,
                                        borderRadius: 1,
                                        bgcolor: message.senderId === accounts[0]?.localAccountId 
                                            ? 'primary.light' 
                                            : 'grey.100'
                                    }}
                                >
                                    <ListItemText
                                        primary={message.content}
                                        secondary={`${message.sender.firstName} ${message.sender.lastName} - ${new Date(message.createdAt).toLocaleString()}`}
                                    />
                                </ListItem>
                            ))}
                            <div ref={messagesEndRef} />
                            {messages.length === 0 && (
                                <Typography align="center" sx={{ mt: 2 }}>
                                    No messages yet
                                </Typography>
                            )}
                        </List>
                        <Box sx={{ mt: 2, display: 'flex', gap: 1 }}>
                            <TextField
                                fullWidth
                                variant="outlined"
                                placeholder="Type a message..."
                                value={newMessage}
                                onChange={(e) => setNewMessage(e.target.value)}
                                onKeyPress={(e) => {
                                    if (e.key === 'Enter') {
                                        sendMessage();
                                    }
                                }}
                            />
                            <Button 
                                variant="contained" 
                                onClick={sendMessage}
                                disabled={!newMessage.trim()}
                            >
                                Send
                            </Button>
                        </Box>
                    </>
                )}
            </Box>
        </Container>
    );
}; 