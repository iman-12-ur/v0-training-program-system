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
      <DialogContent className="max-w-2xl max-h-[90vh] p-0">
        <DialogHeader className="border-b p-6 pb-4">
          <div className="flex items-start justify-between gap-4">
            <div className="flex-1">
              <div className="flex flex-wrap gap-2 mb-2">
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
              <DialogTitle className="text-2xl font-bold">
                {program.title}
              </DialogTitle>
              {program.targetAudience && (
                <p className="mt-2 text-sm text-primary">
                  الفئة المستهدفة: {program.targetAudience}
                </p>
              )}
            </div>
            {program.logo && (
              <img
                src={program.logo}
                alt="شعار البرنامج"
                className="h-16 w-16 rounded-lg object-contain border bg-white shrink-0"
              />
            )}
          </div>
        </DialogHeader>

        <ScrollArea className="max-h-[60vh]">
          <div className="space-y-6 p-6">
            {/* Description */}
            <div>
              <h4 className="mb-2 font-semibold text-foreground">وصف البرنامج</h4>
              <p className="text-muted-foreground">{program.description}</p>
            </div>

            {/* Details Grid */}
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="flex items-center gap-3 rounded-lg bg-muted/50 p-4">
                <div className="rounded-full bg-primary/10 p-2">
                  <Clock className="h-5 w-5 text-primary" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">المدة</p>
                  <p className="font-medium">{program.duration}</p>
                </div>
              </div>
              <div className="flex items-center gap-3 rounded-lg bg-muted/50 p-4">
                <div className="rounded-full bg-primary/10 p-2">
                  <User className="h-5 w-5 text-primary" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">المدرب</p>
                  <p className="font-medium">{program.instructor}</p>
                </div>
              </div>
              <div className="flex items-center gap-3 rounded-lg bg-muted/50 p-4 sm:col-span-2">
                <div className="rounded-full bg-primary/10 p-2">
                  <MapPin className="h-5 w-5 text-primary" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">الموقع</p>
                  <p className="font-medium">{program.location}</p>
                </div>
              </div>
            </div>

            {/* Objectives */}
            {program.objectives && program.objectives.length > 0 && (
              <div>
                <h4 className="mb-3 font-semibold text-foreground">
                  أهداف البرنامج
                </h4>
                <ul className="space-y-2">
                  {program.objectives.map((obj, index) => (
                    <li
                      key={index}
                      className="flex items-start gap-2 text-muted-foreground"
                    >
                      <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-emerald-500" />
                      <span>{obj}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Prerequisites */}
            {program.prerequisites && program.prerequisites.length > 0 && (
              <div>
                <h4 className="mb-3 font-semibold text-foreground">
                  المتطلبات المسبقة
                </h4>
                <ul className="space-y-2">
                  {program.prerequisites.map((req, index) => (
                    <li
                      key={index}
                      className="flex items-start gap-2 text-muted-foreground"
                    >
                      <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-amber-500" />
                      <span>{req}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Batches */}
            {program.batches.length > 0 && (
              <div>
                <h4 className="mb-3 font-semibold text-foreground">الدفعات</h4>
                <div className="space-y-3">
                  {program.batches.map((batch) => {
                    const isFull =
                      batch.currentParticipants >= batch.maxParticipants;
                    const canRegister =
                      batch.status === 'upcoming' && !isFull;

                    return (
                      <div
                        key={batch.id}
                        className="flex flex-wrap items-center justify-between gap-4 rounded-lg border p-4"
                      >
                        <div className="space-y-1">
                          <div className="flex items-center gap-2">
                            <span className="font-medium">{batch.name}</span>
                            {getBatchStatusBadge(batch.status)}
                          </div>
                          <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
                            <span className="flex items-center gap-1">
                              <Calendar className="h-4 w-4" />
                              {batch.startDate} - {batch.endDate}
                            </span>
                            <span className="flex items-center gap-1">
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
                            تسجيل في هذه الدفعة
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
