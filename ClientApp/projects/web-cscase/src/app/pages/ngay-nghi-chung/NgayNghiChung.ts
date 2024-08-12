export interface DayOff {
    id?: number;
    datefrom?: Date | string;
    dateto?: Date | string;
    sumday?: number;
    reason?: string;
    note?: string;
}
