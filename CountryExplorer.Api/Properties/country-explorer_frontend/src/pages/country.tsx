import React, { useState } from 'react';
import { useRoute, Link } from 'wouter';
import { 
  useGetCountryDetails, 
  useGetBudgetEstimate 
} from '@workspace/api-client-react';
import { MapPin, Users, Globe, Building2, Coins, ArrowLeft, Languages } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Skeleton } from '@/components/ui/skeleton';
import { Separator } from '@/components/ui/separator';
import { Card, CardContent } from '@/components/ui/card';

export default function CountryDetail() {
  const [, params] = useRoute('/country/:name');
  const countryName = params?.name ? decodeURIComponent(params.name) : '';

  const { data, isLoading, isError } = useGetCountryDetails(countryName, {
    query: { enabled: !!countryName }
  });

  const [budgetAmount, setBudgetAmount] = useState<string>('1000');
  const [baseCurrency, setBaseCurrency] = useState<string>('USD');

  // We only fire the budget query if we have a target currency (from the country data)
  const targetCurrency = data?.country?.currencies?.[0]?.code;
  const shouldFetchBudget = !!(countryName && budgetAmount && baseCurrency && targetCurrency);

  const { data: budgetData, isLoading: budgetLoading } = useGetBudgetEstimate(
    countryName, 
    { amount: Number(budgetAmount) || 0, fromCurrency: baseCurrency },
    { query: { enabled: shouldFetchBudget } }
  );

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8 space-y-8">
        <div className="flex gap-8">
          <Skeleton className="w-32 h-20 rounded-xl" />
          <div className="space-y-4 flex-1">
            <Skeleton className="w-2/3 h-12" />
            <Skeleton className="w-1/3 h-6" />
          </div>
        </div>
        <div className="grid md:grid-cols-3 gap-8">
          <Skeleton className="md:col-span-2 h-96 rounded-xl" />
          <Skeleton className="h-96 rounded-xl" />
        </div>
      </div>
    );
  }

  if (isError || !data?.country) {
    return (
      <div className="container mx-auto px-4 py-20 text-center space-y-6">
        <h2 className="text-3xl font-serif font-bold">Country Not Found</h2>
        <p className="text-muted-foreground">We couldn't find data for "{countryName}". Please try a different search.</p>
        <Link href="/">
          <Button variant="outline" data-testid="button-back-home"><ArrowLeft className="mr-2 h-4 w-4"/> Back to Search</Button>
        </Link>
      </div>
    );
  }

  const { country, attractions } = data;

  return (
    <div className="min-h-screen bg-background">
      {/* Header section with Flag */}
      <div className="bg-card border-b border-border/50">
        <div className="container mx-auto px-4 py-12">
          <Link href="/">
            <Button variant="ghost" size="sm" className="mb-6 -ml-3 text-muted-foreground hover:text-foreground">
              <ArrowLeft className="mr-2 h-4 w-4" /> Back
            </Button>
          </Link>
          
          <div className="flex flex-col md:flex-row gap-8 items-start md:items-center">
            <div className="shrink-0 rounded-xl overflow-hidden shadow-md border border-border/50 bg-white">
              <img 
                src={country.flagUrl} 
                alt={`Flag of ${country.name}`} 
                className="w-48 h-auto object-cover"
                data-testid="img-country-flag"
              />
            </div>
            <div>
              <h1 className="text-5xl font-serif font-bold text-foreground mb-2 tracking-tight" data-testid="text-country-name">
                {country.name}
              </h1>
              <p className="text-xl text-muted-foreground font-medium">
                {country.officialName}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="container mx-auto px-4 py-12">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-10">
          
          {/* Main Info Column */}
          <div className="lg:col-span-2 space-y-12">
            
            <section>
              <h2 className="text-2xl font-serif font-bold mb-6 text-primary border-b border-border pb-2">Fact File</h2>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <InfoItem icon={<Building2 />} label="Capital" value={country.capital || 'N/A'} />
                <InfoItem icon={<Globe />} label="Region" value={`${country.region} (${country.subregion})`} />
                <InfoItem icon={<Users />} label="Population" value={country.population?.toLocaleString()} />
                <InfoItem icon={<Languages />} label="Languages" value={country.languages?.join(', ') || 'N/A'} />
                <InfoItem 
                  icon={<Coins />} 
                  label="Currencies" 
                  value={country.currencies?.map(c => `${c.name} (${c.symbol})`).join(', ') || 'N/A'} 
                />
              </div>
            </section>

            <section>
              <h2 className="text-2xl font-serif font-bold mb-6 text-primary border-b border-border pb-2">Notable Attractions</h2>
              {attractions?.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  {attractions.map(attr => (
                    <Link key={attr.id} href={`/attraction/${attr.id}`}>
                      <Card className="hover:border-accent hover:shadow-md transition-colors cursor-pointer h-full" data-testid={`card-attraction-${attr.id}`}>
                        <CardContent className="p-4 flex flex-col h-full justify-between gap-2">
                          <div>
                            <h3 className="font-bold text-lg leading-tight line-clamp-2">{attr.name}</h3>
                            <p className="text-sm text-muted-foreground capitalize mt-1">
                              {attr.category.replace(/_/g, ' ')}
                            </p>
                          </div>
                          {attr.distanceFromCenterMeters && (
                            <p className="text-xs font-medium text-accent flex items-center gap-1 mt-2">
                              <MapPin className="h-3 w-3" />
                              {(attr.distanceFromCenterMeters / 1000).toFixed(1)} km from center
                            </p>
                          )}
                        </CardContent>
                      </Card>
                    </Link>
                  ))}
                </div>
              ) : (
                <p className="text-muted-foreground italic">No attractions found for this destination.</p>
              )}
            </section>
          </div>

          {/* Sidebar / Tools */}
          <div className="space-y-8">
            <Card className="bg-secondary/30 border-secondary-border shadow-sm">
              <CardContent className="p-6">
                <h3 className="text-xl font-serif font-bold mb-4 flex items-center gap-2 text-primary">
                  <Coins className="h-5 w-5" />
                  Budget Estimator
                </h3>
                <p className="text-sm text-muted-foreground mb-6">
                  Get a quick sense of local currency value against your base budget.
                </p>
                
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label>Amount</Label>
                      <Input 
                        type="number" 
                        value={budgetAmount} 
                        onChange={(e) => setBudgetAmount(e.target.value)}
                        data-testid="input-budget-amount"
                      />
                    </div>
                    <div className="space-y-2">
                      <Label>From</Label>
                      <Input 
                        type="text" 
                        value={baseCurrency} 
                        onChange={(e) => setBaseCurrency(e.target.value.toUpperCase())}
                        maxLength={3}
                        data-testid="input-budget-currency"
                      />
                    </div>
                  </div>

                  <Separator className="my-4" />

                  {budgetLoading ? (
                    <div className="animate-pulse flex items-center justify-center py-4 text-muted-foreground text-sm">
                      Calculating...
                    </div>
                  ) : budgetData ? (
                    <div className="bg-card p-4 rounded-xl border border-border text-center space-y-1">
                      <p className="text-sm text-muted-foreground">Estimated Conversion</p>
                      <p className="text-3xl font-serif font-bold text-accent" data-testid="text-budget-result">
                        {budgetData.convertedAmount.toLocaleString(undefined, { maximumFractionDigits: 2 })}
                        <span className="text-base ml-1">{budgetData.toCurrency}</span>
                      </p>
                      <p className="text-xs text-muted-foreground mt-2">
                        Rate: 1 {budgetData.fromCurrency} = {budgetData.exchangeRate.toFixed(2)} {budgetData.toCurrency}
                      </p>
                    </div>
                  ) : (
                    <div className="bg-muted/50 p-4 rounded-xl border border-border border-dashed text-center text-sm text-muted-foreground">
                      Enter amount to calculate
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
            
            {/* Call to action for users */}
            <Card className="bg-primary text-primary-foreground border-none">
              <CardContent className="p-6 text-center space-y-4">
                <h3 className="font-serif font-bold text-xl">Planning a trip?</h3>
                <p className="text-sm opacity-90">Save this country to your personal trip list to keep track of your itinerary.</p>
                <Link href="/trips">
                  <Button variant="secondary" className="w-full font-bold" data-testid="button-plan-trip">
                    Go to My Trips
                  </Button>
                </Link>
              </CardContent>
            </Card>

          </div>
        </div>
      </div>
    </div>
  );
}

function InfoItem({ icon, label, value }: { icon: React.ReactNode, label: string, value: string | number }) {
  return (
    <div className="flex gap-4 p-4 rounded-xl bg-card border border-border/50 hover:border-border transition-colors">
      <div className="mt-1 text-accent flex-shrink-0">
        {React.cloneElement(icon as React.ReactElement, { className: 'h-6 w-6' })}
      </div>
      <div>
        <p className="text-sm font-medium text-muted-foreground">{label}</p>
        <p className="text-lg font-bold text-foreground" data-testid={`text-info-${label.toLowerCase()}`}>{value}</p>
      </div>
    </div>
  );
}
