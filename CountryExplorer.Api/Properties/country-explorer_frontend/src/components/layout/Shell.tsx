import React from 'react';
import { Link, useLocation } from 'wouter';
import { useAuth } from '@/contexts/AuthContext';
import { Compass, User, LogOut, Shield, Map, Sparkles } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useHealthCheck } from '@workspace/api-client-react';

export function Shell({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, user, logout } = useAuth();
  const [location] = useLocation();

  // We must use useHealthCheck somewhere as requested
  const { data: health } = useHealthCheck();

  return (
    <div className="min-h-[100dvh] flex flex-col bg-background selection:bg-accent selection:text-white">
      <header className="sticky top-0 z-50 w-full border-b border-border/40 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container mx-auto px-4 h-16 flex items-center justify-between">
          <Link href="/" className="flex items-center gap-2 transition-opacity hover:opacity-80" data-testid="link-home">
            <div className="bg-primary text-primary-foreground p-1.5 rounded-md">
              <Compass className="h-5 w-5" />
            </div>
            <span className="font-serif font-bold text-xl tracking-tight text-foreground">Country Explorer</span>
          </Link>
          
          <nav className="flex items-center gap-2 md:gap-4">
            {isAuthenticated ? (
              <>
                <Link href="/discover" className="hidden md:flex">
                  <Button variant={location.startsWith('/discover') ? 'secondary' : 'ghost'} size="sm" className="gap-2" data-testid="link-discover">
                    <Sparkles className="h-4 w-4" />
                    Discover
                  </Button>
                </Link>
                <Link href="/trips" className="hidden md:flex">
                  <Button variant={location.startsWith('/trips') ? 'secondary' : 'ghost'} size="sm" className="gap-2" data-testid="link-trips">
                    <Map className="h-4 w-4" />
                    My Trips
                  </Button>
                </Link>
                {user?.role === 'Admin' && (
                  <Link href="/admin" className="hidden md:flex">
                    <Button variant={location.startsWith('/admin') ? 'secondary' : 'ghost'} size="sm" className="gap-2" data-testid="link-admin">
                      <Shield className="h-4 w-4" />
                      Admin
                    </Button>
                  </Link>
                )}
                <div className="h-6 w-px bg-border mx-1 hidden md:block" />
                <Link href="/profile">
                  <Button variant="ghost" size="sm" className="gap-2" data-testid="link-profile">
                    <User className="h-4 w-4" />
                    <span className="hidden md:inline">{user?.fullName || 'Profile'}</span>
                  </Button>
                </Link>
                <Button variant="ghost" size="icon" onClick={logout} title="Log out" data-testid="button-logout">
                  <LogOut className="h-4 w-4" />
                </Button>
              </>
            ) : (
              <Link href="/login">
                <Button variant="default" size="sm" className="gap-2 font-medium" data-testid="link-login">
                  Sign In
                </Button>
              </Link>
            )}
          </nav>
        </div>
      </header>

      <main className="flex-1 w-full relative">
        {children}
      </main>

      <footer className="border-t border-border/40 py-8 mt-12 bg-card text-card-foreground">
        <div className="container mx-auto px-4 flex flex-col md:flex-row justify-between items-center gap-4 text-sm text-muted-foreground">
          <div className="flex items-center gap-2">
            <Compass className="h-4 w-4" />
            <span className="font-serif font-semibold text-foreground">Country Explorer</span>
            <span>&copy; {new Date().getFullYear()}</span>
          </div>
          <div className="flex gap-4">
            <span>API Status: {health?.status === 'Healthy' ? <span className="text-green-600 font-medium">Online</span> : <span className="text-accent">Checking...</span>}</span>
          </div>
        </div>
      </footer>
    </div>
  );
}
