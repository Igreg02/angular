import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AdminUserService } from '../../services/admin-user.service';

@Component({
  selector: 'app-admin-change-role',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './admin-change-role.page.html',
})
export class AdminChangeRolePage {
  private readonly fb = inject(FormBuilder);
  private readonly adminUsersService = inject(AdminUserService);

  readonly isSubmitting = signal(false);
  readonly successMessage = signal('');
  readonly errorMessage = signal('');
  readonly roles = ['Admin', 'Editor', 'User'];

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    newRole: ['User', [Validators.required]],
  });

  submit(): void {
    if (this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.successMessage.set('');
    this.errorMessage.set('');

    this.AdminUserService.changeRole(this.form.getRawValue()).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        this.successMessage.set(`${response.message} Nuovo Ruolo: ${response.role}`);
      },
      error: (error: unknown) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(this.extractErrorMessage(error));
      },
    });
  }

  private extractErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      return error.error?.message ?? 'Cambio ruolo non riuscito.';
    }
    return 'Cambio ruolo non riuscito.';
  }
}
