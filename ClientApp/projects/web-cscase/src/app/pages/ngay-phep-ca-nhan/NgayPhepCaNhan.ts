export interface IndividualDayOff {
    id?: number;
    dateFrom?: Date | string;
    dateTo?: Date | string;
    sumDay?: number;
    numberOfDay_whole?: number;
    numberOfDay_half?: number;
    member?: Member;
    reason?: string;
    isAnnual?: boolean;
    totalIsAnnual?: number;
    isWithoutPay?: boolean;
    totalIsWithoutPay?: number;
    approvalStatus?: string;
    note?: string;
}

export interface Member {
    id?: number;
    fullName?: string;
    nickName?: string;
}

export interface IndividualDayOff_API_DO {
    id?: number;
    dateFrom?: Date | string;
    dateTo?: Date | string;
    sumDay?: number;
    numberOfDay_whole?: number;
    numberOfDay_half?: number;
    memberId?: number;
    reason?: string;
    isAnnual?: boolean;
    totalIsAnnual?: number;
    isWithoutPay?: boolean;
    totalIsWithoutPay?: number;
    approvalStatus?: string;
    note?: string;
}
