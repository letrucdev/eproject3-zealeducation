export interface FacultyListItem {
  facultyId: string;
  facultyCode: string;
  fullName: string;
  email: string;
  phone: string;
  qualification: string;
  specialization: string;
  experienceYears: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}
