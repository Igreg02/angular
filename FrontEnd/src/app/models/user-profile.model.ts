export interface UserProfile{
    id          : string,
    nomeCompleto: string;
    email       : string;
    phoneNumber?: string | null;
}