import React, { useState } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { useGetMyProfile } from '@workspace/api-client-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { User, Mail, Calendar, Shield, Map } from 'lucide-react';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Skeleton } from '@/components/ui/skeleton';

export default function Profile() {
  const { data: profile, isLoading } = useGetMyProfile();

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-12 max-w-2xl">
        <Skeleton className="w-full h-64 rounded-2xl" />
      </div>
    );
  }

  if (!profile) {
    return <div className="text-center py-20 text-muted-foreground">Profile not found.</div>;
  }

  const initials = profile.fullName
    .split(' ')
    .map(n => n[0])
    .join('')
    .substring(0, 2)
    .toUpperCase();

  return (
    <div className="container mx-auto px-4 py-12 max-w-2xl">
      <div className="space-y-8">
        <div>
          <h1 className="text-4xl font-serif font-bold mb-2">My Profile</h1>
          <p className="text-muted-foreground">Manage your personal information and preferences.</p>
        </div>

        <Card className="border-border/50 shadow-md">
          <CardHeader className="bg-muted/30 border-b border-border/50 pb-8 pt-8 px-8">
            <div className="flex flex-col md:flex-row items-center gap-6">
              <Avatar className="h-24 w-24 border-4 border-background shadow-sm">
                <AvatarImage src={profile.pictureUrl || undefined} alt={profile.fullName} />
                <AvatarFallback className="bg-primary text-primary-foreground text-2xl font-serif">
                  {initials}
                </AvatarFallback>
              </Avatar>
              <div className="text-center md:text-left">
                <CardTitle className="text-2xl font-serif mb-1" data-testid="text-profile-name">{profile.fullName}</CardTitle>
                <div className="inline-flex items-center gap-1.5 px-2.5 py-1 bg-secondary text-secondary-foreground text-xs font-semibold rounded-full uppercase tracking-wider">
                  <Shield className="h-3 w-3" />
                  {profile.role}
                </div>
              </div>
            </div>
          </CardHeader>
          <CardContent className="p-8 space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <InfoField icon={<User />} label="Full Name" value={profile.fullName} />
              <InfoField icon={<Mail />} label="Email Address" value={profile.email} />
              <InfoField icon={<Calendar />} label="Member Since" value={new Date(profile.createdAt).toLocaleDateString()} />
              <InfoField icon={<Map />} label="User ID" value={profile.id.substring(0, 8) + '...'} />
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function InfoField({ icon, label, value }: { icon: React.ReactNode, label: string, value: string }) {
  return (
    <div className="flex gap-3">
      <div className="mt-0.5 text-muted-foreground">
        {React.cloneElement(icon as React.ReactElement, { className: 'h-5 w-5' })}
      </div>
      <div>
        <p className="text-sm text-muted-foreground mb-0.5">{label}</p>
        <p className="font-medium text-foreground">{value}</p>
      </div>
    </div>
  );
}
