import React from 'react';
import { Route, Switch, Router as WouterRouter } from 'wouter';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from '@/components/ui/toaster';
import { TooltipProvider } from '@/components/ui/tooltip';

import { AuthProvider } from '@/contexts/AuthContext';
import { Shell } from '@/components/layout/Shell';
import { PrivateRoute, AdminRoute } from '@/components/layout/ProtectedRoute';

import Home from '@/pages/home';
import Login from '@/pages/login';
import AuthCallback from '@/pages/auth-callback';
import CountryDetail from '@/pages/country';
import AttractionDetail from '@/pages/attraction';
import Trips from '@/pages/trips';
import Profile from '@/pages/profile';
import Admin from '@/pages/admin';
import NotFound from '@/pages/not-found';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function Router() {
  return (
    <Switch>
      <Route path="/" component={Home} />
      <Route path="/login" component={Login} />
      <Route path="/auth/callback" component={AuthCallback} />
      <Route path="/country/:name" component={CountryDetail} />
      <Route path="/attraction/:xid" component={AttractionDetail} />
      
      {/* Protected Routes */}
      <PrivateRoute path="/trips" component={Trips} />
      <PrivateRoute path="/profile" component={Profile} />
      
      {/* Admin Route */}
      <AdminRoute path="/admin" component={Admin} />

      <Route component={NotFound} />
    </Switch>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <TooltipProvider>
          <WouterRouter base={import.meta.env.BASE_URL.replace(/\/$/, '')}>
            <Shell>
              <Router />
            </Shell>
          </WouterRouter>
          <Toaster />
        </TooltipProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
