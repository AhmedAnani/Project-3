import React, { useState } from 'react';
import { 
  useGetTrips, 
  useCreateTrip, 
  useUpdateTrip, 
  useDeleteTrip,
  getGetTripsQueryKey,
  TripItem,
  TripItemStatus
} from '@workspace/api-client-react';
import { useQueryClient } from '@tanstack/react-query';
import { Map, Plus, MoreVertical, Calendar, Globe, MapPin, Trash2, Edit2, CheckCircle2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle, CardFooter } from '@/components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { useToast } from '@/hooks/use-toast';
import { Skeleton } from '@/components/ui/skeleton';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Checkbox } from '@/components/ui/checkbox';
export default function Trips() {
  const queryClient = useQueryClient();
  const { toast } = useToast();
  
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  
  const { data, isLoading } = useGetTrips({ pageNumber, pageSize });
  
  const createTrip = useCreateTrip();
  const updateTrip = useUpdateTrip();
  const deleteTrip = useDeleteTrip();

  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [editingTrip, setEditingTrip] = useState<TripItem | null>(null);

  // Form states
  const [title, setTitle] = useState('');
  const [countryCode, setCountryCode] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [notes, setNotes] = useState('');
  const [status, setStatus] = useState<TripItemStatus>('Planned');
  const [syncWithGoogleCalendar, setSyncWithGoogleCalendar] = useState(false);

  const resetForm = () => {
    setTitle('');
    setCountryCode('');
    setStartDate('');
    setEndDate('');
    setNotes('');
    setStatus('Planned');
  };

  const handleCreateOpen = () => {
    resetForm();
    setIsCreateOpen(true);
  };

  const handleEditOpen = (trip: TripItem) => {
    setEditingTrip(trip);
    setTitle(trip.title);
    setCountryCode(trip.countryCode);
    setStartDate(trip.startDate.split('T')[0]);
    setEndDate(trip.endDate.split('T')[0]);
    setNotes(trip.notes || '');
    setStatus(trip.status);
    setIsEditOpen(true);
  };

  const handleCreateSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    createTrip.mutate(
        { data: { title, countryCode, startDate, endDate, notes, syncWithGoogleCalendar } },
      {
        onSuccess: () => {
          queryClient.invalidateQueries({ queryKey: getGetTripsQueryKey() });
          setIsCreateOpen(false);
          toast({ title: "Trip created", description: "Your new adventure has been saved." });
        },
        onError: () => {
          toast({ title: "Error", description: "Failed to create trip.", variant: "destructive" });
        }
      }
    );
  };

  const handleEditSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingTrip) return;
    
    updateTrip.mutate(
      { 
        id: editingTrip.id, 
        data: { title, countryCode, startDate, endDate, notes, status, visitedDate: status === 'Visited' ? new Date().toISOString() : null } 
      },
      {
        onSuccess: () => {
          queryClient.invalidateQueries({ queryKey: getGetTripsQueryKey() });
          setIsEditOpen(false);
          toast({ title: "Trip updated", description: "Changes saved successfully." });
        },
        onError: () => {
          toast({ title: "Error", description: "Failed to update trip.", variant: "destructive" });
        }
      }
    );
  };

  const handleDelete = (id: number) => {
    if (confirm("Are you sure you want to delete this trip?")) {
      deleteTrip.mutate(
        { id },
        {
          onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getGetTripsQueryKey() });
            toast({ title: "Trip deleted" });
          }
        }
      );
    }
  };

  const handleMarkVisited = (trip: TripItem) => {
    updateTrip.mutate(
      { 
        id: trip.id, 
        data: { 
          title: trip.title, 
          countryCode: trip.countryCode, 
          startDate: trip.startDate, 
          endDate: trip.endDate, 
          notes: trip.notes, 
          status: 'Visited', 
          visitedDate: new Date().toISOString() 
        } 
      },
      {
        onSuccess: () => {
          queryClient.invalidateQueries({ queryKey: getGetTripsQueryKey() });
          toast({ title: "Marked as Visited", description: "Trip completed!" });
        }
      }
    );
  };

  return (
    <div className="container mx-auto px-4 py-12">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 gap-4">
        <div>
          <h1 className="text-4xl font-serif font-bold">My Trips</h1>
          <p className="text-muted-foreground mt-2">Manage your past and future adventures.</p>
        </div>
        <Button onClick={handleCreateOpen} className="gap-2 font-bold" data-testid="button-create-trip">
          <Plus className="h-5 w-5" />
          Plan New Trip
        </Button>
      </div>

      <Tabs defaultValue="all" className="w-full">
        <TabsList className="mb-8">
          <TabsTrigger value="all">All Trips</TabsTrigger>
          <TabsTrigger value="planned">Planned</TabsTrigger>
          <TabsTrigger value="visited">Visited</TabsTrigger>
        </TabsList>

        <TabsContent value="all" className="mt-0">
          <TripGrid trips={data?.items} isLoading={isLoading} onEdit={handleEditOpen} onDelete={handleDelete} onMarkVisited={handleMarkVisited} />
        </TabsContent>
        <TabsContent value="planned" className="mt-0">
          <TripGrid trips={data?.items?.filter(t => t.status === 'Planned')} isLoading={isLoading} onEdit={handleEditOpen} onDelete={handleDelete} onMarkVisited={handleMarkVisited} />
        </TabsContent>
        <TabsContent value="visited" className="mt-0">
          <TripGrid trips={data?.items?.filter(t => t.status === 'Visited')} isLoading={isLoading} onEdit={handleEditOpen} onDelete={handleDelete} onMarkVisited={handleMarkVisited} />
        </TabsContent>
      </Tabs>

      {/* Pagination Placeholder */}
      {data && data.totalPages > 1 && (
        <div className="flex justify-center mt-12 gap-2">
          <Button variant="outline" disabled={pageNumber === 1} onClick={() => setPageNumber(p => p - 1)}>Previous</Button>
          <div className="flex items-center px-4 text-sm font-medium">Page {pageNumber} of {data.totalPages}</div>
          <Button variant="outline" disabled={pageNumber === data.totalPages} onClick={() => setPageNumber(p => p + 1)}>Next</Button>
        </div>
          )}
          {/* Google Calendar sync checkbox */}
          <div className="flex items-center space-x-2 p-3 bg-blue-50 border border-blue-200 rounded-md">
              <Checkbox
                  id="syncGoogle"
                  checked={syncWithGoogleCalendar}
                  onCheckedChange={(checked) => setSyncWithGoogleCalendar(checked as boolean)}
              />
              <Label htmlFor="syncGoogle" className="cursor-pointer flex-1 mb-0">
                  Add to Google Calendar
              </Label>
          </div>
      {/* Create Dialog */}
      <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
        <DialogContent className="sm:max-w-[500px]">
          <form onSubmit={handleCreateSubmit}>
            <DialogHeader>
              <DialogTitle className="font-serif text-2xl">Plan a New Trip</DialogTitle>
            </DialogHeader>
            <div className="grid gap-6 py-6">
              <div className="space-y-2">
                <Label htmlFor="title">Trip Title</Label>
                <Input id="title" value={title} onChange={(e) => setTitle(e.target.value)} required placeholder="e.g. Summer in Tokyo" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="countryCode">Country Code (2 letters)</Label>
                <Input id="countryCode" value={countryCode} onChange={(e) => setCountryCode(e.target.value.toUpperCase())} maxLength={2} required placeholder="e.g. JP" />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="startDate">Start Date</Label>
                  <Input id="startDate" type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} required />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="endDate">End Date</Label>
                  <Input id="endDate" type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} required />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="notes">Notes</Label>
                <Textarea id="notes" value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Places to see, things to do..." />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsCreateOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={createTrip.isPending}>Save Trip</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
        <DialogContent className="sm:max-w-[500px]">
          <form onSubmit={handleEditSubmit}>
            <DialogHeader>
              <DialogTitle className="font-serif text-2xl">Edit Trip</DialogTitle>
            </DialogHeader>
            <div className="grid gap-6 py-6">
              <div className="space-y-2">
                <Label htmlFor="edit-title">Trip Title</Label>
                <Input id="edit-title" value={title} onChange={(e) => setTitle(e.target.value)} required />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Status</Label>
                  <Select value={status} onValueChange={(val: TripItemStatus) => setStatus(val)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Status" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Planned">Planned</SelectItem>
                      <SelectItem value="Visited">Visited</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="edit-country">Country Code</Label>
                  <Input id="edit-country" value={countryCode} onChange={(e) => setCountryCode(e.target.value.toUpperCase())} maxLength={2} required />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="edit-startDate">Start Date</Label>
                  <Input id="edit-startDate" type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} required />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="edit-endDate">End Date</Label>
                  <Input id="edit-endDate" type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} required />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-notes">Notes</Label>
                <Textarea id="edit-notes" value={notes} onChange={(e) => setNotes(e.target.value)} />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsEditOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={updateTrip.isPending}>Save Changes</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function TripGrid({ trips, isLoading, onEdit, onDelete, onMarkVisited }: { 
  trips?: TripItem[], 
  isLoading: boolean, 
  onEdit: (t: TripItem) => void, 
  onDelete: (id: number) => void,
  onMarkVisited: (t: TripItem) => void
}) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
        {[1,2,3].map(i => <Skeleton key={i} className="h-64 rounded-xl" />)}
      </div>
    );
  }

  if (!trips || trips.length === 0) {
    return (
      <div className="text-center py-24 bg-muted/30 rounded-2xl border border-dashed border-border">
        <Map className="h-12 w-12 text-muted-foreground mx-auto mb-4 opacity-50" />
        <h3 className="text-xl font-serif font-bold text-foreground">No trips found</h3>
        <p className="text-muted-foreground mt-2">Start planning your next adventure.</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
      {trips.map(trip => (
        <Card key={trip.id} className="group overflow-hidden flex flex-col hover-elevate transition-all border-border/60 shadow-sm" data-testid={`card-trip-${trip.id}`}>
          <div className="h-3 bg-gradient-to-r from-primary to-accent" />
          <CardHeader className="pb-4">
            <div className="flex justify-between items-start">
              <div>
                <div className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold uppercase tracking-wide bg-secondary text-secondary-foreground mb-3">
                  {trip.status === 'Visited' ? <CheckCircle2 className="h-3 w-3 mr-1" /> : <MapPin className="h-3 w-3 mr-1" />}
                  {trip.status}
                </div>
                <CardTitle className="font-serif text-xl line-clamp-1">{trip.title}</CardTitle>
              </div>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="icon" className="-mr-2 h-8 w-8 text-muted-foreground hover:text-foreground">
                    <MoreVertical className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem onClick={() => onEdit(trip)} className="cursor-pointer">
                    <Edit2 className="mr-2 h-4 w-4" /> Edit
                  </DropdownMenuItem>
                  {trip.status === 'Planned' && (
                    <DropdownMenuItem onClick={() => onMarkVisited(trip)} className="cursor-pointer text-primary">
                      <CheckCircle2 className="mr-2 h-4 w-4" /> Mark Visited
                    </DropdownMenuItem>
                  )}
                  <DropdownMenuItem onClick={() => onDelete(trip.id)} className="cursor-pointer text-destructive focus:text-destructive">
                    <Trash2 className="mr-2 h-4 w-4" /> Delete
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          </CardHeader>
          <CardContent className="pb-4 flex-1">
            <div className="space-y-3 text-sm">
              <div className="flex items-center text-muted-foreground">
                <Globe className="h-4 w-4 mr-2 text-primary" />
                <span className="font-medium text-foreground">{trip.countryName}</span> ({trip.countryCode})
              </div>
              <div className="flex items-center text-muted-foreground">
                <Calendar className="h-4 w-4 mr-2 text-accent" />
                <span>{new Date(trip.startDate).toLocaleDateString()} &mdash; {new Date(trip.endDate).toLocaleDateString()}</span>
              </div>
            </div>
            {trip.notes && (
              <p className="mt-4 text-sm text-muted-foreground line-clamp-3 bg-muted/50 p-3 rounded-md italic border border-border/30">
                "{trip.notes}"
              </p>
            )}
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
