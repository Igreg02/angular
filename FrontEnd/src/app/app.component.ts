import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/components/navbar/navbar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  template:'<app-navbar> </app-navbar> <main class="containet page"> <router-outlet> </router-outlet> </main>'
})
export class AppComponent {}
