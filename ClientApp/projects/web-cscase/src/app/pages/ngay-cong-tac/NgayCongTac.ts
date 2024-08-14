export interface CommissionDay {
    id?: number;
    dateFrom?: Date | string;
    dateTo?: Date | string;
    sumDay?: number;
    comissionContent?: string;
    transportation?: string;
    memberList?: CommissionMember[];
    commissionExpenses?: number;
    note?: string;
}

export interface CommissionMember {
    fullName?: string;
    nickName?: string;
    memberExpenses?: number;
}
