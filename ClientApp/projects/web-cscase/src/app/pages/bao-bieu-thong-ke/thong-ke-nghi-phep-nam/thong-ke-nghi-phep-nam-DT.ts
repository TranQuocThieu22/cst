export interface IndividualDayOffReport {
    fullName?: string;
    nickName?: string;
    absenceQuota?: number;
    dayOffs?: number;
    absenceQuotaLeft?: number;
    wfhQuota?: number;
    wfhQuotaNumber?: number;
    total_wfh?: number;
    wfhQuotaLeft?: number;
}

export interface IndividualDayOffReport_API_DO {
    fullName?: string;
    nickName?: string;
    absenceQuota?: number;
    wfhQuota?: number;
    dayOffs?: number;
}