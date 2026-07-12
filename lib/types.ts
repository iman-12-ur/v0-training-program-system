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
  categories: string[];
  programType?: string;
  targetAudience?: string;
  duration: string;
  instructor: string;
  location: string;
  image?: string;
  logo?: string;
  batches: Batch[];
  prerequisites?: string[];
  objectives?: string[];
  topics?: string[];
  status: 'active' | 'inactive' | 'draft';
  registrationStartDate?: string;
  registrationEndDate?: string;
  createdAt: string;
}

export interface Registration {
  id: string;
  programId: string;
  programTitle: string;
  batchId: string;
  batchName: string;
  visitorName: string;
  employeeId: string;
  jobTitle: string;
  court: string;
  department: string;
  email: string;
  phone?: string;
  status: 'pending' | 'approved' | 'rejected' | 'completed';
  registeredAt: string;
  approvedBy?: string;
  approvedAt?: string;
  notes?: string;
}

export interface DashboardStats {
  totalPrograms: number;
  activePrograms: number;
  totalRegistrations: number;
  pendingApprovals: number;
  completedTrainings: number;
  totalEmployees: number;
}

export interface AdminUser {
  id: string;
  username: string;
  name: string;
  email: string;
  role: 'admin' | 'supervisor';
  permissions: {
    managePrograms: boolean;
    manageBatches: boolean;
    approveRegistrations: boolean;
    viewReports: boolean;
    manageUsers: boolean;
  };
  isActive: boolean;
  createdAt: string;
}
