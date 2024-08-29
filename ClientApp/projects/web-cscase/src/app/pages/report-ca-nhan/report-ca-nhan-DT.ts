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
    isLunch?: boolean;
    wfhQuota?: number;
    absenceQuota?: number;
    isActive?: boolean;
}

export interface AQRole {
    role: string,
    code: string,
    total: number
}