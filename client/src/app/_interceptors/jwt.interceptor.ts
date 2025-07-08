// import { HttpInterceptorFn } from '@angular/common/http';
// import { inject } from '@angular/core';
// import { AccountService } from '../_services/account.service';

// export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  
//   const accountService = inject(AccountService);

//   if (accountService.currentUser()){
//     req = req.clone({
//       setHeaders:{
//         Authorization: `Bearer ${accountService.currentUser}?.token`
//       }
//     })
//   }
//   return next(req);
// };

// THE SNIPPET ABOVE IS FROM COURSE 

//THE SNIPPET BELOW IS FROM OPENAI (WORKS)

import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../_services/account.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  
  const accountService = inject(AccountService);
  const user = accountService.currentUser(); // This is the signal value

  if (user?.token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${user.token}`
      }
    });
  }

  return next(req);
};