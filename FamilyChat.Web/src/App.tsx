import { MsalProvider } from "@azure/msal-react";
import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig } from "./config/authConfig";
import { Home } from "./components/Home";
import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";

const msalInstance = new PublicClientApplication(msalConfig);
const theme = createTheme();

function App() {
  return (
    <MsalProvider instance={msalInstance}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Home />
      </ThemeProvider>
    </MsalProvider>
  );
}

export default App;
