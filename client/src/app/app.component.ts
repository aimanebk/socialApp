import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Observable, throwError } from 'rxjs';

import { catchError, first, tap } from 'rxjs/operators'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'The Social app';
  users$ : Observable<any>;

  constructor(private http : HttpClient){
    this.getUsers();
  }

  getUsers(){
    this.users$ = this.http.get('https://localhost:5001/api/users')
    .pipe(
      tap(console.log),
      catchError(err => {
        return throwError(err)
      }),
      first()
    );
  }
}
