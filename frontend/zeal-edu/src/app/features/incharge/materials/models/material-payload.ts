export interface CourseListQuery {
  page: number;
  pageSize: number;
  search?: string;
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
