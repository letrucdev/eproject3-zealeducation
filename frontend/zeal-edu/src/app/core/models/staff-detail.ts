import { Gender } from './gender';
import { UserRole } from './user-role';

export interface StaffDetail {
  staffId: string;
  userAccountId: string;
  username: string;
  fullName: string;
  email: string;
  phone: string;
  dob: string;
  gender: Gender;
  role: UserRole;
  isActive: boolean;
  position: string;
  department: string;
  joinedDate: string;
  lastLogin: string | null;
  facultyId: string | null;
  facultyCode: string | null;
  qualification: string | null;
  specialization: string | null;
  experienceYears: number | null;
}
