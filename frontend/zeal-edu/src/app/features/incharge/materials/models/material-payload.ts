export interface CourseListQuery {
  page: number;
  pageSize: number;
  search?: string;
}

export interface CourseMaterialsQuery {
  page: number;
  pageSize: number;
  search?: string;
  includeInactive?: boolean;
}

export interface MaterialCourseListItem {
  courseId: string;
  courseName: string;
  totalMaterials: number;
}

export interface CreateMaterialsPayload {
  title: string;
  courseIds: string[];
  file: File;
}

export interface UpdateMaterialTitlePayload {
  title: string;
}

export interface ReplaceMaterialFilePayload {
  file: File;
}

export interface ToggleMaterialActivePayload {
  isActive: boolean;
}

export interface StudyMaterialListItem {
  materialId: string;
  courseId: string;
  title: string;
  fileName: string;
  fileType: string;
  fileSizeMb: number;
  isActive: boolean;
  uploadedAt: string;
  uploadedByName: string;
}

export interface CreateMaterialsResponse {
  materialIds: string[];
  filePath: string;
  fileSizeMb: number;
}

export interface MaterialSiblingCourse {
  materialId: string;
  courseId: string;
  courseName: string;
}
