export interface IndividualDayOff {
    id?: number;
    date?: Date | string;
    member?: Member;
    reason?: string;
    periodType?: number;
    dayOffType?: number;
    isDayOffWithPayment?: boolean;
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
    date?: Date | string;
    memberId?: number;
    reason?: string;
    periodType?: number;
    dayOffType?: number;
    isDayOffWithPayment?: boolean;
    approvalStatus?: string;
    note?: string;
}
