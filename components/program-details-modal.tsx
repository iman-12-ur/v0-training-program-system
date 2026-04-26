'use client';

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import {
  Calendar,
  Clock,
  MapPin,
  User,
  Users,
  CheckCircle2,
  AlertCircle,
  BookOpen,
  Target,
  Tag,
  Briefcase,
} from 'lucide-react';
import type { TrainingProgram, Batch } from '@/lib/types';

interface ProgramDetailsModalProps {
  program: TrainingProgram | null;
  isOpen: boolean;
  onClose: () => void;
  onRegister?: (program: TrainingProgram, batch: Batch) => void;
}

export function ProgramDetailsModal({
  program,
  isOpen,
  onClose,
  onRegister,
}: ProgramDetailsModalProps) {
  if (!program) return null;

  const getBatchStatusBadge = (status: Batch['status']) => {
    switch (status) {
      case 'upcoming':
        return <Badge className="bg-blue-500">قادم</Badge>;
      case 'ongoing':
        return <Badge className="bg-emerald-500">جاري</Badge>;
      case 'completed':
        return <Badge variant="secondary">مكتمل</Badge>;
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] p-0" style={{ direction: 'rtl' }}>
        <DialogHeader className="border-b p-6 pb-4">
          <div className="flex items-start justify-between gap-4 flex-row-reverse">
            {program.logo && (
              <img
                src={program.logo}
                alt="شعار البرنامج"
                className="h-16 w-16 rounded-lg object-contain border bg-white shrink-0"
              />
            )}
            <div className="flex-1 text-right">
              <DialogTitle className="text-2xl font-bold mb-2">
                {program.title}
              </DialogTitle>
              <div className="flex flex-wrap gap-2 justify-end">
                {program.categories?.map((cat) => (
                  <Badge key={cat} variant="secondary">
                    {cat}
                  </Badge>
                ))}
                {program.programType && (
                  <Badge variant="outline" className="border-primary text-primary">
                    {program.programType}
                  </Badge>
                )}
              </div>
            </div>
          </div>
        </DialogHeader>

        <ScrollArea className="max-h-[60vh]">
          <div className="space-y-6 p-6" style={{ direction: 'rtl' }}>
            
            {/* 1. Description - وصف البرنامج */}
            <div className="rounded-lg border p-4">
              <h4 className="mb-2 font-semibold text-foreground text-right">وصف البرنامج</h4>
              <p className="text-muted-foreground text-right leading-relaxed">{program.description}</p>
            </div>

            {/* 2. Target Audience - الفئة المستهدفة */}
            {program.targetAudience && (
              <div className="rounded-lg border p-4 bg-primary/5">
                <h4 className="mb-2 font-semibold text-foreground flex items-center gap-2 justify-end flex-row-reverse">
                  <Users className="h-5 w-5 text-primary" />
                  الفئة المستهدفة
                </h4>
                <p className="text-muted-foreground text-right">{program.targetAudience}</p>
              </div>
            )}

            {/* 3. Program Details Grid - تفاصيل البرنامج */}
            <div className="grid gap-4 sm:grid-cols-2" style={{ direction: 'rtl' }}>
              {/* نوع البرنامج */}
              {program.programType && (
                <div className="flex items-center gap-3 rounded-lg bg-muted/50 p-4 flex-row-reverse">
                  <div className="rounded-full bg-primary/10 p-2">
                    <Briefcase className="h-5 w-5 text-primary" />
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-muted-foreground">نوع البرنامج</p>
                    <p className="font-medium">{program.programType}</p>
                  </div>
                </div>
              )}
              
              {/* المدرب */}
              <div className="flex items-center gap-3 rounded-lg bg-muted/50 p-4 flex-row-reverse">
                <div className="rounded-full bg-primary/10 p-2">
                  <User className="h-5 w-5 text-primary" />
                </div>
                <div className="text-right">
                  <p className="text-sm text-muted-foreground">المدرب</p>
                  <p className="font-medium">{program.instructor}</p>
                </div>
              </div>
              
              {/* المدة */}
              <div className="flex items-center gap-3 rounded-lg bg-muted/50 p-4 flex-row-reverse">
                <div className="rounded-full bg-primary/10 p-2">
                  <Clock className="h-5 w-5 text-primary" />
                </div>
                <div className="text-right">
                  <p className="text-sm text-muted-foreground">المدة</p>
                  <p className="font-medium">{program.duration}</p>
                </div>
              </div>
              
              {/* الموقع */}
              <div className="flex items-center gap-3 rounded-lg bg-muted/50 p-4 flex-row-reverse">
                <div className="rounded-full bg-primary/10 p-2">
                  <MapPin className="h-5 w-5 text-primary" />
                </div>
                <div className="text-right">
                  <p className="text-sm text-muted-foreground">الموقع</p>
                  <p className="font-medium">{program.location}</p>
                </div>
              </div>
            </div>

            {/* 4. Objectives - أهداف البرنامج */}
            {program.objectives && program.objectives.length > 0 && program.objectives.some(o => o.trim()) && (
              <div className="rounded-lg border p-4">
                <h4 className="mb-3 font-semibold text-foreground flex items-center gap-2 justify-end flex-row-reverse">
                  <Target className="h-5 w-5 text-primary" />
                  أهداف البرنامج
                </h4>
                <ul className="space-y-2">
                  {program.objectives.filter(o => o.trim()).map((obj, index) => (
                    <li
                      key={index}
                      className="flex items-start gap-2 text-muted-foreground flex-row-reverse text-right"
                    >
                      <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-emerald-500" />
                      <span>{obj}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* 5. Topics - محاور البرنامج */}
            {program.topics && program.topics.length > 0 && program.topics.some(t => t.trim()) && (
              <div className="rounded-lg border p-4 bg-blue-50/50 border-blue-200">
                <h4 className="mb-3 font-semibold text-foreground flex items-center gap-2 justify-end flex-row-reverse">
                  <BookOpen className="h-5 w-5 text-blue-600" />
                  محاور البرنامج
                </h4>
                <ul className="space-y-2">
                  {program.topics.filter(t => t.trim()).map((topic, index) => (
                    <li
                      key={index}
                      className="flex items-start gap-2 text-muted-foreground flex-row-reverse text-right"
                    >
                      <div className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-blue-600" />
                      <span>{topic}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* 6. Prerequisites - المتطلبات المسبقة */}
            {program.prerequisites && program.prerequisites.length > 0 && program.prerequisites.some(p => p.trim()) && (
              <div className="rounded-lg border p-4 border-amber-200 bg-amber-50/50">
                <h4 className="mb-3 font-semibold text-foreground flex items-center gap-2 justify-end flex-row-reverse">
                  <AlertCircle className="h-5 w-5 text-amber-500" />
                  المتطلبات المسبقة
                </h4>
                <ul className="space-y-2">
                  {program.prerequisites.filter(p => p.trim()).map((req, index) => (
                    <li
                      key={index}
                      className="flex items-start gap-2 text-muted-foreground flex-row-reverse text-right"
                    >
                      <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-amber-500" />
                      <span>{req}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* 7. Batches - الدفعات */}
            {program.batches.length > 0 && (
              <div className="rounded-lg border p-4">
                <h4 className="mb-3 font-semibold text-foreground flex items-center gap-2 justify-end flex-row-reverse">
                  <Calendar className="h-5 w-5 text-primary" />
                  الدفعات المتاحة
                </h4>
                <div className="space-y-3">
                  {program.batches.map((batch) => {
                    const isFull =
                      batch.currentParticipants >= batch.maxParticipants;
                    const canRegister =
                      batch.status === 'upcoming' && !isFull;

                    return (
                      <div
                        key={batch.id}
                        className="flex flex-wrap items-center justify-between gap-4 rounded-lg border p-4 flex-row-reverse"
                      >
                        <div className="space-y-1 text-right">
                          <div className="flex items-center gap-2 flex-row-reverse">
                            <span className="font-medium">{batch.name}</span>
                            {getBatchStatusBadge(batch.status)}
                          </div>
                          <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground flex-row-reverse">
                            <span className="flex items-center gap-1 flex-row-reverse">
                              <Calendar className="h-4 w-4" />
                              {batch.startDate} - {batch.endDate}
                            </span>
                            <span className="flex items-center gap-1 flex-row-reverse">
                              <Users className="h-4 w-4" />
                              {batch.currentParticipants}/{batch.maxParticipants}{' '}
                              مشارك
                            </span>
                          </div>
                        </div>
                        {onRegister && canRegister && (
                          <Button
                            size="sm"
                            onClick={() => onRegister(program, batch)}
                          >
                            تسجيل
                          </Button>
                        )}
                        {isFull && batch.status === 'upcoming' && (
                          <Badge variant="outline" className="text-red-500">
                            مكتمل العدد
                          </Badge>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        </ScrollArea>
      </DialogContent>
    </Dialog>
  );
}
