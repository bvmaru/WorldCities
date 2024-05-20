import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormControl, Validators, AbstractControl, AsyncValidatorFn } from '@angular/forms';

import { BaseFormComponent } from '../base-form.component';
import { AuthService } from './auth.service';
import { LoginRequest } from './login-request';
import { LoginResult } from './login-result';
import { ResetMailRequest } from './reset-mail-request';
import { NewPasswordRequest } from './new-password-request';

@Component({
  selector: 'app-reset',
  templateUrl: './reset.component.html',
  styleUrls: ['./reset.component.scss']
})
export class ResetComponent
  extends BaseFormComponent implements OnInit {

  title?: string;
  loginResult?: LoginResult;
  mail?: string;

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private authService: AuthService) {
    super();
  }

  ngOnInit() {
    this.form = new FormGroup({
      email: new FormControl('', Validators.required),
      token: new FormControl('', Validators.required),
      newPassword: new FormControl('', [Validators.required, Validators.pattern(/^(?=.*[A-Z])(?=.*[!@#$%^&*(),.?":{}|<>])(?=.{8,}).*$/)]),
    });
  }

  onSubmit() {
    var newPasswordRequest = <NewPasswordRequest>{};
    newPasswordRequest.email = this.form.controls['email'].value;
    newPasswordRequest.token = this.form.controls['token'].value;
    newPasswordRequest.password = this.form.controls['newPassword'].value;

    this.authService
      .resetPassword(newPasswordRequest)
      .subscribe(result => {
        console.log(result);
        this.loginResult = result;
        if (result.success) {
          this.router.navigate(["/"]);
        }
      }, error => {
        console.log(error);
        if (error.status == 401) {
          this.loginResult = error.error;
        }
      });
  }

  sendMail(){
    var resetMailRequest = <ResetMailRequest>{};
    resetMailRequest.email = this.form.controls['email'].value;
    this.mail = resetMailRequest.email;

    this.authService
      .resetMail(resetMailRequest)
      .subscribe(result => {
        console.log(result);
      }, error => {
        console.log(error);
      });
  }
}