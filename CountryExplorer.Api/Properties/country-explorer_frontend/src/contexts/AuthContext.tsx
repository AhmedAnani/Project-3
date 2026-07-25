import React, { createContext, useContext, useEffect, useState, ReactNode, useRef } from 'react';
import { UserProfile, AuthResponse, useAuthRefresh, useAuthLogout } from '@workspace/api-client-react';

export const tokenRef = { current: null as string | null };

interface AuthState {
  user: UserProfile | null;
  accessToken: string | null;
  refreshToken: string | null;
  accessTokenExpiresAt: string | null;
}

interface AuthContextType extends AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  login: () => void;
  logout: () => void;
  setAuthData: (data: AuthResponse) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [authState, setAuthState] = useState<AuthState>({
    user: null,
    accessToken: null,
    refreshToken: null,
    accessTokenExpiresAt: null,
  });
  const [isLoading, setIsLoading] = useState(true);

  const refreshMutation = useAuthRefresh();
  const logoutMutation = useAuthLogout();

  // Load from local storage on mount
  useEffect(() => {
    const stored = localStorage.getItem('ce_auth');
    if (stored) {
      try {
        const parsed = JSON.parse(stored) as AuthState;
        if (parsed.accessToken && parsed.user) {
          setAuthState(parsed);
          tokenRef.current = parsed.accessToken;
        }
      } catch (e) {
        localStorage.removeItem('ce_auth');
      }
    }
    setIsLoading(false);
  }, []);

  const setAuthData = (data: AuthResponse) => {
    const newState: AuthState = {
      user: data.user,
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      accessTokenExpiresAt: data.accessTokenExpiresAt,
    };
    setAuthState(newState);
    tokenRef.current = data.accessToken;
    localStorage.setItem('ce_auth', JSON.stringify(newState));
  };

  const login = () => {
    window.location.href = '/api/auth/login';
  };

  const logout = () => {
    if (authState.refreshToken) {
      logoutMutation.mutate({ data: { refreshToken: authState.refreshToken } });
    }
    setAuthState({
      user: null,
      accessToken: null,
      refreshToken: null,
      accessTokenExpiresAt: null,
    });
    tokenRef.current = null;
    localStorage.removeItem('ce_auth');
    window.location.href = '/login';
  };

  // Background token refresh
  useEffect(() => {
    if (!authState.accessToken || !authState.refreshToken || !authState.accessTokenExpiresAt) return;

    const checkRefresh = () => {
      const expiresAt = new Date(authState.accessTokenExpiresAt as string).getTime();
      const now = Date.now();
      const timeToExpiry = expiresAt - now;

      // Refresh if within 2 minutes of expiry
      if (timeToExpiry < 2 * 60 * 1000) {
        refreshMutation.mutate(
          { data: { refreshToken: authState.refreshToken as string } },
          {
            onSuccess: (data) => {
              setAuthData(data);
            },
            onError: () => {
              logout();
            }
          }
        );
      }
    };

    const interval = setInterval(checkRefresh, 60 * 1000); // Check every minute
    return () => clearInterval(interval);
  }, [authState, refreshMutation]);

  return (
    <AuthContext.Provider
      value={{
        ...authState,
        isAuthenticated: !!authState.accessToken,
        isLoading,
        login,
        logout,
        setAuthData,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
