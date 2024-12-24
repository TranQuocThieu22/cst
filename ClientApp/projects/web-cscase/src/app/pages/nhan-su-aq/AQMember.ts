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
  minWFHQuota?: number;
  additionalWFHQuota?: number;
  minAbsenceQuota?: number;
  additionalAbsenceQuota?: number;
  isActive?: boolean;
  maSoCCCD?: string;
  address?: string;
  workingYear?: number;
  contractStartDate?: Date | string;
  contractExpireDate?: Date | string;
  contractType?: string;
}

export interface AQRole {
  role: string;
  code: string;
  total: number;
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
  contractStartDate?: Date | string;
  contractExpireDate?: Date | string;
  contractType?: string;
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
  minWFHQuota?: number;
  minAbsenceQuota?: number;
  isActive?: boolean;
  maSoCCCD?: string;
  address?: string;
  contractStartDate?: Date | string;
  contractExpireDate?: Date | string;
  contractType?: string;
}