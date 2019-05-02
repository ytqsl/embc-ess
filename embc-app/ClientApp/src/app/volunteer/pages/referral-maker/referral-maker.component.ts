import { Component, OnInit } from '@angular/core';
import { RegistrationService } from 'src/app/core/services/registration.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';
import { UniqueKeyService } from 'src/app/core/services/unique-key.service';
import { Registration } from 'src/app/core/models';
import { IncidentalsReferral } from 'src/app/core/models';

@Component({
  selector: 'app-referral-maker',
  templateUrl: './referral-maker.component.html',
  styleUrls: ['./referral-maker.component.scss']
})
export class ReferralMakerComponent implements OnInit {

  // TODO: retrieve incidentTask (for start date/time) if not already attached to Registration object

  editMode = true; // when you first land on this page
  submitting = false;
  registration: Registration = null;
  path: string = null; // for relative routing
  regId: string = null;
  purchaser: string = null;

  foodReferrals: Array<any> = [];
  clothingReferrals: Array<any> = [];
  accommodationReferrals: Array<any> = [];
  incidentalsReferrals: Array<IncidentalsReferral> = [];
  transportationReferrals: Array<any> = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private registrationService: RegistrationService,
    private authService: AuthService,
    private uniqueKeyService: UniqueKeyService,
  ) { }

  ngOnInit() {
    // get path for routing
    this.authService.path.subscribe(p => this.path = p);

    // get URL params
    this.regId = this.route.snapshot.paramMap.get('id');
    this.purchaser = this.route.snapshot.paramMap.get('purchaser');

    if (!this.regId || !this.purchaser) {
      // error - return to list
      this.router.navigate([`/${this.path}/registrations`]);
    }

    // get registration data
    this.registrationService.getRegistrationById(this.regId)
      .subscribe(r => {
        if (!r.essFileNumber) {
          // error - send them back to their home page
          this.router.navigate([`/${this.path}`]);
        } else {
          this.registration = r;
          if (this.registration.incidentTask) {
            // tslint:disable-next-line: no-string-literal
            this.registration.incidentTask['startDate'] = Date.now(); // TODO: fix when startDate is implemented in incidentTask object
          }
        }
      });
  }

  back() {
    // show the editing parts of the form
    this.editMode = true;
    window.scrollTo(0, 0); // scroll to top
  }

  cancel() {
    // navigate back to evacuee list
    this.router.navigate([`/${this.path}/registrations`]);
  }

  createReferral() {
    this.editMode = false;
    window.scrollTo(0, 0); // scroll to top
  }

  finalize() {
    this.submitting = true;
    // TODO: save stuff, etc
    this.submitting = false;

    // redirect to summary page
    // save the key for lookup
    this.uniqueKeyService.setKey(this.registration.id);
    this.router.navigate([`/${this.path}/registration/summary`]);
  }

  incidentalsReferralChange() {
    if (this.incidentalsReferrals.length > 0) {
      this.clearIncidentalsReferrals();
    } else {
      this.addIncidentalsReferral();
    }
  }

  removeIncidentalsReferral(i: number) {
    this.incidentalsReferrals.splice(i, 1);
  }

  addIncidentalsReferral() {
    this.incidentalsReferrals.push({
      id: null, // is populated back BE after save
      active: false,
      validFrom: new Date(2019, 0, 1), // TODO: for local testing only
      validTo: new Date(2019, 11, 31), // TODO: for local testing only
      evacuees: [],
      approvedItems: '',
      maxTotal: 100,
      comments: 'some comments here',
      purchaser: this.purchaser
    });
  }

  clearIncidentalsReferrals(): void {
    // TODO: replace confirm with a better popup
    if (confirm('Do you really want to clear all Incidentals referrals?')) {
      while (this.incidentalsReferrals.length > 0) { this.incidentalsReferrals.pop(); }
    }
  }

  // SAMPLE CODE
  // family member formgroup
  // createFamilyMember(fmbr?: FamilyMember): FormGroup {
  //   if (fmbr) {
  //     return this.formBuilder.group({
  //       bcServicesNumber: fmbr.bcServicesNumber || null,
  //       id: fmbr.id || null,
  //       active: fmbr.active || null,
  //       sameLastNameAsEvacuee: fmbr.sameLastNameAsEvacuee,
  //       firstName: [fmbr.firstName, Validators.required],
  //       lastName: [fmbr.lastName, Validators.required],
  //       nickname: fmbr.nickname,
  //       initials: fmbr.initials,
  //       gender: fmbr.gender,
  //       dob: [fmbr.dob, [Validators.required, CustomValidators.date('YYYY-MM-DD'), CustomValidators.maxDate(moment())]], // TODO: check this!!
  //       relationshipToEvacuee: [fmbr.relationshipToEvacuee, Validators.required],
  //     });
  //   } else {
  //     // make a new family member blank and return it.
  //     return this.formBuilder.group({
  //       sameLastNameAsEvacuee: true,
  //       firstName: ['', Validators.required],
  //       lastName: ['', Validators.required],
  //       initials: '',
  //       gender: null,
  //       dob: [null, [Validators.required, CustomValidators.date('YYYY-MM-DD'), CustomValidators.maxDate(moment())]], // TODO: Split into [DD] [MM] [YYYY]
  //       relationshipToEvacuee: [null, Validators.required],
  //     });
  //   }
  // }

}
