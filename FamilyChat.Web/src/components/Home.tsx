import { useMsal } from "@azure/msal-react";
import { loginRequest, apiRequest } from "../config/authConfig";
import { Button, Container, Typography, Box } from "@mui/material";
import { useState, useEffect } from "react";

interface UserDetails {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
}

export const Home = () => {
    const { instance, accounts, inProgress } = useMsal();
    const [accessToken, setAccessToken] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [userDetails, setUserDetails] = useState<UserDetails | null>(null);

    const acquireToken = async () => {
        if (!accounts[0]) return;

        try {
            const response = await instance.acquireTokenSilent({
                ...apiRequest,
                account: accounts[0]
            });
            setAccessToken(response.accessToken);
            setError(null);
            console.log("Access Token acquired:", response.accessToken);
        } catch (error) {
            console.error("Token acquisition failed:", error);
            setError("Failed to acquire token. Please try signing in again.");
        }
    };

    const fetchUserDetails = async () => {
        if (!accessToken) return;

        try {
            const response = await fetch('https://localhost:7296/api/Users/c47999f9-1a5c-44c8-a94e-08bd8f1dc71e', {
                headers: {
                    'Authorization': `Bearer ${accessToken}`
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            setUserDetails(data);
            setError(null);
        } catch (error) {
            console.error("Failed to fetch user details:", error);
            setError("Failed to fetch user details. Please try again.");
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

    // Fetch user details when access token is available
    useEffect(() => {
        if (accessToken) {
            fetchUserDetails();
        }
    }, [accessToken]);

    return (
        <Container maxWidth="sm">
            <Box sx={{ mt: 4, textAlign: 'center' }}>
                <Typography variant="h4" component="h1" gutterBottom>
                    Welcome to Family Chat
                </Typography>
                
                {!accounts[0] ? (
                    <Button 
                        variant="contained" 
                        color="primary" 
                        onClick={handleLogin}
                        size="large"
                        disabled={inProgress !== "none"}
                    >
                        {inProgress === "login" ? "Signing in..." : "Sign In"}
                    </Button>
                ) : (
                    <Box sx={{ mt: 2 }}>
                        <Typography variant="h6" gutterBottom>
                            Signed in as: {accounts[0].username}
                        </Typography>
                        {error && (
                            <Typography color="error" sx={{ mt: 2 }}>
                                {error}
                            </Typography>
                        )}
                        <Typography 
                            variant="body1" 
                            sx={{ 
                                wordBreak: 'break-word',
                                backgroundColor: '#f5f5f5',
                                padding: 2,
                                borderRadius: 1,
                                mt: 2
                            }}
                        >
                            Access Token: {accessToken ?? "Loading..."}
                        </Typography>
                        {userDetails && (
                            <Box sx={{ mt: 2, p: 2, backgroundColor: '#f5f5f5', borderRadius: 1 }}>
                                <Typography variant="h6" gutterBottom>User Details:</Typography>
                                <Typography>ID: {userDetails.id}</Typography>
                                <Typography>Email: {userDetails.email}</Typography>
                                <Typography>Name: {userDetails.firstName} {userDetails.lastName}</Typography>
                            </Box>
                        )}
                        <Button 
                            variant="outlined" 
                            onClick={acquireToken}
                            sx={{ mt: 2 }}
                            disabled={inProgress !== "none"}
                        >
                            Refresh Token
                        </Button>
                    </Box>
                )}
            </Box>
        </Container>
    );
};
