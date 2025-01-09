export interface IndividualQuotaReport {
    memberId?: number;
    fullName?: string;
    role?: string;
    employeeType?: number;
    //Absence data
    minAbsenceQuota?: number;
    additionalAbsenceQuota?: number;
    totalDayOff?: number;
    totalDayOffFullType1?: number;
    totalDayOffFullType2?: number;
    totalDayOffHalfType1?: number;
    totalDayOffHalfType2?: number;
    totalDayOffType3_4?: number;
    usedMinAbsenceQuota?: number;
    usedAdditionalAbsenceQuota?: number;
    remainMinAbsenceQuota?: number;
    remainAdditionalAbsenceQuota?: number;
    //WFH data
    minWfhQuota?: number;
    additionalWfhQuota?: number;
    totalWorkOnlineDay?: number;
    totalWorkOnlineDayFullType1?: number;
    totalWorkOnlineDayFullType2?: number;
    totalWorkOnlineDayHalfType1?: number;
    totalWorkOnlineDayHalfType2?: number;
    totalWorkOnlineDayType3_4?: number;
    usedMinWfhQuota?: number;
    usedAdditionalWfhQuota?: number;
    remainMinWfhQuota?: number;
    remainAdditionalWfhQuota?: number;
}

// export interface IndividualQuotaReport_API_DO {
//     id?: number;
//     fullName?: string;
//     nickName?: string;
//     absenceQuota?: number;
//     wfhQuota?: number;
//     dayOffs?: number;
// }