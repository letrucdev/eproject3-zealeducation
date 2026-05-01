export interface CourseMaterialsQuery {
  page: number;
  pageSize: number;
  search?: string;
  includeInactive?: boolean;
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
