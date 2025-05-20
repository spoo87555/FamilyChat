import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Home } from './components/Home';
import { MsalProvider } from "@azure/msal-react";
import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig } from './config/authConfig';
import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";

const msalInstance = new PublicClientApplication(msalConfig);
const theme = createTheme();

function App() {
  return (
    <MsalProvider instance={msalInstance}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Router>
          <Routes>
            <Route path="/" element={<Home />} />
          </Routes>
        </Router>
      </ThemeProvider>
    </MsalProvider>
  );
}

export default App;
