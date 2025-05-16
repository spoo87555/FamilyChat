import type { PopupRequest } from "@azure/msal-browser";

// Azure AD B2C configuration
export const msalConfig = {
    auth: {
        clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
        authority: `https://${import.meta.env.VITE_AZURE_TENANT_NAME}.b2clogin.com/${import.meta.env.VITE_AZURE_TENANT_NAME}.onmicrosoft.com/${import.meta.env.VITE_AZURE_POLICY_NAME}`,
        knownAuthorities: [`${import.meta.env.VITE_AZURE_TENANT_NAME}.b2clogin.com`],
        redirectUri: window.location.origin,
    },
    cache: {
        cacheLocation: "sessionStorage",
        storeAuthStateInCookie: false,
    }
} as const;

// Add here scopes for id token to be used at MS Identity Platform endpoints.
export const loginRequest: PopupRequest = {
    scopes: [
        "openid", 
        "profile", 
        "email"
    ]
};

// API scope for accessing the backend
export const apiRequest: PopupRequest = {
    scopes: ["https://touinafamily.onmicrosoft.com/chat-api/chat.readwrite"]
}; 