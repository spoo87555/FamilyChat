import { Container, Typography, Box, List, ListItem, ListItemText, CircularProgress, Paper, Avatar, IconButton, Tooltip } from "@mui/material";
import { useMsal } from "@azure/msal-react";
import { apiRequest } from "../config/authConfig";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import AddIcon from '@mui/icons-material/Add';
import ChatIcon from '@mui/icons-material/Chat';

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
    const navigate = useNavigate();

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

    const handleChatClick = (chatId: string) => {
        navigate(`/chat/${chatId}`);
    };

    const getInitials = (name: string) => {
        return name.split(' ').map(word => word[0]).join('').toUpperCase();
    };

    const formatDate = (dateString: string) => {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { 
            year: 'numeric', 
            month: 'short', 
            day: 'numeric' 
        });
    };

    return (
        <Container maxWidth="md" sx={{ height: '100vh', py: 2 }}>
            <Paper elevation={3} sx={{ 
                height: '100%', 
                display: 'flex', 
                flexDirection: 'column',
                bgcolor: '#ffffff',
                borderRadius: '16px',
                overflow: 'hidden'
            }}>
                {/* Header */}
                <Box sx={{ 
                    p: 2, 
                    borderBottom: 1, 
                    borderColor: 'divider',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    bgcolor: '#954d48',
                    color: 'white'
                }}>
                    <Typography variant="h6" component="h1">
                        Your Chats
                    </Typography>
                    <Tooltip title="Create new chat">
                        <IconButton 
                            sx={{ 
                                color: 'white',
                                '&:hover': {
                                    bgcolor: 'rgba(255, 255, 255, 0.1)'
                                }
                            }}
                        >
                            <AddIcon />
                        </IconButton>
                    </Tooltip>
                </Box>

                {loading ? (
                    <Box sx={{ 
                        display: 'flex', 
                        justifyContent: 'center', 
                        alignItems: 'center',
                        flexGrow: 1
                    }}>
                        <CircularProgress sx={{ color: '#954d48' }} />
                    </Box>
                ) : error ? (
                    <Box sx={{ 
                        display: 'flex', 
                        justifyContent: 'center', 
                        alignItems: 'center',
                        flexGrow: 1
                    }}>
                        <Typography color="error" align="center">
                            {error}
                        </Typography>
                    </Box>
                ) : (
                    <List sx={{ 
                        flexGrow: 1, 
                        overflow: 'auto',
                        p: 2,
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 1.5
                    }}>
                        {chats.map((chat) => (
                            <ListItem 
                                key={chat.id}
                                onClick={() => handleChatClick(chat.id)}
                                sx={{ 
                                    bgcolor: 'white',
                                    borderRadius: '12px',
                                    boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
                                    cursor: 'pointer',
                                    transition: 'all 0.2s ease',
                                    width: '100%',
                                    maxWidth: '100%',
                                    '&:hover': {
                                        transform: 'translateY(-2px)',
                                        boxShadow: '0 4px 8px rgba(0,0,0,0.15)',
                                        bgcolor: '#fafafa'
                                    }
                                }}
                            >
                                <Avatar 
                                    sx={{ 
                                        bgcolor: '#954d48',
                                        mr: 2
                                    }}
                                >
                                    <ChatIcon />
                                </Avatar>
                                <ListItemText
                                    primary={
                                        <Typography variant="subtitle1" sx={{ fontWeight: 500 }}>
                                            {chat.name}
                                        </Typography>
                                    }
                                    secondary={
                                        <Box sx={{ 
                                            display: 'flex', 
                                            flexDirection: 'column',
                                            gap: 0.5,
                                            mt: 0.5
                                        }}>
                                            <Typography variant="caption" color="text.secondary">
                                                Created by: {chat.createdBy}
                                            </Typography>
                                            <Typography variant="caption" color="text.secondary">
                                                {formatDate(chat.createdAt)}
                                            </Typography>
                                        </Box>
                                    }
                                />
                            </ListItem>
                        ))}
                        {chats.length === 0 && (
                            <Box sx={{ 
                                display: 'flex', 
                                flexDirection: 'column',
                                justifyContent: 'center', 
                                alignItems: 'center',
                                flexGrow: 1,
                                gap: 2
                            }}>
                                <ChatIcon sx={{ fontSize: 48, color: '#954d48', opacity: 0.5 }} />
                                <Typography color="text.secondary" align="center">
                                    No chats available. Start a new conversation!
                                </Typography>
                            </Box>
                        )}
                    </List>
                )}
            </Paper>
        </Container>
    );
}; 