import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({

	selector: 'app-root',
	standalone: true,
	imports: [RouterOutlet, NavbarComponent],
	template: `
		<app-navbar></app-navbar>
		<main class="container page">
			<router-outlet></router-outlet>
		</main>

	`

})
export class AppComponent{}
