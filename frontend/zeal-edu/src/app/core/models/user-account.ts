import { CandidateInfo } from './candidate-info';
import { FacultyInfo } from './faculty-info';
import { Gender } from './gender';
import { StaffInfo } from './staff-info';
import { UserRole } from './user-role';

export interface UserAccount {
  id: string;
  username: string;
  fullName: string;
  email: string;
  phone: string;
  dob: string;
  gender: Gender;
  role: UserRole;
  isActive: boolean;
  lastLogin: string | null;
  staff?: StaffInfo;
  faculty?: FacultyInfo;
  candidate?: CandidateInfo;
}
