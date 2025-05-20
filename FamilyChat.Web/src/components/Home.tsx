import { useMsal } from "@azure/msal-react";
import { loginRequest, apiRequest } from "../config/authConfig";
import { Button, Container, Typography, Box, Stack, Menu, MenuItem } from "@mui/material";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import LoginIcon from '@mui/icons-material/Login';
import MessageIcon from '@mui/icons-material/Message';
import { ChatWindow } from "./ChatWindow";
import ChatIcon from '@mui/icons-material/Chat';
import MenuIcon from '@mui/icons-material/Menu';
import React from "react";
import { theme } from "../theme/theme";


export const Home = () => {
    const { instance, accounts, inProgress } = useMsal();
    const [accessToken, setAccessToken] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [showChatWindow, setShowChatWindow] = useState<boolean>(false);
    const navigate = useNavigate();
    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const handleClick = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorEl(event.currentTarget);
    };
    const handleClose = () => {
        setAnchorEl(null);
    };

    const acquireToken = async () => {
        if (!accounts[0]) return;

        try {
            console.log(accounts[0]);
            const response = await instance.acquireTokenSilent({
                ...apiRequest,
                account: accounts[0]
            });
            setAccessToken(response.accessToken);
            setError(null);
        } catch (error) {
            console.error("Token acquisition failed:", error);
            setError("Failed to acquire token. Please try signing in again.");
        }
    };

    const handleLogin = async () => {
        try {
            await instance.loginPopup(loginRequest);
            // Token acquisition will be handled by the useEffect
        } catch (error) {
            console.error("Login failed:", error);
            setError("Login failed. Please try again.");
        }
    };

    // Acquire token when account is available
    useEffect(() => {
        if (inProgress === "none" && accounts[0]) {
            acquireToken();
        }
    }, [inProgress, accounts]);

    return (
        <Container maxWidth="sm">
            {/* Menu for medium and larger screens */}
            <Box sx={{
                position: 'fixed',
                left: '20px',
                top: '50%',
                transform: 'translateY(-50%)',
                zIndex: 1000,
                // display: { xs: 'none', md: 'block' }
            }}>
                <StackMenu />
            </Box>
    
            {showChatWindow && (
                <ChatWindow 
                    chatId="c4340764-f15f-48c9-9a4f-a28defe6db38"
                    show={showChatWindow}
                    setShow={setShowChatWindow}
                />
            )}

            {!showChatWindow && (
                <Box
                    sx={{
                    position: 'fixed',
                    bottom: 32,
                    right: 32,
                    zIndex: 1200,
                    bgcolor: '#954d48',
                    borderRadius: '50%',
                    width: 64,
                    height: 64,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    boxShadow: '0 4px 12px rgba(0,0,0,0.18)',
                    cursor: 'pointer',
                    '&:hover': {
                        bgcolor: '#7a3d38'
                    }
                    }}
                    onClick={() => setShowChatWindow(true)}
                >
                    <ChatIcon sx={{ color: '#fff', fontSize: 36 }} />
                </Box>
            )}
        </Container>
    );

    function StackMenu() {
        return (
            <Stack spacing={2}>
                {!accounts[0] && (
                    <Button
                        onClick={handleLogin}
                        disabled={inProgress !== "none"}
                        variant="contained"
                        startIcon={<LoginIcon />}
                        sx={{
                            bgcolor: '#954d48',
                            '&:hover': {
                                bgcolor: '#7a3d38'
                            },
                            borderRadius: '12px',
                            padding: '12px 24px',
                            minWidth: '180px',
                            boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)',
                            textTransform: 'none',
                            fontSize: '1rem'
                        }}
                    >
                        {inProgress === "login" ? "Signing in..." : "Sign In"}
                    </Button>
                )}
                <Button
                    variant="contained"
                    startIcon={<MessageIcon />}
                    onClick={() => setShowChatWindow(true)}
                    sx={{
                        bgcolor: '#954d48',
                        '&:hover': {
                            bgcolor: '#7a3d38'
                        },
                        borderRadius: '12px',
                        padding: '12px 24px',
                        minWidth: '180px',
                        boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)',
                        textTransform: 'none',
                        fontSize: '1rem'
                    }}
                >
                    Messages
                </Button>
            </Stack>
        );
    }
};
