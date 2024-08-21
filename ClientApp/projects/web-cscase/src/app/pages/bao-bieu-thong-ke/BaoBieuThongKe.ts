
export interface ReportLunchPayment {
    member: Member;
    total_IndividualDayOff: number;
    total_WorkingOnline: number;
    total_CommissionDay: number;
    total_AQDayOff: number;
    actual_workingDay: number;
}

export interface Member {
    id?: number;
    fullName?: string;
    nickName?: string;
}