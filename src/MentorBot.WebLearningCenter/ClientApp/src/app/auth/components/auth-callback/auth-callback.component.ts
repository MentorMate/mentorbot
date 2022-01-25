import { AuthService } from '../../auth.service';
import { Router } from '@angular/router';
import { Component, OnInit, NgZone } from '@angular/core';

const template = '<p>Please wait while we redirect you back</p>';

@Component({
  selector: 'app-auth-callback',
  template,
})
export class AuthCallbackComponent implements OnInit {
  constructor(private router: Router, private authService: AuthService, private zone: NgZone) {}

  ngOnInit() {
    this.authService.completeAuthentication().then(success => {
      if (success) {
        this.zone.run(() => this.router.navigate(['/']));
      }
    });
  }
}
