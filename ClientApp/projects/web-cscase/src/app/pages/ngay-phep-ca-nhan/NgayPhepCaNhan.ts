export interface IndividualDayOff {
    id?: number;
    dateFrom?: Date | string;
    dateTo?: Date | string;
    sumDay?: number;
    member?: Member;
    reason?: string;
    isAnnual?: boolean;
    isWithoutPay?: boolean;
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
    memberId?: number;
    reason?: string;
    isAnnual?: boolean;
    isWithoutPay?: boolean;
    approvalStatus?: string;
    note?: string;
}
