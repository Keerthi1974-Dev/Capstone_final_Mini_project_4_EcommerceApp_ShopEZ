import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent {
  name = '';
  email = '';
  password = '';
  confirmPassword = '';
  role = 'User';
  message = '';
  error = '';
  loading = false;
  showPassword = false;

  constructor(private authService: AuthService, private router: Router) {}

  
  get passwordMismatch(): boolean {
    return this.confirmPassword.length > 0 && this.password !== this.confirmPassword;
  }

  register(form: NgForm): void {
    if (form.invalid) {
      this.error = 'Please fill all fields correctly';
      return;
    }

    if (this.passwordMismatch) {
      this.error = 'Passwords do not match';
      return;
    }

    this.error = '';
    this.loading = true;

    this.authService.register({
      name: this.name,
      email: this.email,
      password: this.password,
      role: this.role
    }).subscribe({
      next: () => {
        this.loading = false;
        this.message = 'Account created successfully!';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: () => {
        this.loading = false;
        this.error = 'Registration failed. This email may already be in use.';
      }
    });
  }
}