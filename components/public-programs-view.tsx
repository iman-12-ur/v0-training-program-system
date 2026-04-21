'use client';

import { useState } from 'react';
import { ProgramCard } from '@/components/program-card';
import { ProgramDetailsModal } from '@/components/program-details-modal';
import { PublicRegistrationModal } from '@/components/public-registration-modal';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Search, Filter, BookOpen, GraduationCap, Users, Award } from 'lucide-react';
import { categories } from '@/lib/mock-data';
import type { TrainingProgram, Batch, Registration } from '@/lib/types';

interface PublicProgramsViewProps {
  programs: TrainingProgram[];
  onRegister: (registration: Omit<Registration, 'id' | 'status' | 'registeredAt' | 'approvedBy' | 'approvedAt'>) => void;
  onAdminLogin: () => void;
}

export function PublicProgramsView({
  programs,
  onRegister,
  onAdminLogin,
}: PublicProgramsViewProps) {
  const [selectedCategory, setSelectedCategory] = useState('الكل');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedProgram, setSelectedProgram] = useState<TrainingProgram | null>(null);
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const [isRegistrationModalOpen, setIsRegistrationModalOpen] = useState(false);
  const [selectedBatch, setSelectedBatch] = useState<Batch | null>(null);

  // Filter active programs only
  const activePrograms = programs.filter((p) => p.status === 'active');
  
  const filteredPrograms = activePrograms.filter((program) => {
    const matchesCategory =
      selectedCategory === 'الكل' || program.category === selectedCategory;
    const matchesSearch =
      program.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      program.description.toLowerCase().includes(searchQuery.toLowerCase());
    return matchesCategory && matchesSearch;
  });

  // Stats
  const totalOpenBatches = activePrograms.reduce(
    (acc, p) => acc + p.batches.filter((b) => b.status === 'upcoming').length,
    0
  );
  const totalAvailableSeats = activePrograms.reduce(
    (acc, p) =>
      acc +
      p.batches
        .filter((b) => b.status === 'upcoming')
        .reduce((sum, b) => sum + (b.maxParticipants - b.currentParticipants), 0),
    0
  );

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

  const handleConfirmRegistration = (
    registration: Omit<Registration, 'id' | 'status' | 'registeredAt' | 'approvedBy' | 'approvedAt'>
  ) => {
    onRegister(registration);
    setIsRegistrationModalOpen(false);
    setSelectedProgram(null);
    setSelectedBatch(null);
  };

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="border-b bg-card">
        <div className="mx-auto max-w-7xl px-4 py-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary">
                <GraduationCap className="h-6 w-6 text-primary-foreground" />
              </div>
              <div>
                <h1 className="text-lg font-bold text-foreground">البرامج التدريبية</h1>
                <p className="text-xs text-muted-foreground">سجّل في البرامج المتاحة</p>
              </div>
            </div>
            <Button variant="outline" onClick={onAdminLogin}>
              دخول المدير
            </Button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        {/* Hero Section */}
        <div className="mb-8 rounded-2xl bg-gradient-to-l from-primary/10 via-primary/5 to-transparent p-8">
          <h2 className="text-3xl font-bold text-foreground">
            مرحباً بك في بوابة التدريب
          </h2>
          <p className="mt-2 max-w-2xl text-muted-foreground">
            استعرض البرامج التدريبية المتاحة وقدّم طلب ترشيحك للبرنامج المناسب.
            سيتم مراجعة طلبك وإبلاغك بالنتيجة عبر البريد الإلكتروني.
          </p>
        </div>

        {/* Stats */}
        <div className="mb-8 grid gap-4 sm:grid-cols-3">
          <Card className="border-none shadow-sm">
            <CardContent className="flex items-center gap-4 p-6">
              <div className="rounded-full bg-blue-500/10 p-3">
                <BookOpen className="h-6 w-6 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-foreground">{activePrograms.length}</p>
                <p className="text-sm text-muted-foreground">برنامج متاح</p>
              </div>
            </CardContent>
          </Card>
          <Card className="border-none shadow-sm">
            <CardContent className="flex items-center gap-4 p-6">
              <div className="rounded-full bg-emerald-500/10 p-3">
                <Award className="h-6 w-6 text-emerald-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-foreground">{totalOpenBatches}</p>
                <p className="text-sm text-muted-foreground">دفعة مفتوحة للتسجيل</p>
              </div>
            </CardContent>
          </Card>
          <Card className="border-none shadow-sm">
            <CardContent className="flex items-center gap-4 p-6">
              <div className="rounded-full bg-amber-500/10 p-3">
                <Users className="h-6 w-6 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-foreground">{totalAvailableSeats}</p>
                <p className="text-sm text-muted-foreground">مقعد متاح</p>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Filters */}
        <div className="mb-6 flex flex-col gap-4 sm:flex-row">
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
        <div className="mb-4 flex items-center gap-2">
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
              />
            ))}
          </div>
        )}
      </main>

      {/* Footer */}
      <footer className="border-t bg-card mt-12">
        <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
          <p className="text-center text-sm text-muted-foreground">
            نظام إدارة البرامج التدريبية - جميع الحقوق محفوظة
          </p>
        </div>
      </footer>

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
        onConfirm={handleConfirmRegistration}
      />
    </div>
  );
}
