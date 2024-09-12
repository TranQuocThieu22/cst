export interface AQMember {
    id?: number;
    tfsName?: string;
    fullName?: string;
    email?: string;
    phone?: string;
    avatar?: string;
    birthDate?: Date | string;
    startDate?: Date | string;
    nickName?: string;
    role?: string;
    isLeader?: boolean;
    isLunchStatus?: boolean;
    detailLunch?: detailLunch[];
    detailWFHQuota?: detailWFHQuota;
    detailAbsenceQuota?: detailAbsenceQuota;
    isActive?: boolean;
    maSoCCCD?: string;
    address?: string;
    workingYear?: number;
    detailContract?: detailContract;
}

export interface AQRole {
    role: string,
    code: string,
    total: number
}

export interface AQMemberInsertDO {
    tfsName?: string;
    fullName?: string;
    email?: string;
    phone?: string;
    avatar?: string;
    birthDate?: Date | string;
    startDate?: Date | string;
    nickName?: string;
    role?: string;
    isLeader?: boolean;
    isLunchStatus?: boolean;
    minWFHQuota?: number;
    minAbsenceQuota?: number;
    isActive?: boolean;
    maSoCCCD?: string;
    address?: string;
    workingYears?: number;
    detailContract?: detailContract;
}

export interface AQMemberUpdateDO {
    id?: number;
    tfsName?: string;
    fullName?: string;
    email?: string;
    phone?: string;
    avatar?: string;
    birthDate?: Date | string;
    startDate?: Date | string;
    nickName?: string;
    role?: string;
    isLeader?: boolean;
    isLunchStatus?: boolean;
    detailLunch?: detailLunch[];
    detailWFHQuota?: detailWFHQuota;
    detailAbsenceQuota?: detailAbsenceQuota;
    isActive?: boolean;
    maSoCCCD?: string;
    address?: string;
    workingYear?: number;
    detailContract?: detailContract;
}

export interface detailContract {
    contractStartDate?: Date | string;
    contractExpireDate?: Date | string;
    contractDuration?: number;
    contractType?: string;
}

export interface detailLunch {
    year?: number
    lunchByMonth?: lunchByMonth[];
}

export interface lunchByMonth {
    month?: number;
    isLunch?: boolean;
    lunchFee?: number;
    note?: string;
}

export interface detailWFHQuota {
    minWFHQuota?: number;
    actualWFHQuotaByYear?: actualWFHQuotaByYear[];
}

export interface actualWFHQuotaByYear {
    year?: number;
    WFHQuota?: number;
}

export interface detailAbsenceQuota {
    minAbsenceQuota?: number;
    actualAbsenceQuotaByYear?: actualAbsenceQuotaByYear;
}

export interface actualAbsenceQuotaByYear {
    year?: number;
    absenceQuota?: number;
}