import { AuthService } from './auth.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Component, OnInit, NgZone } from '@angular/core';

@Component({
  selector: 'lp-auth-callback',
  template: '<p>Please wait while we redirect you back</p>'
})

export class AuthCallbackComponent implements OnInit {

  constructor(private router: Router, private authService: AuthService, private zone: NgZone) { }

  ngOnInit() {
    this.authService.completeAuthentication();

    setTimeout(() => {
      this.zone.run(
        () => this.router.navigate(['/'])
      );
    }, 200);
  }
}
