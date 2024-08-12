export interface DayOffIndividual {
    id?: number;
    datefrom?: Date | string;
    dateto?: Date | string;
    sumday?: number;
    reason?: string;
    annual?: boolean;
    withoutday?: boolean;
    status?: number;
    note?: string;
}
