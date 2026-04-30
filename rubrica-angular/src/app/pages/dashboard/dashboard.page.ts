import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.page.html',
  styleUrl: './dashboard.page.css',
})
export class DashboardPage {
  private readonly authService = inject(AuthService);

  readonly user = this.authService.currentUser;

  canEditInterests(): boolean{
    return this.authService.hasAnyRole(['Admin', 'Editor']);
  }

  isAdmin(): boolean{
    return this.authService.hasRole('Admin');
  }
}
