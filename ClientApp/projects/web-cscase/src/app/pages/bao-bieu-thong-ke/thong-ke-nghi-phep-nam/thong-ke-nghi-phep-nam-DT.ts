export interface IndividualQuotaReport {
    id?: number;
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

// export interface IndividualQuotaReport_API_DO {
//     id?: number;
//     fullName?: string;
//     nickName?: string;
//     absenceQuota?: number;
//     wfhQuota?: number;
//     dayOffs?: number;
// }