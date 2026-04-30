import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.page.html'
})
export class RegisterPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly successMessage = signal('');
  readonly errorMessage = signal('');


  readonly form = this.fb.group({
    email: this.fb.nonNullable.control('', [Validators.required, Validators.email]),
    password: this.fb.nonNullable.control('', [Validators.required, Validators.minLength(6)]),
    nomeCompleto: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    phoneNumber: this.fb.control<string | null>('')
  })

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.successMessage.set('');
    this.errorMessage.set('');

    const payload = {
      ...this.form.getRawValue(),
      phoneNumber: this.form.getRawValue().phoneNumber || null
    };

    this.authService.register(payload).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.successMessage.set(response.message);
        setTimeout(() => void this.router.navigate(['/login']), 900);
      },

      error: (error: unknown) => {
        this.isLoading.set(false);
        this.errorMessage.set(this.exctractErrorMessage(error));
      }
    });
  }
  private exctractErrorMessage(error: unknown): string {
    if(error instanceof HttpErrorResponse)
    {
      if(Array.isArray(error.error) && error.error.length > 0){
        return error.error.map((item: {description?: string}) => item.description ?? 'Errore').join('|');
      }

      return error.error?.message ??  'Registrazione non riuscita';
    }
    return 'Registrazione non riuscita'
  }



}
