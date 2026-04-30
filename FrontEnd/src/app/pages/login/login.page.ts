import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.page.html',
  styleUrl: './login.page.css',
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal('');

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }


    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.isLoading.set(false);
        void this.router.navigate(['/dashboard']);
      },
      error: (error:unknown) => {
        this.isLoading.set(false);
        this.errorMessage.set(this.extractErrorMessage(error, 'Login non riusciuto'));
      }
    })
  }
   
  private extractErrorMessage(error:unknown, fallback: string): string {
    if(error instanceof HttpErrorResponse){
      return error.error?.message ?? fallback;
    }

    return fallback;
  }
}
