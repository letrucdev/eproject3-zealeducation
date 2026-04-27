export interface CourseListItem {
  courseId: string;
  courseName: string;
  description: string | null;
  durationWeeks: number;
  baseFee: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}
