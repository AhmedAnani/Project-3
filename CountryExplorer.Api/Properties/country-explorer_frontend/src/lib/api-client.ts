import axios from "axios";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

export interface UserProfile {
    id: string;
    email: string;
    fullName: string;
    pictureUrl?: string;
    role: string;
    createdAt: string;
}

export interface AuthResponse {
    user: UserProfile;
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAt: string;
    googleAccessToken?: string;
}

export const apiClient = axios.create({
    baseURL: "/api",
    headers: { "Content-Type": "application/json" },
});
let googleTokenGetter: (() => string | null) | null = null;

export const setGoogleTokenGetter = (getter: () => string | null) => {
    googleTokenGetter = getter;
};
let authTokenGetter: (() => string | null) | null = null;

export const setAuthTokenGetter = (getter: () => string | null) => {
    authTokenGetter = getter;
    apiClient.interceptors.request.use((config) => {
        const token = authTokenGetter?.();
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        const googleToken = googleTokenGetter?.();
        if (googleToken) {
            config.headers["X-Google-Token"] = googleToken;
        }
        return config;
    });
};

// ---------- Health ----------
export const useHealthCheck = () =>
    useQuery({
        queryKey: ["health"],
        queryFn: async () => {
            const { data } = await apiClient.get("/health");
            return data;
        },
    });

// ---------- Auth ----------
export const useAuthRefresh = () =>
    useMutation({
        mutationFn: async (payload: { data: { refreshToken: string } }) => {
            const { data } = await apiClient.post("/auth/refresh", payload.data);
            return data;
        },
    });

export const useAuthLogout = () =>
    useMutation({
        mutationFn: async (payload: { data: { refreshToken: string } }) => {
            await apiClient.post("/auth/logout", payload.data);
        },
    });

// ---------- Country Details ----------
export const useGetCountryDetails = (
    countryId: string,
    options?: { query?: { enabled?: boolean } }
) =>
    useQuery({
        queryKey: ["country", countryId],
        queryFn: async () => {
            const { data } = await apiClient.get(`/countries/${countryId}`);
            return data; // { country, attractions }
        },
        enabled: !!countryId && (options?.query?.enabled !== false),
    });

// ---------- Budget Estimate ----------
export const useGetBudgetEstimate = (
    countryName: string,
    params: { amount: number; fromCurrency: string },
    options?: { query?: { enabled?: boolean } }
) =>
    useQuery({
        queryKey: ["budget", countryName, params.amount, params.fromCurrency],
        queryFn: async () => {
            const { data } = await apiClient.get(
                `/countries/${countryName}/budget-estimate`,
                { params: { amount: params.amount, fromCurrency: params.fromCurrency } }
            );
            return data;
        },
        enabled:
            !!countryName &&
            params.amount > 0 &&
            !!params.fromCurrency &&
            (options?.query?.enabled !== false),
    });

// ---------- Users ----------
export const useGetMyProfile = () =>
    useQuery({
        queryKey: ["profile", "me"],
        queryFn: async () => {
            const { data } = await apiClient.get("/users/me");
            return data;
        },
    });

export const useGetAllUsers = () =>
    useQuery({
        queryKey: ["users"],
        queryFn: async () => {
            const { data } = await apiClient.get("/users");
            return data; // expects { users: [], totalUsers: number }
        },
    });

// Corrected: expects { userId } as argument
export const useDeleteUser = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async ({ userId }: { userId: string }) => {
            await apiClient.delete(`/users/${userId}`);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["users"] });
        },
    });
};

export const getGetAllUsersQueryKey = () => ["users"];

// ---------- Attractions (with options support) ----------
export const useGetAttractionDetails = (
    xid: string,
    options?: { query?: { enabled?: boolean } }
) =>
    useQuery({
        queryKey: ["attraction", xid],
        queryFn: async () => {
            const { data } = await apiClient.get(`/attractions/${xid}`);
            return data;
        },
        enabled: !!xid && (options?.query?.enabled !== false),
    });

// ---------- Trips (correct signatures) ----------
export type TripItem = {
    id: number;
    title: string;
    countryCode: string;
    countryName?: string;
    startDate: string;
    endDate: string;
    notes?: string;
    status: 'Planned' | 'Visited';
    visitedDate?: string;
    syncWithGoogleCalendar?: boolean;  
};

export type TripItemStatus = 'Planned' | 'Visited';

// Accepts pagination params, returns { items, totalPages, totalItems }
export const useGetTrips = ({ pageNumber, pageSize }: { pageNumber: number; pageSize: number }) =>
    useQuery({
        queryKey: ["trips", pageNumber, pageSize],
        queryFn: async () => {
            const { data } = await apiClient.get("/trips", {
                params: { pageNumber, pageSize },
            });
            return data;
        },
    });

// Create expects { data: ... }
export const useCreateTrip = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async ({ data }: { data: Omit<TripItem, 'id' | 'countryName' | 'status' | 'visitedDate'> }) => {
            const { data: created } = await apiClient.post("/trips", data);
            return created;
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["trips"] });
        },
    });
};

// Update expects { id, data: ... }
export const useUpdateTrip = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async ({ id, data }: { id: number; data: Partial<TripItem> }) => {
            const { data: updated } = await apiClient.put(`/trips/${id}`, data);
            return updated;
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["trips"] });
        },
    });
};

// Delete expects { id }
export const useDeleteTrip = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async ({ id }: { id: number }) => {
            await apiClient.delete(`/trips/${id}`);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["trips"] });
        },
    });
};

export const getGetTripsQueryKey = () => ["trips"];

// ---------- Discovery / Recommendations ----------
export type ScoreBreakdownDto = {
    destinationId: number;
    cityName: string;
    countryCode: string;
    climateScore: number;
    infrastructureScore: number;
    vibeMatchScore: number;
    costScore: number;
    totalWeightedScore: number;
    isDisqualified: boolean;
    disqualificationReason?: string;
};

// Mirrors CountryExplorer.Domain.Enums.TripPurpose
export const TripPurposeMap = {
    Workation: 0,
    Adventure: 1,
    Vacation: 2,
    CultureHistory: 3,
} as const;

// Mirrors CountryExplorer.Domain.Enums.VibeTag [Flags]
export const VibeTagMap = {
    None: 0,
    IsCoastal: 1,
    HasMountains: 2,
    IsHistoric: 4,
    IsUrban: 8,
    IsWarmClimate: 16,
    IsColdClimate: 32,
} as const;

export type DiscoveryRequest = {
    purpose: number;
    maxBudget: number;
    preferredTags?: number;
    topN: number;
};

export type SaveToTripsRequest = {
    destinationId: number;
    startDate?: string;
    endDate?: string;
    syncWithGoogleCalendar?: boolean;
};

export const useGetRecommendations = () =>
    useMutation({
        mutationFn: async (request: DiscoveryRequest) => {
            const { data } = await apiClient.post<ScoreBreakdownDto[]>('/discovery/recommend', request);
            return data;
        },
    });

export const useSaveToTrips = () =>
    useMutation({
        mutationFn: async ({ destinationId, startDate, endDate, syncWithGoogleCalendar }: SaveToTripsRequest) => {
            const { data } = await apiClient.post(`/discovery/${destinationId}/save`, {
                startDate,
                endDate,
                syncWithGoogleCalendar: syncWithGoogleCalendar ?? false,
            });
            return data;
        },
    });

export default apiClient;