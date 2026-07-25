import React, { createContext, useContext, useState, useEffect } from 'react';
import { apiClient, setAuthTokenGetter, setGoogleTokenGetter } from '@/lib/api-client';

export interface User {
    id: string;
    email: string;
    fullName: string;
    pictureUrl?: string;
    role?: string;
    createdAt: string;
}

export interface AuthResponse {
    accessToken: string;
    refreshToken?: string;
    accessTokenExpiresAt?: string;
    googleAccessToken?: string;  
    user: User;
}
interface AuthContextType {
    user: User | null;
    token: string | null;
    googleToken: string | null;
    isAuthenticated: boolean;
    login: () => void;
    setAuthData: (data: AuthResponse) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<User | null>(null);
    const [token, setToken] = useState<string | null>(null);
    const [googleToken, setGoogleToken] = useState<string | null>(null);
    useEffect(() => {
        const storedUser = localStorage.getItem('user');
        const storedToken = localStorage.getItem('token');
        const storedGoogleToken = localStorage.getItem('googleToken');

        if (storedUser && storedToken) {
            const parsed = JSON.parse(storedUser);
            setUser(parsed);
            setToken(storedToken);
            if (storedGoogleToken) {
                setGoogleToken(storedGoogleToken);
            }

            setAuthTokenGetter(() => storedToken);
            setGoogleTokenGetter(() => storedGoogleToken);
        }
    }, []);

    const login = () => {
        sessionStorage.setItem('returnTo', window.location.pathname);
        window.location.href = '/api/auth/google';
    };

    const setAuthData = (authData: AuthResponse) => {
        const { accessToken, googleAccessToken, user } = authData;
        setUser(user);
        setToken(accessToken);

        if (googleAccessToken) {
            setGoogleToken(googleAccessToken);
            localStorage.setItem('googleToken', googleAccessToken);
        }

        localStorage.setItem('user', JSON.stringify(user));
        localStorage.setItem('token', accessToken);

        setAuthTokenGetter(() => accessToken);
        setGoogleTokenGetter(() => googleAccessToken || null);
    };

    const logout = () => {
        setUser(null);
        setToken(null);
        setGoogleToken(null);

        localStorage.removeItem('user');
        localStorage.removeItem('token');
        localStorage.removeItem('googleToken');

        setAuthTokenGetter(() => null);
        setGoogleTokenGetter(() => null);
    };

    return (
        <AuthContext.Provider value={{
            user,
            token,
            googleToken,
            isAuthenticated: !!user && !!token,  
            login,
            setAuthData,
            logout
        }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within AuthProvider');
    return ctx;
};