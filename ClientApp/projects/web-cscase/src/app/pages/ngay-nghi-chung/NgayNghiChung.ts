export interface DayOff {
    id?: number;
    dateFrom?: Date | string;
    dateTo?: Date | string;
    sumDay?: number;
    reason?: string;
    note?: string;
}
