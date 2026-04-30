export interface ChangeUserRoleRequest {
    email: string;
    newRole : string;
}

export interface ChangeUserRoleResponse {
    message: string;
    email  : string;
    role   : string;
}