export interface AQMember {
    id?: number;
    TFSName?: string;
    fullName?: string;
    email?: string;
    phone?: string;
    avatar?: string;
    birthDate?: string;
    startDate?: string;
    nickName?: string;
    role?: string;
    isLeader?: boolean;
    isLunch?: boolean;
    WFHQuota?: number;
    absenceQuota?: number;
    isActive?: boolean;
}

export interface AQRole {
    role: string,
    code: string
}