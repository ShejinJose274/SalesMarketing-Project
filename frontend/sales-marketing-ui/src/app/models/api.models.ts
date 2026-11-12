export type UserRole = 'MasterAdmin' | 'SalesManagerAdmin';

export interface CurrentUser {
  username: string;
  role: UserRole;
}

export interface LoginResponse extends CurrentUser {
  dashboardUrl: string;
}

export interface Salesman {
  id: number;
  salesmancode: string;
  salesmanname: string;
  mobileno: string | null;
  email: string | null;
  joiningdate: string | null;
  status: number;
  createddate: string;
  modifieddate: string | null;
}

export interface MarketingTeamListItem {
  id: number;
  teamcode: string;
  teamname: string;
  teamleaderid: number;
  teamleadername: string;
  salesmancount: number;
  status: number;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface SalesmanRequest {
  salesmancode: string;
  salesmanname: string;
  mobileno: string | null;
  email: string | null;
  joiningdate: string | null;
  status: number;
}

export interface TeamMemberRequest {
  salesmanid: number;
  role: number;
  targetamount: number;
  status: number;
}

export interface TeamMember extends TeamMemberRequest {
  salesmanname: string;
}

export interface MarketingTeam {
  id: number;
  teamcode: string;
  teamname: string;
  teamleaderid: number;
  teamleadername: string;
  description: string | null;
  status: number;
  createddate: string;
  modifieddate: string | null;
  members: TeamMember[];
}

export interface MarketingTeamRequest {
  teamcode: string;
  teamname: string;
  teamleaderid: number;
  description: string | null;
  members: TeamMemberRequest[];
  status: number;
}

export const SALESMAN_ROLES: Record<number, string> = {
  1: 'Team leader',
  2: 'Senior sales executive',
  3: 'Sales executive',
  4: 'Trainee',
};

export const STATUS_LABELS: Record<number, string> = {
  1: 'Active',
  2: 'Inactive',
};
