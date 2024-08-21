export interface WorkingOT {
    id?: number;
    date?: Date | string;
    time?: number;
    member?: Member;
    note?: string;
}

export interface Member {
    id?: number;
    fullName?: string;
    nickName?: string;
}

export interface WorkingOT_API_DO {
    id?: number;
    date?: Date | string;
    time?: number;
    memberId?: number;
    note?: string;
}

