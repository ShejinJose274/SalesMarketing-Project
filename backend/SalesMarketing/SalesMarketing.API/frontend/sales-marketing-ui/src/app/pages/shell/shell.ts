
import {
  Component,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import {
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';

import { AuthService } from '../../services/auth.service';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ButtonModule
  ],
  templateUrl: './shell.html',
  styleUrl: './shell.css',
})
export class Shell {

  // Dependency injection
  readonly auth = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);

  // Sidebar menu state
  menuOpen = false;

  // Logout
  logout() {
    this.auth.logout().subscribe({
      next: () => {
        // Refresh the shell after logout
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Logout failed:', error);

        // Refresh the view after an error
        this.cdr.detectChanges();
      }
    });
  }
}
