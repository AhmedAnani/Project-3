import React, { useState } from 'react';
import { useLocation } from 'wouter';
import { Search, MapPin, Navigation } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

const POPULAR_DESTINATIONS = [
  { name: 'Japan', code: 'JP', image: 'https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e?q=80&w=800&auto=format&fit=crop' },
  { name: 'Italy', code: 'IT', image: 'https://images.unsplash.com/photo-1516483638261-f40af5eba321?q=80&w=800&auto=format&fit=crop' },
  { name: 'Iceland', code: 'IS', image: 'https://images.unsplash.com/photo-1476610182048-b716b8518aae?q=80&w=800&auto=format&fit=crop' },
  { name: 'Morocco', code: 'IS', image: 'https://images.unsplash.com/photo-1539020140153-e479b8c22e70?q=80&w=800&auto=format&fit=crop' },
];

export default function Home() {
  const [, setLocation] = useLocation();
  const [query, setQuery] = useState('');

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (query.trim()) {
      setLocation(`/country/${encodeURIComponent(query.trim())}`);
    }
  };

  return (
    <div className="w-full">
      {/* Hero Section */}
      <section className="relative w-full h-[70vh] min-h-[500px] flex items-center justify-center overflow-hidden">
        {/* Abstract patterned background - simple gradient approach for now */}
        <div className="absolute inset-0 bg-gradient-to-br from-primary/90 to-primary/80 z-10" />
        <div 
          className="absolute inset-0 z-0 opacity-40 bg-cover bg-center" 
          style={{ backgroundImage: 'url(https://images.unsplash.com/photo-1488646953014-85cb44e25828?q=80&w=2000&auto=format&fit=crop)' }}
        />
        
        <div className="relative z-20 container mx-auto px-4 text-center text-primary-foreground max-w-3xl">
          <h1 className="text-5xl md:text-7xl font-serif font-bold mb-6 drop-shadow-md">
            Discover the World
          </h1>
          <p className="text-lg md:text-xl mb-10 opacity-90 font-medium">
            Explore countries, discover hidden attractions, and plan your next adventure with personal travel intelligence.
          </p>
          
          <form onSubmit={handleSearch} className="relative w-full max-w-2xl mx-auto shadow-2xl">
            <div className="relative flex items-center">
              <Search className="absolute left-4 h-6 w-6 text-muted-foreground" />
              <Input
                type="text"
                placeholder="Where to next? (e.g., Japan, France, Brazil)"
                className="w-full h-16 pl-14 pr-32 text-lg rounded-full bg-card text-card-foreground border-0 focus-visible:ring-accent"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                data-testid="input-country-search"
              />
              <Button 
                type="submit" 
                className="absolute right-2 h-12 rounded-full px-6 bg-accent hover:bg-accent/90 text-accent-foreground font-semibold"
                data-testid="button-search-submit"
              >
                Explore
              </Button>
            </div>
          </form>
        </div>
      </section>

      {/* Popular Destinations */}
      <section className="py-20 container mx-auto px-4">
        <div className="flex items-center justify-between mb-10">
          <div>
            <h2 className="text-3xl font-serif font-bold text-foreground">Trending Destinations</h2>
            <p className="text-muted-foreground mt-2">Curated locations for your bucket list</p>
          </div>
          <Navigation className="text-accent h-8 w-8 opacity-50" />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {POPULAR_DESTINATIONS.map((dest) => (
            <div 
              key={dest.name}
              className="group relative h-80 rounded-2xl overflow-hidden cursor-pointer shadow-lg hover-elevate transition-all duration-300"
              onClick={() => setLocation(`/country/${encodeURIComponent(dest.name)}`)}
              data-testid={`card-popular-${dest.name}`}
            >
              <div 
                className="absolute inset-0 bg-cover bg-center transition-transform duration-700 group-hover:scale-105"
                style={{ backgroundImage: `url(${dest.image})` }}
              />
              <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/30 to-transparent" />
              <div className="absolute bottom-0 left-0 p-6 w-full">
                <h3 className="text-2xl font-serif font-bold text-white mb-1 flex items-center gap-2">
                  <MapPin className="h-5 w-5 text-accent" />
                  {dest.name}
                </h3>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Value Prop */}
      <section className="bg-secondary/50 py-24">
        <div className="container mx-auto px-4">
          <div className="max-w-4xl mx-auto text-center space-y-8">
            <h2 className="text-4xl font-serif font-bold text-primary">Your Personal Travel Atlas</h2>
            <p className="text-xl text-muted-foreground">
              Country Explorer is more than just a search engine. It's an intelligent companion that brings geographical data, attraction discovery, and budget planning into one cohesive, beautifully crafted space.
            </p>
          </div>
        </div>
      </section>
    </div>
  );
}
