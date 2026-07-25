import React from 'react';
import { Link } from 'wouter';
import { Button } from '@/components/ui/button';
import { Compass } from 'lucide-react';

export default function NotFound() {
  return (
    <div className="min-h-[calc(100vh-16rem)] flex items-center justify-center p-4 text-center">
      <div className="max-w-md space-y-6">
        <div className="inline-flex items-center justify-center p-4 bg-muted rounded-full text-muted-foreground mb-4">
          <Compass className="h-12 w-12 opacity-50" />
        </div>
        <h1 className="text-5xl font-serif font-bold text-foreground">404</h1>
        <h2 className="text-2xl font-serif font-bold text-muted-foreground">Lost off the map</h2>
        <p className="text-muted-foreground">
          The page you are looking for doesn't exist or has been moved.
        </p>
        <div className="pt-4">
          <Link href="/">
            <Button size="lg" className="font-bold">
              Return Home
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
