import { FeedbackType } from './feedback-list-item';

export const FEEDBACK_TYPE_LABELS: Record<FeedbackType, string> = {
  Faculty: 'Faculty',
  Course: 'Course',
  General: 'General',
};

export const FEEDBACK_TYPE_BADGE_CLASSES: Record<FeedbackType, string> = {
  Faculty: 'bg-indigo-100 text-indigo-800',
  Course: 'bg-teal-100 text-teal-800',
  General: 'bg-slate-100 text-slate-800',
};
