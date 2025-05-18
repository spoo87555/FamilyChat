import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
    palette: {
        primary: {
            main: '#954d48',
            light: '#b36d68',
            dark: '#7a3d38',
            contrastText: '#ffffff',
        },
        secondary: {
            main: '#4a6b8a',
            light: '#6b8baa',
            dark: '#3a556a',
            contrastText: '#ffffff',
        },
        background: {
            default: '#f5f5f5',
            paper: '#ffffff',
        },
    },
    components: {
        MuiButton: {
            styleOverrides: {
                root: {
                    borderRadius: 8,
                    textTransform: 'none',
                    fontWeight: 600,
                },
            },
        },
        MuiTextField: {
            styleOverrides: {
                root: {
                    '& .MuiOutlinedInput-root': {
                        borderRadius: 8,
                    },
                },
            },
        },
        MuiPaper: {
            styleOverrides: {
                root: {
                    borderRadius: 12,
                },
            },
        },
        MuiIconButton: {
            styleOverrides: {
                root: {
                    '&.MuiIconButton-colorPrimary': {
                        color: '#954d48',
                        '&:hover': {
                            color: '#7a3d38',
                        },
                    },
                },
            },
        },
    },
}); 