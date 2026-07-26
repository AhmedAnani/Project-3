import React, { useEffect } from 'react';
import { useLocation } from 'wouter';
import { useAuth } from '@/contexts/AuthContext';
import { setGoogleTokenGetter } from '@/lib/api-client';

export default function AuthCallback() {
    const [, setLocation] = useLocation();
    const { setAuthData } = useAuth();

    useEffect(() => {
        const params = new URLSearchParams(window.location.search);

        console.log('Full URL:', window.location.href);          
        console.log('All params:', Object.fromEntries(params.entries())); 

        const accessToken = params.get('accessToken');
        const refreshToken = params.get('refreshToken');
        const userId = params.get('userId');
        const email = params.get('email');
        const fullName = params.get('fullName');
        const pictureUrl = params.get('pictureUrl');
        const role = params.get('role');
        const googleAccessToken = params.get('googleAccessToken');

        if (accessToken && userId && email && fullName) {
            // Store google token in session storage so it persists across page navigations
            if (googleAccessToken) {
                sessionStorage.setItem('ce_google_token', googleAccessToken);
            }
            // Wire up the getter so API interceptor sends X-Google-Token header automatically
            setGoogleTokenGetter(() => sessionStorage.getItem('ce_google_token'));

            const authData = {
                accessToken,
                refreshToken: refreshToken || '',
                accessTokenExpiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
                googleAccessToken: googleAccessToken || undefined,
                user: {
                    id: userId,
                    email,
                    fullName,
                    pictureUrl: pictureUrl || '',
                    role: role || 'User',
                    createdAt: new Date().toISOString(),
                },
            };
            setAuthData(authData);
            // Return to the page they were on
            const returnTo = sessionStorage.getItem('returnTo') || '/trips';
            sessionStorage.removeItem('returnTo');
            setLocation(returnTo);
        } else {
            setLocation('/login?auth_error=1');
        }
    }, [setLocation, setAuthData]);

    return (
        <div className="flex min-h-[50vh] items-center justify-center">
            <div className="flex flex-col items-center gap-4">
                <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
                <p className="text-muted-foreground font-medium">Completing sign in...</p>
            </div>
        </div>
    );
}