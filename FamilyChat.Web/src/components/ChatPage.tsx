import { Container, Typography, Box, List, ListItem, ListItemText, CircularProgress } from "@mui/material";
import { useMsal } from "@azure/msal-react";
import { apiRequest } from "../config/authConfig";
import { useState, useEffect } from "react";

interface Chat {
    id: string;
    name: string;
    createdAt: string;
    createdBy: string;
}

export const ChatPage = () => {
    const { instance, accounts } = useMsal();
    const [chats, setChats] = useState<Chat[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchChats = async () => {
        if (!accounts[0]) return;

        try {
            const response = await instance.acquireTokenSilent({
                ...apiRequest,
                account: accounts[0]
            });

            const apiResponse = await fetch('https://localhost:7296/api/Chats', {
                headers: {
                    'Authorization': `Bearer ${response.accessToken}`
                }
            });

            if (!apiResponse.ok) {
                throw new Error(`HTTP error! status: ${apiResponse.status}`);
            }

            const data = await apiResponse.json();
            setChats(data);
            setError(null);
        } catch (error) {
            console.error("Failed to fetch chats:", error);
            setError("Failed to load chats. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchChats();
    }, [accounts]);

    return (
        <Container maxWidth="sm">
            <Box sx={{ mt: 4 }}>
                <Typography variant="h4" component="h1" gutterBottom align="center">
                    Chat Page
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
                    <List sx={{ mt: 2 }}>
                        {chats.map((chat) => (
                            <ListItem 
                                key={chat.id}
                                sx={{ 
                                    bgcolor: 'background.paper',
                                    mb: 1,
                                    borderRadius: 1,
                                    boxShadow: 1
                                }}
                            >
                                <ListItemText
                                    primary={chat.name}
                                    secondary={`Created by: ${chat.createdBy} on ${new Date(chat.createdAt).toLocaleDateString()}`}
                                />
                            </ListItem>
                        ))}
                        {chats.length === 0 && (
                            <Typography align="center" sx={{ mt: 2 }}>
                                No chats available
                            </Typography>
                        )}
                    </List>
                )}
            </Box>
        </Container>
    );
}; 