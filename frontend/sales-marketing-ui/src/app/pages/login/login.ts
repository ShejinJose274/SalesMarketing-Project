
import {
  Component,
  inject,
  ChangeDetectorRef,
  OnInit
} from '@angular/core';

import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';

import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [
    FormsModule,
    ButtonModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {

  // Dependency injection
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  // Login form data
  username = '';
  password = '';
  error = '';

  ngOnInit(): void {
    if (new URLSearchParams(window.location.search).get('expired') === '1') {
      this.error = 'Your session expired. Please sign in again.';
    }
  }

  // Login method
  submit() {
    this.error = '';

    // Validate username and password
    if (!this.username.trim() || !this.password) {
      this.error = 'Enter your username and password.';
      return;
    }

    // Call authentication service
    this.auth.login(
      this.username.trim(),
      this.password
    ).subscribe({

      // Login successful
      next: (response) => {
        // Navigate to the appropriate dashboard
        void this.router.navigateByUrl(response.dashboardUrl);
      },

      // Login failed
      error: (error) => {
        this.error = this.api.errorMessage(error);

        // Immediately display the error message
        this.cdr.detectChanges();
      }
    });
  }
}
