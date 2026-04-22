import { Gender } from '../../../../core/models/gender';
import { UserRole } from '../../../../core/models/user-role';

export interface CreateStaffPayload {
  username: string;
  password: string;
  fullName: string;
  email: string;
  phone: string;
  dob: string;
  gender: Gender;
  role: UserRole;
  position: string;
  department: string;
  joinedDate: string | null;
}

export interface UpdateStaffPayload {
  fullName: string;
  email: string;
  phone: string;
  dob: string;
  gender: Gender;
  role: UserRole;
  position: string;
  department: string;
  joinedDate: string;
  isActive: boolean;
}

export interface CreateFacultyPayload {
  username: string;
  password: string;
  fullName: string;
  email: string;
  phone: string;
  dob: string;
  gender: Gender;
  position: string;
  department: string;
  joinedDate: string | null;
  facultyCode: string;
  qualification: string;
  specialization: string;
  experienceYears: number;
}

export interface UpdateFacultyPayload {
  fullName: string;
  email: string;
  phone: string;
  dob: string;
  gender: Gender;
  position: string;
  department: string;
  joinedDate: string;
  isActive: boolean;
  facultyCode: string;
  qualification: string;
  specialization: string;
  experienceYears: number;
}

export interface StaffListQuery {
  page: number;
  pageSize: number;
  search?: string;
  role?: UserRole;
  isActive?: boolean;
}
