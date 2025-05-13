import { CanDeactivateFn } from '@angular/router';
import { Member } from '../_models/member';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';

export const preventUnsavedChangesGuard: CanDeactivateFn<MemberEditComponent> = (component) => {
  
  if(component.editForm?.dirty){
    return confirm ("You have some changes but not Saved")
  }
  return true;
};
