// أنواع البيانات للنظام

export type UserRole = 'employee' | 'supervisor' | 'admin';

export interface User {
  id: string;
  name: string;
  email: string;
  department: string;
  role: UserRole;
  avatar?: string;
}

export interface Batch {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  maxParticipants: number;
  currentParticipants: number;
  status: 'upcoming' | 'ongoing' | 'completed';
}

export interface TrainingProgram {
  id: string;
  title: string;
  description: string;
  category: string;
  duration: string;
  instructor: string;
  location: string;
  image?: string;
  batches: Batch[];
  prerequisites?: string[];
  objectives?: string[];
  status: 'active' | 'inactive' | 'draft';
  createdAt: string;
}

export interface Registration {
  id: string;
  programId: string;
  programTitle: string;
  batchId: string;
  batchName: string;
  userId: string;
  userName: string;
  userDepartment: string;
  status: 'pending' | 'approved' | 'rejected' | 'completed';
  registeredAt: string;
  approvedBy?: string;
  approvedAt?: string;
}

export interface DashboardStats {
  totalPrograms: number;
  activePrograms: number;
  totalRegistrations: number;
  pendingApprovals: number;
  completedTrainings: number;
  totalEmployees: number;
}
