import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { enviroment } from "../../enviroments/enviroment";
import { ChangeUserRoleRequest, ChangeUserRoleResponse } from "../models/change-user-role.model";

@Injectable({ providedIn: 'root'})
export class AdminUserService
{
    private readonly http = inject (HttpClient);

    changeRole(payload: ChangeUserRoleRequest): Observable<ChangeUserRoleResponse> {
        return this.http.put<ChangeUserRoleResponse>(`${enviroment.apiBaseUrl}/AdminUsers/change-role`, payload);
    }
}