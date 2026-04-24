export interface RegisterRequest
{
    email: string;
    password: string;
    nomeCompleto: string;
    phoneNumber?: string | null;
}