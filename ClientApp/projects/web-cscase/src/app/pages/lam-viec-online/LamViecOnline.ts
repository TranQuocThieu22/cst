export interface WorkingOnline {
    id?: number;
    date?: Date | string;
    member?: Member;
    reason?: string;
    periodType?: number;
    wfhType?: number;
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
    date?: Date | string;
    memberId?: number;
    reason?: string;
    periodType?: number;
    wfhType?: number;
    approvalStatus?: string;
    note?: string;
}
