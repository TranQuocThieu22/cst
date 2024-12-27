
export interface LunchPaymentReport {
    id: number;
    fullName?: string;
    nickName?: string;
    total_IndividualDayOff?: number;
    total_WorkingOnline?: number;
    total_IndividualDayOff_full?: number;
    total_IndividualDayOff_half?: number;
    total_WorkingOnline_full?: number;
    total_WorkingOnline_half?: number;
    total_CommissionDay_full?: number;
    total_CommissionDay_half?: number;
    total_AQDayOff?: number;
    office_workingDay?: number;
}
