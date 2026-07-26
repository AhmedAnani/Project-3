import React, { useState } from 'react';
import { useLocation } from 'wouter';
import {
    Sparkles, Laptop, Mountain, Sun, BookOpen,
    Waves, Building2, Landmark, Snowflake, MapPin,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { useToast } from '@/hooks/use-toast';
import { Checkbox } from '@/components/ui/checkbox';
import { useAuth } from '@/contexts/AuthContext';
import { useQueryClient } from '@tanstack/react-query';
import {
    useGetRecommendations,
    useSaveToTrips,
    getGetTripsQueryKey,
    type ScoreBreakdownDto,
} from '@workspace/api-client-react';

// ─── Constants ────────────────────────────────────────────────────────────────

const PURPOSES = [
    { value: 0, label: 'Workation', description: 'Remote work + exploration', icon: Laptop },
    { value: 1, label: 'Adventure', description: 'Thrills & outdoor activities', icon: Mountain },
    { value: 2, label: 'Vacation', description: 'Relax & unwind', icon: Sun },
    { value: 3, label: 'Culture & History', description: 'Museums, heritage & art', icon: BookOpen },
] as const;

const VIBE_TAGS = [
    { value: 1, label: 'Coastal', icon: Waves },
    { value: 2, label: 'Mountains', icon: Mountain },
    { value: 4, label: 'Historic', icon: Landmark },
    { value: 8, label: 'Urban', icon: Building2 },
    { value: 16, label: 'Warm Climate', icon: Sun },
    { value: 32, label: 'Cold Climate', icon: Snowflake },
] as const;

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Converts a 2-letter country code to a flag emoji */
const toFlag = (cc: string) =>
    cc.toUpperCase().split('').map(c => String.fromCodePoint(c.charCodeAt(0) + 127397)).join('');

const budgetLabel = (v: number) => {
    if (v <= 30) return 'Backpacker 🎒';
    if (v <= 60) return 'Mid-range ✈️';
    if (v <= 80) return 'Comfortable 🏨';
    return 'Luxury 💎';
};

const scoreColor = (v: number) =>
    v >= 70 ? '#22c55e' : v >= 50 ? '#f59e0b' : '#ef4444';

// ─── Sub-components ───────────────────────────────────────────────────────────

function ScoreRing({ score, size = 80 }: { score: number; size?: number }) {
    const r = (size - 12) / 2;
    const circ = 2 * Math.PI * r;
    const offset = circ * (1 - Math.max(0, Math.min(100, score)) / 100);
    const color = scoreColor(score);
    return (
        <svg width={size} height={size} style={{ transform: 'rotate(-90deg)' }}>
            <circle cx={size / 2} cy={size / 2} r={r} fill="none" stroke="rgba(255,255,255,0.2)" strokeWidth={8} />
            <circle
                cx={size / 2} cy={size / 2} r={r} fill="none"
                stroke={color} strokeWidth={8}
                strokeDasharray={circ}
                strokeDashoffset={offset}
                strokeLinecap="round"
                style={{ transition: 'stroke-dashoffset 0.9s ease' }}
            />
        </svg>
    );
}

function ScoreBar({ label, value }: { label: string; value: number }) {
    const color = scoreColor(value);
    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 3 }}>
                <span style={{ fontSize: 12, color: 'hsl(var(--muted-foreground))' }}>{label}</span>
                <span style={{ fontSize: 12, fontWeight: 700, color }}>{Math.round(value)}</span>
            </div>
            <div style={{ height: 5, background: 'hsl(var(--border))', borderRadius: 3, overflow: 'hidden' }}>
                <div
                    style={{
                        height: '100%',
                        width: `${value}%`,
                        background: color,
                        borderRadius: 3,
                        transition: 'width 0.9s ease',
                    }}
                />
            </div>
        </div>
    );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function Discover() {
    const [, setLocation] = useLocation();
    const { toast } = useToast();
    const queryClient = useQueryClient();
    const { googleAccessToken } = useAuth();

    // Form state
    const [purpose, setPurpose] = useState<number>(2);   // default: Vacation
    const [maxBudget, setMaxBudget] = useState(80);
    const [vibeTags, setVibeTags] = useState<number>(0);
    const [topN, setTopN] = useState('5');

    // Results
    const [results, setResults] = useState<ScoreBreakdownDto[] | null>(null);

    // Save dialog
    const [saveTarget, setSaveTarget] = useState<ScoreBreakdownDto | null>(null);
    const [saveStartDate, setSaveStartDate] = useState('');
    const [saveEndDate, setSaveEndDate] = useState('');
    const [saveSyncGoogle, setSaveSyncGoogle] = useState(false);

    const { mutate: getRecommendations, isPending: isSearching } = useGetRecommendations();
    const { mutate: saveToTrips, isPending: isSaving } = useSaveToTrips();

    const toggleVibeTag = (value: number) =>
        setVibeTags(prev => (prev & value) ? (prev & ~value) : (prev | value));

    const handleSearch = () => {
        getRecommendations(
            {
                purpose,
                maxBudget,
                preferredTags: vibeTags || undefined,
                topN: parseInt(topN, 10),
            },
            {
                onSuccess: data => {
                    setResults(data);
                    if (data.length === 0) {
                        toast({ title: 'No results', description: 'Try adjusting your preferences.', variant: 'destructive' });
                    }
                },
                onError: () => {
                    toast({ title: 'Error', description: 'Failed to fetch recommendations.', variant: 'destructive' });
                },
            }
        );
    };

    const handleSaveOpen = (result: ScoreBreakdownDto) => {
        setSaveTarget(result);
        setSaveStartDate('');
        setSaveEndDate('');
        setSaveSyncGoogle(false);
    };

    const handleSaveConfirm = () => {
        if (!saveTarget) return;
        const cityName = saveTarget.cityName;
        saveToTrips(
            {
                destinationId: saveTarget.destinationId,
                startDate: saveStartDate ? new Date(saveStartDate).toISOString() : undefined,
                endDate: saveEndDate ? new Date(saveEndDate).toISOString() : undefined,
                syncWithGoogleCalendar: saveSyncGoogle,
            },
            {
                onSuccess: () => {
                    queryClient.invalidateQueries({ queryKey: getGetTripsQueryKey() });
                    setSaveTarget(null);
                    toast({ title: '🌍 Trip saved!', description: `${cityName} has been added to your bucket list.` });
                    setLocation('/trips');
                },
                onError: () => {
                    toast({ title: 'Error', description: 'Failed to save trip. Please try again.', variant: 'destructive' });
                },
            }
        );
    };

    const qualifiedCount = results?.filter(r => !r.isDisqualified).length ?? 0;
    const disqualifiedCount = results?.filter(r => r.isDisqualified).length ?? 0;

    return (
        <div className="w-full">

            {/* ── Hero ─────────────────────────────────────────────────────── */}
            <section style={{
                background: 'linear-gradient(135deg, hsl(var(--primary)) 0%, hsl(var(--primary)/0.75) 55%, hsl(var(--accent)) 100%)',
                padding: '72px 16px 56px',
                textAlign: 'center',
                color: 'hsl(var(--primary-foreground))',
                position: 'relative',
                overflow: 'hidden',
            }}>
                {/* dotted pattern overlay */}
                <div style={{
                    position: 'absolute', inset: 0, pointerEvents: 'none',
                    backgroundImage: 'radial-gradient(circle, rgba(255,255,255,0.12) 1px, transparent 1px)',
                    backgroundSize: '30px 30px',
                }} />
                <div style={{ position: 'relative', maxWidth: 640, margin: '0 auto' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 10, marginBottom: 18 }}>
                        <Sparkles style={{ width: 24, height: 24, opacity: 0.9 }} />
                        <span style={{ fontSize: 13, fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.18em', opacity: 0.85 }}>
                            Smart Discovery
                        </span>
                    </div>
                    <h1 style={{
                        fontSize: 'clamp(1.9rem, 5vw, 3.2rem)',
                        fontFamily: 'Georgia, serif',
                        fontWeight: 700, marginBottom: 16, lineHeight: 1.2,
                    }}>
                        Find Your Perfect Destination
                    </h1>
                    <p style={{ fontSize: '1.05rem', opacity: 0.85, maxWidth: 460, margin: '0 auto', lineHeight: 1.6 }}>
                        Tell us your travel intent and our scoring engine will rank the best destinations just for you.
                    </p>
                </div>
            </section>

            {/* ── Form Card ────────────────────────────────────────────────── */}
            <section style={{ maxWidth: 880, margin: '-36px auto 0', padding: '0 16px 80px', position: 'relative', zIndex: 10 }}>
                <div style={{
                    background: 'hsl(var(--card))',
                    border: '1px solid hsl(var(--border))',
                    borderRadius: 20,
                    boxShadow: '0 20px 60px rgba(0,0,0,0.13)',
                    padding: 'clamp(24px, 5vw, 44px)',
                }}>

                    {/* Purpose */}
                    <div style={{ marginBottom: 36 }}>
                        <Label style={{ fontSize: 16, fontWeight: 700, color: 'hsl(var(--foreground))', marginBottom: 14, display: 'block' }}>
                            What's the purpose of your trip?
                        </Label>
                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(152px, 1fr))', gap: 12 }}>
                            {PURPOSES.map(p => {
                                const Icon = p.icon;
                                const active = purpose === p.value;
                                return (
                                    <button
                                        key={p.value}
                                        onClick={() => setPurpose(p.value)}
                                        data-testid={`purpose-${p.label.toLowerCase().replace(/\s+/g, '-')}`}
                                        style={{
                                            padding: '18px 12px',
                                            borderRadius: 14,
                                            border: `2px solid ${active ? 'hsl(var(--primary))' : 'hsl(var(--border))'}`,
                                            background: active ? 'hsl(var(--primary)/0.07)' : 'hsl(var(--background))',
                                            cursor: 'pointer',
                                            textAlign: 'center',
                                            transition: 'all 0.2s ease',
                                            display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 10,
                                            transform: active ? 'scale(1.02)' : 'scale(1)',
                                        }}
                                    >
                                        <Icon style={{ width: 26, height: 26, color: active ? 'hsl(var(--primary))' : 'hsl(var(--muted-foreground))' }} />
                                        <div>
                                            <div style={{ fontWeight: 700, fontSize: 13, color: active ? 'hsl(var(--primary))' : 'hsl(var(--foreground))' }}>
                                                {p.label}
                                            </div>
                                            <div style={{ fontSize: 11, color: 'hsl(var(--muted-foreground))', marginTop: 3 }}>
                                                {p.description}
                                            </div>
                                        </div>
                                    </button>
                                );
                            })}
                        </div>
                    </div>

                    {/* Budget */}
                    <div style={{ marginBottom: 36 }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12, flexWrap: 'wrap', gap: 8 }}>
                            <Label style={{ fontSize: 16, fontWeight: 700, color: 'hsl(var(--foreground))' }}>
                                Max Cost-of-Living Index
                            </Label>
                            <span style={{
                                fontSize: 13, fontWeight: 700,
                                color: 'hsl(var(--primary))',
                                background: 'hsl(var(--primary)/0.1)',
                                padding: '4px 14px', borderRadius: 100,
                                whiteSpace: 'nowrap',
                            }}>
                                {maxBudget} — {budgetLabel(maxBudget)}
                            </span>
                        </div>
                        <input
                            type="range"
                            min="0" max="100"
                            value={maxBudget}
                            onChange={e => setMaxBudget(Number(e.target.value))}
                            data-testid="budget-slider"
                            style={{ width: '100%', accentColor: 'hsl(var(--primary))', height: 6, cursor: 'pointer', display: 'block' }}
                        />
                        <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 6, fontSize: 11, color: 'hsl(var(--muted-foreground))' }}>
                            <span>Budget (0)</span>
                            <span>Luxury (100)</span>
                        </div>
                    </div>

                    {/* Vibe Tags */}
                    <div style={{ marginBottom: 36 }}>
                        <Label style={{ fontSize: 16, fontWeight: 700, color: 'hsl(var(--foreground))', marginBottom: 12, display: 'block' }}>
                            Preferred Vibes{' '}
                            <span style={{ fontSize: 12, fontWeight: 400, color: 'hsl(var(--muted-foreground))' }}>
                                (select any that apply)
                            </span>
                        </Label>
                        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 10 }}>
                            {VIBE_TAGS.map(tag => {
                                const Icon = tag.icon;
                                const active = (vibeTags & tag.value) !== 0;
                                return (
                                    <button
                                        key={tag.value}
                                        onClick={() => toggleVibeTag(tag.value)}
                                        data-testid={`vibe-${tag.label.toLowerCase().replace(/\s+/g, '-')}`}
                                        style={{
                                            display: 'flex', alignItems: 'center', gap: 7,
                                            padding: '8px 18px',
                                            borderRadius: 100,
                                            border: `2px solid ${active ? 'hsl(var(--primary))' : 'hsl(var(--border))'}`,
                                            background: active ? 'hsl(var(--primary))' : 'hsl(var(--background))',
                                            color: active ? 'hsl(var(--primary-foreground))' : 'hsl(var(--foreground))',
                                            cursor: 'pointer',
                                            fontWeight: 600, fontSize: 13,
                                            transition: 'all 0.15s ease',
                                        }}
                                    >
                                        <Icon style={{ width: 13, height: 13 }} />
                                        {tag.label}
                                    </button>
                                );
                            })}
                        </div>
                    </div>

                    {/* Top N + Submit Row */}
                    <div style={{ display: 'flex', gap: 12, alignItems: 'flex-end', flexWrap: 'wrap' }}>
                        <div style={{ flex: '0 0 auto' }}>
                            <Label style={{ fontSize: 13, marginBottom: 6, display: 'block', color: 'hsl(var(--muted-foreground))' }}>
                                Results
                            </Label>
                            <Select value={topN} onValueChange={setTopN}>
                                <SelectTrigger style={{ width: 110 }} data-testid="topn-select">
                                    <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="3">Top 3</SelectItem>
                                    <SelectItem value="5">Top 5</SelectItem>
                                    <SelectItem value="10">Top 10</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>

                        <Button
                            onClick={handleSearch}
                            disabled={isSearching}
                            data-testid="button-find-destinations"
                            style={{ flex: 1, minWidth: 200, height: 48, fontSize: 15, fontWeight: 700 }}
                        >
                            {isSearching ? (
                                <span style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                                    <span style={{
                                        width: 17, height: 17, borderRadius: '50%',
                                        border: '2.5px solid rgba(255,255,255,0.3)',
                                        borderTopColor: 'white',
                                        animation: 'disc-spin 0.65s linear infinite',
                                        display: 'inline-block',
                                        flexShrink: 0,
                                    }} />
                                    Scoring destinations...
                                </span>
                            ) : (
                                <span style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                                    <Sparkles style={{ width: 17, height: 17 }} />
                                    Find My Destinations
                                </span>
                            )}
                        </Button>
                    </div>
                </div>

                {/* ── Results ──────────────────────────────────────────────── */}
                {results !== null && (
                    <div style={{ marginTop: 52 }}>
                        <div style={{ marginBottom: 28, display: 'flex', alignItems: 'baseline', gap: 14, flexWrap: 'wrap' }}>
                            <h2 style={{
                                fontSize: '1.7rem', fontFamily: 'Georgia, serif',
                                fontWeight: 700, color: 'hsl(var(--foreground))',
                            }}>
                                {results.length} Destination{results.length !== 1 ? 's' : ''} Found
                            </h2>
                            <span style={{ color: 'hsl(var(--muted-foreground))', fontSize: 14 }}>
                                {qualifiedCount} qualified · {disqualifiedCount} over budget
                            </span>
                        </div>

                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(265px, 1fr))', gap: 22 }}>
                            {results.map((result, idx) => (
                                <div
                                    key={result.destinationId}
                                    data-testid={`result-card-${result.destinationId}`}
                                    style={{
                                        background: 'hsl(var(--card))',
                                        border: `1px solid ${result.isDisqualified ? 'hsl(var(--destructive)/0.25)' : 'hsl(var(--border))'}`,
                                        borderRadius: 18,
                                        overflow: 'hidden',
                                        boxShadow: result.isDisqualified ? 'none' : '0 4px 24px rgba(0,0,0,0.09)',
                                        opacity: result.isDisqualified ? 0.6 : 1,
                                        animation: `disc-fade-up 0.45s ease ${idx * 0.07}s both`,
                                        transition: 'transform 0.2s ease, box-shadow 0.2s ease',
                                    }}
                                    onMouseEnter={e => {
                                        if (!result.isDisqualified) {
                                            (e.currentTarget as HTMLElement).style.transform = 'translateY(-5px)';
                                            (e.currentTarget as HTMLElement).style.boxShadow = '0 16px 48px rgba(0,0,0,0.15)';
                                        }
                                    }}
                                    onMouseLeave={e => {
                                        (e.currentTarget as HTMLElement).style.transform = '';
                                        (e.currentTarget as HTMLElement).style.boxShadow = result.isDisqualified ? 'none' : '0 4px 24px rgba(0,0,0,0.09)';
                                    }}
                                >
                                    {/* Card Header */}
                                    <div style={{
                                        padding: '22px 20px 18px',
                                        background: result.isDisqualified
                                            ? 'linear-gradient(135deg, #374151 0%, #1f2937 100%)'
                                            : 'linear-gradient(135deg, hsl(var(--primary)) 0%, hsl(var(--primary)/0.65) 100%)',
                                        display: 'flex',
                                        alignItems: 'center',
                                        justifyContent: 'space-between',
                                        position: 'relative',
                                    }}>
                                        {result.isDisqualified && (
                                            <div style={{
                                                position: 'absolute', top: 0, right: 0,
                                                background: 'hsl(var(--destructive))',
                                                color: 'white', fontSize: 10, fontWeight: 800,
                                                padding: '4px 10px',
                                                borderBottomLeftRadius: 10,
                                                textTransform: 'uppercase', letterSpacing: '0.08em',
                                            }}>
                                                Over Budget
                                            </div>
                                        )}

                                        <div>
                                            <div style={{ fontSize: 30, marginBottom: 4, lineHeight: 1 }}>
                                                {toFlag(result.countryCode)}
                                            </div>
                                            <h3 style={{
                                                color: 'white', fontFamily: 'Georgia, serif',
                                                fontSize: '1.15rem', fontWeight: 700, marginBottom: 4,
                                            }}>
                                                {result.cityName}
                                            </h3>
                                            <div style={{ color: 'rgba(255,255,255,0.65)', fontSize: 12, display: 'flex', alignItems: 'center', gap: 3 }}>
                                                <MapPin style={{ width: 10, height: 10 }} />
                                                {result.countryCode}
                                            </div>
                                        </div>

                                        {/* Score Ring */}
                                        <div style={{ position: 'relative', flexShrink: 0 }}>
                                            <ScoreRing score={result.isDisqualified ? 0 : Math.round(result.totalWeightedScore)} size={84} />
                                            <div style={{
                                                position: 'absolute', inset: 0,
                                                display: 'flex', flexDirection: 'column',
                                                alignItems: 'center', justifyContent: 'center',
                                            }}>
                                                <span style={{ color: 'white', fontWeight: 800, fontSize: 20, lineHeight: 1 }}>
                                                    {result.isDisqualified ? '—' : Math.round(result.totalWeightedScore)}
                                                </span>
                                                {!result.isDisqualified && (
                                                    <span style={{ color: 'rgba(255,255,255,0.6)', fontSize: 8, marginTop: 2, letterSpacing: '0.05em' }}>
                                                        SCORE
                                                    </span>
                                                )}
                                            </div>
                                        </div>
                                    </div>

                                    {/* Card Body */}
                                    <div style={{ padding: '18px 20px 22px' }}>
                                        {result.isDisqualified ? (
                                            <p style={{ fontSize: 13, color: 'hsl(var(--muted-foreground))', fontStyle: 'italic', marginBottom: 18, lineHeight: 1.5 }}>
                                                {result.disqualificationReason ?? 'This destination exceeds your cost-of-living ceiling.'}
                                            </p>
                                        ) : (
                                            <div style={{ display: 'flex', flexDirection: 'column', gap: 11, marginBottom: 18 }}>
                                                <ScoreBar label="🌤️  Climate" value={result.climateScore} />
                                                <ScoreBar label="🏗️  Infrastructure" value={result.infrastructureScore} />
                                                <ScoreBar label="✨  Vibe Match" value={result.vibeMatchScore} />
                                                <ScoreBar label="💰  Cost Score" value={result.costScore} />
                                            </div>
                                        )}

                                        <Button
                                            onClick={() => handleSaveOpen(result)}
                                            disabled={result.isDisqualified}
                                            variant={result.isDisqualified ? 'outline' : 'default'}
                                            style={{ width: '100%', fontSize: 14, fontWeight: 600 }}
                                            data-testid={`save-btn-${result.destinationId}`}
                                        >
                                            {result.isDisqualified ? 'Not Available' : '+ Save to My Trips'}
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </section>

            {/* ── Save Dialog ───────────────────────────────────────────────── */}
            <Dialog open={!!saveTarget} onOpenChange={open => { if (!open) setSaveTarget(null); }}>
                <DialogContent className="sm:max-w-[420px]">
                    <DialogHeader>
                        <DialogTitle className="font-serif text-2xl">
                            Save {saveTarget?.cityName} {saveTarget ? toFlag(saveTarget.countryCode) : ''}
                        </DialogTitle>
                    </DialogHeader>
                    <div className="grid gap-4 py-4">
                        <p className="text-sm text-muted-foreground">
                            Optionally set travel dates. If left blank, we'll default to 30 days from now for 7 days.
                        </p>
                        <div className="space-y-2">
                            <Label htmlFor="save-start">Start Date <span className="text-muted-foreground font-normal">(optional)</span></Label>
                            <Input
                                id="save-start"
                                type="date"
                                value={saveStartDate}
                                onChange={e => setSaveStartDate(e.target.value)}
                            />
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="save-end">End Date <span className="text-muted-foreground font-normal">(optional)</span></Label>
                            <Input
                                id="save-end"
                                type="date"
                                value={saveEndDate}
                                onChange={e => setSaveEndDate(e.target.value)}
                            />
                        </div>
                        <div className="flex items-center space-x-2 mt-2">
                            {googleAccessToken ? (
                                <>
                                    <Checkbox
                                        id="syncGoogle"
                                        checked={saveSyncGoogle}
                                        onCheckedChange={(checked) => setSaveSyncGoogle(checked as boolean)}
                                    />
                                    <Label htmlFor="syncGoogle" className="cursor-pointer mb-0">
                                        Add to Google Calendar
                                    </Label>
                                </>
                            ) : (
                                <>
                                    <Checkbox id="syncGoogleDisabled" disabled />
                                    <Label htmlFor="syncGoogleDisabled" className="text-gray-400 mb-0">
                                        Add to Google Calendar
                                        <span className="block text-xs font-normal mt-0.5">Sign in with Google to enable this feature</span>
                                    </Label>
                                </>
                            )}
                        </div>
                    </div>
                    <DialogFooter>
                        <Button variant="outline" onClick={() => setSaveTarget(null)}>Cancel</Button>
                        <Button onClick={handleSaveConfirm} disabled={isSaving}>
                            {isSaving ? 'Saving...' : '🌍 Save Trip'}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

            {/* Keyframe animations */}
            <style>{`
                @keyframes disc-spin {
                    to { transform: rotate(360deg); }
                }
                @keyframes disc-fade-up {
                    from { opacity: 0; transform: translateY(22px); }
                    to   { opacity: 1; transform: translateY(0); }
                }
            `}</style>
        </div>
    );
}
