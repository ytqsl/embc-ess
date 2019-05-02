import { Evacuee, Supplier } from './';

export interface IncidentalsReferral {
  id: string;
  active?: boolean;
  purchaser: string;
  validFrom: Date;
  validTo: Date;
  evacuees: Array<{
    evacuee: Evacuee,
    selected: boolean
  }>;
  approvedItems: string;
  totalAmt: number;
  supplier: Supplier;
  comments: string;
}
