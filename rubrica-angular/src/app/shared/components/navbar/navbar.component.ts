import { Component,computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone:true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent{
  private readonly Authservice = inject(AuthService);

  readonly user = this.Authservice.currentUser;
  readonly isAdmin = computed(()=> this.Authservice.hasRole("Admin"));
  readonly isAuthenticated = computed(()=> this.Authservice.isAuthenticated());

  logout(): void
  {
    this.Authservice.logout();
  }
}
