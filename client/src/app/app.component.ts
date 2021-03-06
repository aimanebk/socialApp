import { User } from './models/user';
import { AccountService } from './services/account.service';
import { Component, OnInit } from '@angular/core';

import { PresenceService } from './services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'The Social app';
  users: any;

  constructor(private accountService: AccountService, private presence: PresenceService) {}

  ngOnInit(){
    this.setCurrentUser();
  }

  setCurrentUser() {
    const user: User = JSON.parse(localStorage.getItem('user'));
    if (user) {
      this.accountService.setCurrentUser(user);
      this.presence.createHubConnection(user);
    }
  }
}
