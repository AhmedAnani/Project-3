import React from 'react';
import { useGetAllUsers, useDeleteUser, getGetAllUsersQueryKey } from '@workspace/api-client-react';
import { useQueryClient } from '@tanstack/react-query';
import { Shield, Trash2, Users } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { useToast } from '@/hooks/use-toast';

export default function Admin() {
  const queryClient = useQueryClient();
  const { toast } = useToast();
  
  const { data, isLoading } = useGetAllUsers();
  const deleteUser = useDeleteUser();

  const handleDeleteUser = (id: string, name: string) => {
    if (confirm(`Are you sure you want to delete user ${name}? This action cannot be undone.`)) {
      deleteUser.mutate(
        { userId: id },
        {
          onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: getGetAllUsersQueryKey() });
            toast({ title: "User deleted", description: `${name} has been removed from the system.` });
          },
          onError: () => {
            toast({ title: "Error", description: "Failed to delete user.", variant: "destructive" });
          }
        }
      );
    }
  };

  return (
    <div className="container mx-auto px-4 py-12">
      <div className="flex items-center gap-4 mb-8">
        <div className="p-3 bg-primary text-primary-foreground rounded-xl">
          <Shield className="h-8 w-8" />
        </div>
        <div>
          <h1 className="text-4xl font-serif font-bold">Admin Console</h1>
          <p className="text-muted-foreground mt-1">Manage platform users and access.</p>
        </div>
      </div>

      <Card className="border-border/50 shadow-md">
        <CardHeader className="bg-muted/30 border-b border-border/50">
          <CardTitle className="text-xl flex items-center gap-2">
            <Users className="h-5 w-5 text-primary" />
            Registered Users ({data?.totalUsers || 0})
          </CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {isLoading ? (
            <div className="p-6 space-y-4">
              {[1,2,3,4].map(i => <Skeleton key={i} className="h-16 w-full rounded-lg" />)}
            </div>
          ) : (
            <div className="divide-y divide-border/50">
              {data?.users.map(user => (
                <div key={user.id} className="p-4 sm:px-6 flex flex-col sm:flex-row sm:items-center justify-between gap-4 hover:bg-muted/20 transition-colors" data-testid={`row-user-${user.id}`}>
                  <div className="flex items-center gap-4">
                    <Avatar className="h-12 w-12 border border-border">
                      <AvatarImage src={user.pictureUrl || undefined} />
                      <AvatarFallback className="bg-secondary text-secondary-foreground">
                        {user.fullName.substring(0, 2).toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <p className="font-bold text-foreground leading-tight">{user.fullName}</p>
                      <p className="text-sm text-muted-foreground">{user.email}</p>
                    </div>
                  </div>
                  <div className="flex items-center justify-between sm:justify-end gap-6 w-full sm:w-auto">
                    <div className="text-sm">
                      <span className="text-muted-foreground text-xs uppercase tracking-wider block sm:inline mr-2">Role</span>
                      <span className={`font-semibold ${user.role === 'Admin' ? 'text-accent' : 'text-foreground'}`}>
                        {user.role}
                      </span>
                    </div>
                    <Button 
                      variant="ghost" 
                      size="sm" 
                      className="text-destructive hover:text-destructive hover:bg-destructive/10"
                      onClick={() => handleDeleteUser(user.id, user.fullName)}
                      disabled={user.role === 'Admin' || deleteUser.isPending}
                    >
                      <Trash2 className="h-4 w-4 mr-2" />
                      Delete
                    </Button>
                  </div>
                </div>
              ))}
              
              {(!data?.users || data.users.length === 0) && (
                <div className="p-12 text-center text-muted-foreground">
                  No users found.
                </div>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
