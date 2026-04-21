'use client';

import { useState } from 'react';
import { PublicProgramsView } from '@/components/public-programs-view';
import { AdminLogin } from '@/components/admin-login';
import { Sidebar } from '@/components/sidebar';
import { StatsCards } from '@/components/stats-cards';
import { ProgramCard } from '@/components/program-card';
import { ProgramDetailsModal } from '@/components/program-details-modal';
import { PublicRegistrationModal } from '@/components/public-registration-modal';
import { RegistrationsTable } from '@/components/registrations-table';
import { AddProgramForm } from '@/components/add-program-form';
import { BatchesManager } from '@/components/batches-manager';
import { ReportsView } from '@/components/reports-view';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Search,
  Plus,
  Filter,
  BookOpen,
  TrendingUp,
  Users,
  Calendar,
  LogOut,
} from 'lucide-react';
import {
  mockPrograms,
  mockRegistrations,
  mockStats,
  categories,
} from '@/lib/mock-data';
import type { TrainingProgram, Batch, Registration, User } from '@/lib/types';

// Admin user
const adminUser: User = {
  id: 'admin',
  name: 'مدير النظام',
  email: 'admin@company.com',
  department: 'إدارة التدريب',
  role: 'admin',
};

export default function TrainingManagementSystem() {
  // View state: 'public' | 'login' | 'admin'
  const [viewMode, setViewMode] = useState<'public' | 'login' | 'admin'>('public');
  
  const [activeTab, setActiveTab] = useState('dashboard');
  const [programs, setPrograms] = useState(mockPrograms);
  const [registrations, setRegistrations] = useState(mockRegistrations);
  const [stats, setStats] = useState(mockStats);

  // Programs state
  const [selectedCategory, setSelectedCategory] = useState('الكل');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedProgram, setSelectedProgram] = useState<TrainingProgram | null>(null);
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const [isRegistrationModalOpen, setIsRegistrationModalOpen] = useState(false);
  const [selectedBatch, setSelectedBatch] = useState<Batch | null>(null);
  const [isAddProgramOpen, setIsAddProgramOpen] = useState(false);

  // Filter programs
  const filteredPrograms = programs.filter((program) => {
    const matchesCategory =
      selectedCategory === 'الكل' || program.category === selectedCategory;
    const matchesSearch =
      program.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      program.description.toLowerCase().includes(searchQuery.toLowerCase());
    return matchesCategory && matchesSearch;
  });

  // Handle public registration
  const handlePublicRegister = (
    registration: Omit<Registration, 'id' | 'status' | 'registeredAt' | 'approvedBy' | 'approvedAt'>
  ) => {
    const newRegistration: Registration = {
      ...registration,
      id: `reg-${Date.now()}`,
      status: 'pending',
      registeredAt: new Date().toISOString().split('T')[0],
    };

    setRegistrations((prev) => [...prev, newRegistration]);
    setStats((prev) => ({
      ...prev,
      totalRegistrations: prev.totalRegistrations + 1,
      pendingApprovals: prev.pendingApprovals + 1,
    }));

    // Update batch participants
    setPrograms((prev) =>
      prev.map((p) =>
        p.id === registration.programId
          ? {
              ...p,
              batches: p.batches.map((b) =>
                b.id === registration.batchId
                  ? { ...b, currentParticipants: b.currentParticipants + 1 }
                  : b
              ),
            }
          : p
      )
    );
  };

  // Handlers
  const handleViewDetails = (program: TrainingProgram) => {
    setSelectedProgram(program);
    setIsDetailsModalOpen(true);
  };

  const handleRegister = (program: TrainingProgram, batch?: Batch) => {
    setSelectedProgram(program);
    setSelectedBatch(batch || null);
    setIsDetailsModalOpen(false);
    setIsRegistrationModalOpen(true);
  };

  const handleApproveRegistration = (id: string) => {
    setRegistrations((prev) =>
      prev.map((reg) =>
        reg.id === id
          ? {
              ...reg,
              status: 'approved',
              approvedBy: adminUser.id,
              approvedAt: new Date().toISOString().split('T')[0],
            }
          : reg
      )
    );
    setStats((prev) => ({
      ...prev,
      pendingApprovals: prev.pendingApprovals - 1,
    }));
  };

  const handleRejectRegistration = (id: string) => {
    setRegistrations((prev) =>
      prev.map((reg) => (reg.id === id ? { ...reg, status: 'rejected' } : reg))
    );
    setStats((prev) => ({
      ...prev,
      pendingApprovals: prev.pendingApprovals - 1,
    }));
  };

  const handleAddProgram = (newProgram: TrainingProgram) => {
    setPrograms((prev) => [...prev, newProgram]);
    setStats((prev) => ({
      ...prev,
      totalPrograms: prev.totalPrograms + 1,
    }));
    setIsAddProgramOpen(false);
  };

  const handleAddBatch = (programId: string, batch: Omit<Batch, 'id'>) => {
    const newBatch: Batch = {
      ...batch,
      id: `batch-${Date.now()}`,
    };

    setPrograms((prev) =>
      prev.map((p) =>
        p.id === programId ? { ...p, batches: [...p.batches, newBatch] } : p
      )
    );
  };

  const handleUpdateBatchStatus = (
    programId: string,
    batchId: string,
    status: Batch['status']
  ) => {
    setPrograms((prev) =>
      prev.map((p) =>
        p.id === programId
          ? {
              ...p,
              batches: p.batches.map((b) =>
                b.id === batchId ? { ...b, status } : b
              ),
            }
          : p
      )
    );

    if (status === 'completed') {
      // Update registrations to completed
      setRegistrations((prev) =>
        prev.map((reg) =>
          reg.programId === programId && reg.batchId === batchId
            ? { ...reg, status: 'completed' }
            : reg
        )
      );
      // Update stats
      const completedCount = registrations.filter(
        (r) =>
          r.programId === programId &&
          r.batchId === batchId &&
          r.status === 'approved'
      ).length;
      setStats((prev) => ({
        ...prev,
        completedTrainings: prev.completedTrainings + completedCount,
      }));
    }
  };

  const handleLogout = () => {
    setViewMode('public');
    setActiveTab('dashboard');
  };

  // Public view
  if (viewMode === 'public') {
    return (
      <PublicProgramsView
        programs={programs}
        onRegister={handlePublicRegister}
        onAdminLogin={() => setViewMode('login')}
      />
    );
  }

  // Admin login view
  if (viewMode === 'login') {
    return (
      <AdminLogin
        onLogin={() => setViewMode('admin')}
      />
    );
  }

  // Admin dashboard
  const renderContent = () => {
    switch (activeTab) {
      case 'dashboard':
        return (
          <div className="space-y-8">
            {/* Welcome Section */}
            <div className="flex items-center justify-between">
              <div className="rounded-2xl bg-gradient-to-l from-primary/10 via-primary/5 to-transparent p-8 flex-1">
                <h1 className="text-3xl font-bold text-foreground">
                  مرحباً، {adminUser.name}
                </h1>
                <p className="mt-2 text-muted-foreground">
                  لوحة إدارة البرامج التدريبية - يمكنك إدارة البرامج والتسجيلات من هنا
                </p>
              </div>
              <Button
                variant="outline"
                onClick={handleLogout}
                className="mr-4 gap-2"
              >
                <LogOut className="h-4 w-4" />
                تسجيل الخروج
              </Button>
            </div>

            {/* Stats */}
            <StatsCards stats={stats} />

            {/* Quick Actions */}
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <Card
                className="cursor-pointer border-none shadow-sm transition-all hover:shadow-md"
                onClick={() => setActiveTab('programs')}
              >
                <CardContent className="flex items-center gap-4 p-6">
                  <div className="rounded-full bg-blue-500/10 p-3">
                    <BookOpen className="h-6 w-6 text-blue-600" />
                  </div>
                  <div>
                    <p className="font-medium text-foreground">إدارة البرامج</p>
                    <p className="text-sm text-muted-foreground">
                      {programs.filter((p) => p.status === 'active').length} برنامج نشط
                    </p>
                  </div>
                </CardContent>
              </Card>

              <Card
                className="cursor-pointer border-none shadow-sm transition-all hover:shadow-md"
                onClick={() => setActiveTab('registrations')}
              >
                <CardContent className="flex items-center gap-4 p-6">
                  <div className="rounded-full bg-amber-500/10 p-3">
                    <Calendar className="h-6 w-6 text-amber-600" />
                  </div>
                  <div>
                    <p className="font-medium text-foreground">
                      طلبات بانتظار الموافقة
                    </p>
                    <p className="text-sm text-muted-foreground">
                      {stats.pendingApprovals} طلب
                    </p>
                  </div>
                </CardContent>
              </Card>

              <Card
                className="cursor-pointer border-none shadow-sm transition-all hover:shadow-md"
                onClick={() => setActiveTab('batches')}
              >
                <CardContent className="flex items-center gap-4 p-6">
                  <div className="rounded-full bg-emerald-500/10 p-3">
                    <Users className="h-6 w-6 text-emerald-600" />
                  </div>
                  <div>
                    <p className="font-medium text-foreground">إدارة الدفعات</p>
                    <p className="text-sm text-muted-foreground">
                      تنظيم الدفعات
                    </p>
                  </div>
                </CardContent>
              </Card>

              <Card
                className="cursor-pointer border-none shadow-sm transition-all hover:shadow-md"
                onClick={() => setActiveTab('reports')}
              >
                <CardContent className="flex items-center gap-4 p-6">
                  <div className="rounded-full bg-violet-500/10 p-3">
                    <TrendingUp className="h-6 w-6 text-violet-600" />
                  </div>
                  <div>
                    <p className="font-medium text-foreground">التقارير</p>
                    <p className="text-sm text-muted-foreground">
                      عرض الإحصائيات
                    </p>
                  </div>
                </CardContent>
              </Card>
            </div>

            {/* Recent Registrations */}
            <div>
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-xl font-bold text-foreground">
                  أحدث طلبات الترشيح
                </h2>
                <Button variant="ghost" onClick={() => setActiveTab('registrations')}>
                  عرض الكل
                </Button>
              </div>
              <RegistrationsTable
                registrations={registrations.slice(0, 5)}
                programs={programs}
                onApprove={handleApproveRegistration}
                onReject={handleRejectRegistration}
              />
            </div>
          </div>
        );

      case 'programs':
        return (
          <div className="space-y-6">
            {/* Header */}
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <h2 className="text-2xl font-bold text-foreground">
                  البرامج التدريبية
                </h2>
                <p className="text-muted-foreground">
                  إدارة جميع البرامج التدريبية
                </p>
              </div>
              <Button onClick={() => setIsAddProgramOpen(true)}>
                <Plus className="ml-2 h-4 w-4" />
                إضافة برنامج
              </Button>
            </div>

            {/* Filters */}
            <div className="flex flex-col gap-4 sm:flex-row">
              <div className="relative flex-1">
                <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  placeholder="ابحث عن برنامج..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pr-9"
                />
              </div>
              <Select value={selectedCategory} onValueChange={setSelectedCategory}>
                <SelectTrigger className="w-full sm:w-48">
                  <Filter className="ml-2 h-4 w-4" />
                  <SelectValue placeholder="التصنيف" />
                </SelectTrigger>
                <SelectContent>
                  {categories.map((cat) => (
                    <SelectItem key={cat} value={cat}>
                      {cat}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Results Info */}
            <div className="flex items-center gap-2">
              <Badge variant="secondary">{filteredPrograms.length} برنامج</Badge>
              {selectedCategory !== 'الكل' && (
                <Badge variant="outline">{selectedCategory}</Badge>
              )}
            </div>

            {/* Programs Grid */}
            {filteredPrograms.length === 0 ? (
              <Card className="border-none shadow-sm">
                <CardContent className="flex flex-col items-center justify-center py-16">
                  <BookOpen className="h-12 w-12 text-muted-foreground" />
                  <h3 className="mt-4 text-lg font-medium text-foreground">
                    لا توجد برامج
                  </h3>
                  <p className="mt-2 text-muted-foreground">
                    لم يتم العثور على برامج تطابق معايير البحث
                  </p>
                </CardContent>
              </Card>
            ) : (
              <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                {filteredPrograms.map((program) => (
                  <ProgramCard
                    key={program.id}
                    program={program}
                    onViewDetails={handleViewDetails}
                    onRegister={() => handleRegister(program)}
                    isAdmin
                  />
                ))}
              </div>
            )}
          </div>
        );

      case 'registrations':
        return (
          <div className="space-y-6">
            <div>
              <h2 className="text-2xl font-bold text-foreground">
                إدارة طلبات الترشيح
              </h2>
              <p className="text-muted-foreground">
                مراجعة واعتماد طلبات الترشيح للبرامج التدريبية - يمكنك تصدير قائمة المقبولين بصيغة Excel
              </p>
            </div>
            <RegistrationsTable
              registrations={registrations}
              programs={programs}
              onApprove={handleApproveRegistration}
              onReject={handleRejectRegistration}
            />
          </div>
        );

      case 'batches':
        return (
          <BatchesManager
            programs={programs}
            onAddBatch={handleAddBatch}
            onUpdateBatchStatus={handleUpdateBatchStatus}
          />
        );

      case 'reports':
        return (
          <ReportsView
            programs={programs}
            registrations={registrations}
            stats={stats}
          />
        );

      case 'settings':
        return (
          <div className="space-y-6">
            <div>
              <h2 className="text-2xl font-bold text-foreground">الإعدادات</h2>
              <p className="text-muted-foreground">
                إدارة إعدادات النظام والتفضيلات
              </p>
            </div>
            <Card className="border-none shadow-sm">
              <CardHeader>
                <CardTitle>إعدادات النظام</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-muted-foreground">
                  سيتم إضافة خيارات الإعدادات قريباً...
                </p>
              </CardContent>
            </Card>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="flex min-h-screen flex-row-reverse bg-background">
      <Sidebar
        currentUser={adminUser}
        activeTab={activeTab}
        onTabChange={setActiveTab}
      />

      <main className="flex-1 overflow-auto p-4 lg:p-8">
        <div className="mx-auto max-w-7xl">{renderContent()}</div>
      </main>

      {/* Modals */}
      <ProgramDetailsModal
        program={selectedProgram}
        isOpen={isDetailsModalOpen}
        onClose={() => {
          setIsDetailsModalOpen(false);
          setSelectedProgram(null);
        }}
        onRegister={handleRegister}
      />

      <PublicRegistrationModal
        program={selectedProgram}
        selectedBatch={selectedBatch}
        isOpen={isRegistrationModalOpen}
        onClose={() => {
          setIsRegistrationModalOpen(false);
          setSelectedProgram(null);
          setSelectedBatch(null);
        }}
        onConfirm={(reg) => {
          handlePublicRegister(reg);
          setIsRegistrationModalOpen(false);
          setSelectedProgram(null);
          setSelectedBatch(null);
        }}
      />

      <AddProgramForm
        isOpen={isAddProgramOpen}
        onClose={() => setIsAddProgramOpen(false)}
        onSubmit={handleAddProgram}
      />
    </div>
  );
}
