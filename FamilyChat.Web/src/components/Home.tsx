import { useMsal } from "@azure/msal-react";
import { loginRequest, apiRequest } from "../config/authConfig";
import { Button, Container, Typography, Box } from "@mui/material";
import { useState, useEffect } from "react";

export const Home = () => {
    const { instance, accounts, inProgress } = useMsal();
    const [accessToken, setAccessToken] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

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
