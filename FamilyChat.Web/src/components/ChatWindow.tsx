import { Container, Typography, Box, List, ListItem, CircularProgress, TextField, Paper, Avatar, IconButton, Tooltip } from "@mui/material";
import SendIcon from '@mui/icons-material/Send';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import ChatIcon from '@mui/icons-material/Chat';
import CloseIcon from '@mui/icons-material/Close';
import { useEffect, useRef, useState } from "react";
import { createHubConnection } from "../config/hubConfig";
import { useMsal } from "@azure/msal-react";
import { apiRequest } from "../config/authConfig";
import { useNavigate } from 'react-router-dom';

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

interface ChatWindowProps {
    chatId: string;
    show: boolean;
    setShow: (minimized: boolean) => void;
}

export const ChatWindow = ({ chatId, show, setShow }: ChatWindowProps) => {

    const { instance, accounts } = useMsal();
    const [messages, setMessages] = useState<Message[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [newMessage, setNewMessage] = useState("");
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const messagesEndRef = useRef<HTMLDivElement>(null);
    const navigate = useNavigate(); // Call the hook ONCE at the top


    const scrollToBottom = () => {
            messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
        };
    
        const getInitials = (firstName: string, lastName: string) => {
            return `${firstName[0]}${lastName[0]}`.toUpperCase();
        };
    
        const formatMessageTime = (dateString: string) => {
            const date = new Date(dateString);
            return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
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
                    if (message.chatId === chatId && message.senderId !== accounts[0]?.localAccountId) {
                        setMessages(prevMessages => {
                            // Check if message already exists
                            const messageExists = prevMessages.some(m => m.id === message.id);
                            if (messageExists) {
                                return prevMessages;
                            }
                            return [...prevMessages, message];
                        });
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
            console.log("Fetching messages...");
            console.log("account:", accounts[0]);
            console.log("chatId:", chatId);
            if (!accounts[0] || !chatId) return;
    
            try {
                const response = await instance.acquireTokenSilent({
                    ...apiRequest,
                    account: accounts[0]
                });
                
                console.log("Access token:", response.accessToken);

                const apiResponse = await fetch(`https://localhost:7296/api/Messages/chat/${chatId}`, {
                    headers: {
                        'Authorization': `Bearer ${response.accessToken}`
                    }
                });

                console.log("API response:", apiResponse);
    
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
            console.log("ChatWindow mounted");
            fetchMessages();
            setupSignalRConnection();
    
            return () => {
                if (connection) {
                    connection.stop();
                }
            };
        }, [chatId, accounts]);

    return <Container maxWidth="md" sx={{
        height: '100vh',
        py: 2,
        position: 'absolute',
        right: 0,
        top: 0,
        width: {
            xs: '100%',
            sm: '100%',
            md: show ? '50%' : 'auto',
        },
        margin: 0,
        padding: 2,
        transition: 'all 0.3s ease-in-out'
    }}>
        <Paper elevation={3} sx={{
            height: show ? '100%' : 'auto',
            display: 'flex',
            flexDirection: 'column',
            bgcolor: '#ffffff',
            borderRadius: '16px',
            overflow: 'hidden',
            transition: 'all 0.3s ease-in-out'
        }}>
            {/* Header */}
            <Box sx={{
                p: 2,
                borderBottom: 1,
                borderColor: 'divider',
                display: 'flex',
                alignItems: 'center',
                gap: 2,
                bgcolor: '#954d48',
                color: 'white',
                justifyContent: 'space-between'
            }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Typography variant="h6" component="h1">
                        Family
                    </Typography>
                </Box>
                <IconButton
                    onClick={() => setShow(!show)}
                    sx={{ color: 'white' }}
                >
                    {show ? <CloseIcon /> : <ChatIcon />}
                </IconButton>
            </Box>
            {loading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', flexGrow: 1 }}>
                    <CircularProgress sx={{ color: '#954d48' }} />
                </Box>
            ) : error ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', flexGrow: 1 }}>
                    <Typography color="error" align="center">
                        {error}
                    </Typography>
                </Box>
            ) : (
                <>
                    {/* Messages List */}
                    <List sx={{
                        flexGrow: 1,
                        overflow: 'auto',
                        p: 2,
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 1.5,
                        bgcolor: '#ffffff'
                    }}>
                        {messages.map((message) => (
                            <ListItem
                                key={message.id}
                                sx={{
                                    alignSelf: message.senderId === accounts[0]?.localAccountId
                                        ? 'flex-end'
                                        : 'flex-start',
                                    width: '100%',
                                    maxWidth: '100%',
                                    p: 0
                                }}
                            >
                                <Box sx={{
                                    display: 'flex',
                                    flexDirection: message.senderId === accounts[0]?.localAccountId
                                        ? 'row-reverse'
                                        : 'row',
                                    alignItems: 'flex-end',
                                    gap: 1,
                                    width: '100%',
                                    maxWidth: '100%'
                                }}>
                                    <Avatar
                                        sx={{
                                            bgcolor: message.senderId === accounts[0]?.localAccountId
                                                ? '#954d48'
                                                : '#7a3d38',
                                            width: 32,
                                            height: 32,
                                            fontSize: '0.875rem',
                                            flexShrink: 0
                                        }}
                                    >
                                        {getInitials(message.sender.firstName, message.sender.lastName)}
                                    </Avatar>
                                    <Box sx={{
                                        width: 'fit-content',
                                        maxWidth: '70%'
                                    }}>
                                        <Paper
                                            elevation={1}
                                            sx={{
                                                p: 1.5,
                                                bgcolor: message.senderId === accounts[0]?.localAccountId
                                                    ? '#954d48'
                                                    : '#f5f5f5',
                                                color: message.senderId === accounts[0]?.localAccountId
                                                    ? 'white'
                                                    : '#333',
                                                borderRadius: message.senderId === accounts[0]?.localAccountId
                                                    ? '16px 16px 4px 16px'
                                                    : '16px 16px 16px 4px',
                                                width: '100%',
                                                wordBreak: 'break-word',
                                                overflowWrap: 'break-word'
                                            }}
                                        >
                                            <Typography
                                                variant="body1"
                                                sx={{
                                                    whiteSpace: 'pre-wrap',
                                                    overflowWrap: 'break-word',
                                                    wordBreak: 'break-word'
                                                }}
                                            >
                                                {message.content}
                                            </Typography>
                                        </Paper>
                                        <Box sx={{
                                            display: 'flex',
                                            gap: 1,
                                            mt: 0.5,
                                            justifyContent: message.senderId === accounts[0]?.localAccountId
                                                ? 'flex-end'
                                                : 'flex-start'
                                        }}>
                                            <Typography variant="caption" color="text.secondary">
                                                {message.sender.firstName} {message.sender.lastName}
                                            </Typography>
                                            <Typography variant="caption" color="text.secondary">
                                                {formatMessageTime(message.createdAt)}
                                            </Typography>
                                        </Box>
                                    </Box>
                                </Box>
                            </ListItem>
                        ))}
                        <div ref={messagesEndRef} />
                        {messages.length === 0 && (
                            <Box sx={{
                                display: 'flex',
                                justifyContent: 'center',
                                alignItems: 'center',
                                flexGrow: 1
                            }}>
                                <Typography color="text.secondary">
                                    No messages yet. Start the conversation!
                                </Typography>
                            </Box>
                        )}
                    </List>

                    {/* Message Input */}
                    <Box sx={{
                        p: 2,
                        borderTop: 1,
                        borderColor: 'divider',
                        bgcolor: 'white'
                    }}>
                        <Box sx={{ display: 'flex', gap: 1 }}>
                            <TextField
                                fullWidth
                                variant="outlined"
                                placeholder="Type a message..."
                                value={newMessage}
                                onChange={(e) => setNewMessage(e.target.value)}
                                onKeyPress={(e) => {
                                    if (e.key === 'Enter' && !e.shiftKey) {
                                        e.preventDefault();
                                        sendMessage();
                                    }
                                }}
                                multiline
                                maxRows={4}
                                sx={{
                                    '& .MuiOutlinedInput-root': {
                                        borderRadius: '24px',
                                        '&:hover .MuiOutlinedInput-notchedOutline': {
                                            borderColor: '#954d48'
                                        },
                                        '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                                            borderColor: '#954d48'
                                        }
                                    }
                                }}
                            />
                            <Tooltip title="Send message">
                                <IconButton
                                    onClick={sendMessage}
                                    disabled={!newMessage.trim()}
                                    sx={{
                                        alignSelf: 'flex-end',
                                        bgcolor: '#954d48',
                                        color: '#ffffff',
                                        '&:hover': {
                                            bgcolor: '#7a3d38'
                                        },
                                        '&.Mui-disabled': {
                                            bgcolor: 'action.disabledBackground',
                                            color: 'action.disabled'
                                        },
                                        borderRadius: '50%',
                                        width: 40,
                                        height: 40
                                    }}
                                >
                                    <SendIcon />
                                </IconButton>
                            </Tooltip>
                        </Box>
                    </Box>
                </>
            )}
        </Paper>
    </Container>
}
