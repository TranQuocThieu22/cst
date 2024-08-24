
export interface LunchPaymentReport {
    id: number;
    fullName?: string;
    nickName?: string;
    total_IndividualDayOff: number;
    total_WorkingOnline: number;
    total_CommissionDay: number;
    total_AQDayOff: number;
    actual_workingDay: number;
}
