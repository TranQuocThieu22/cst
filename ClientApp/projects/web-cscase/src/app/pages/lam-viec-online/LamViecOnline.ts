export interface WorkingOnline {
    id?: number;
    dateFrom?: Date | string;
    dateTo?: Date | string;
    sumDay?: number;
    member?: Member;
    reason?: string;
    approvalStatus?: string;
    note?: string;
}

export interface Member {
    id?: number;
    fullName?: string;
    nickName?: string;
}

export interface WorkingOnline_API_DO {
    id?: number;
    dateFrom?: Date | string;
    dateTo?: Date | string;
    sumDay?: number;
    memberId?: number;
    reason?: string;
    approvalStatus?: string;
    note?: string;
}
