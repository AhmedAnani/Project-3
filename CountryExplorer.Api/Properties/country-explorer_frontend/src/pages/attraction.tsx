import React from 'react';
import { useRoute, Link } from 'wouter';
import { useGetAttractionDetails } from '@workspace/api-client-react';
import { ArrowLeft, MapPin, Tag } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';

export default function AttractionDetail() {
  const [, params] = useRoute('/attraction/:xid');
  const xid = params?.xid || '';

  const { data, isLoading, isError } = useGetAttractionDetails(xid, {
    query: { enabled: !!xid }
  });

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-12 space-y-8 max-w-4xl">
        <Skeleton className="w-32 h-10" />
        <Skeleton className="w-full h-[400px] rounded-2xl" />
        <Skeleton className="w-2/3 h-12" />
        <div className="space-y-2">
          <Skeleton className="w-full h-4" />
          <Skeleton className="w-full h-4" />
          <Skeleton className="w-3/4 h-4" />
        </div>
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="container mx-auto px-4 py-20 text-center space-y-6">
        <h2 className="text-3xl font-serif font-bold">Attraction Not Found</h2>
        <p className="text-muted-foreground">We couldn't load details for this attraction.</p>
        <Link href="/">
          <Button variant="outline"><ArrowLeft className="mr-2 h-4 w-4"/> Back Home</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-12 max-w-4xl">
      <Button 
        variant="ghost" 
        size="sm" 
        className="mb-6 -ml-3 text-muted-foreground hover:text-foreground"
        onClick={() => window.history.back()}
      >
        <ArrowLeft className="mr-2 h-4 w-4" /> Go Back
      </Button>

      {data.imageUrl && (
        <div className="w-full h-[300px] md:h-[500px] rounded-2xl overflow-hidden shadow-lg mb-10 border border-border bg-muted">
          <img 
            src={data.imageUrl} 
            alt={data.name} 
            className="w-full h-full object-cover"
            data-testid="img-attraction"
          />
        </div>
      )}

      <div className="space-y-6">
        <h1 className="text-4xl md:text-5xl font-serif font-bold text-foreground leading-tight" data-testid="text-attraction-name">
          {data.name || 'Unnamed Attraction'}
        </h1>

        <div className="flex flex-wrap gap-4 text-sm font-medium">
          <div className="flex items-center gap-1.5 px-3 py-1.5 bg-secondary text-secondary-foreground rounded-full">
            <MapPin className="h-4 w-4" />
            {data.latitude.toFixed(4)}, {data.longitude.toFixed(4)}
          </div>
          {data.categories?.slice(0, 3).map(cat => (
            <div key={cat} className="flex items-center gap-1.5 px-3 py-1.5 bg-muted text-muted-foreground rounded-full capitalize">
              <Tag className="h-4 w-4" />
              {cat.replace(/_/g, ' ')}
            </div>
          ))}
        </div>

        <div className="pt-6 border-t border-border/50">
          <h2 className="text-2xl font-serif font-bold text-primary mb-4">About this place</h2>
          <div className="prose prose-lg dark:prose-invert max-w-none text-muted-foreground">
            {data.description ? (
              <p data-testid="text-attraction-desc">{data.description}</p>
            ) : (
              <p className="italic">No detailed description available for this location.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
