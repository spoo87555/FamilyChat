import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../config/authConfig";
import { Button, Container, Typography, Box } from "@mui/material";

export const Home = () => {
    const { instance, accounts } = useMsal();

    const handleLogin = () => {
        instance.loginPopup(loginRequest).catch(error => {
            console.error("Login failed:", error);
        });
    };

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
                    >
                        Sign In
                    </Button>
                ) : (
                    <Box sx={{ mt: 2 }}>
                        <Typography variant="h6" gutterBottom>
                            Signed in as: {accounts[0].username}
                        </Typography>
                        <Typography variant="body1" sx={{ 
                            wordBreak: 'break-all',
                            backgroundColor: '#f5f5f5',
                            padding: 2,
                            borderRadius: 1,
                            mt: 2
                        }}>
                            ID Token: {accounts[0].idToken}
                        </Typography>
                    </Box>
                )}
            </Box>
        </Container>
    );
}; 