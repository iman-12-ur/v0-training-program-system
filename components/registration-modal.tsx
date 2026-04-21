'use client';

import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { Calendar, Users, CheckCircle2 } from 'lucide-react';
import type { TrainingProgram, Batch } from '@/lib/types';

interface RegistrationModalProps {
  program: TrainingProgram | null;
  selectedBatch: Batch | null;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (programId: string, batchId: string, notes: string) => void;
}

export function RegistrationModal({
  program,
  selectedBatch,
  isOpen,
  onClose,
  onConfirm,
}: RegistrationModalProps) {
  const [batchId, setBatchId] = useState(selectedBatch?.id || '');
  const [notes, setNotes] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  if (!program) return null;

  const availableBatches = program.batches.filter(
    (b) => b.status === 'upcoming' && b.currentParticipants < b.maxParticipants
  );

  const handleSubmit = async () => {
    if (!batchId) return;
    setIsSubmitting(true);
    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 1000));
    setIsSubmitting(false);
    setIsSuccess(true);
    setTimeout(() => {
      onConfirm(program.id, batchId, notes);
      setIsSuccess(false);
      setBatchId('');
      setNotes('');
    }, 1500);
  };

  const handleClose = () => {
    setBatchId(selectedBatch?.id || '');
    setNotes('');
    setIsSuccess(false);
    onClose();
  };

  if (isSuccess) {
    return (
      <Dialog open={isOpen} onOpenChange={handleClose}>
        <DialogContent className="max-w-md text-center">
          <div className="flex flex-col items-center gap-4 py-8">
            <div className="rounded-full bg-emerald-100 p-4">
              <CheckCircle2 className="h-12 w-12 text-emerald-600" />
            </div>
            <h3 className="text-xl font-bold text-foreground">
              تم التسجيل بنجاح!
            </h3>
            <p className="text-muted-foreground">
              سيتم مراجعة طلبك من قبل الإدارة وإعلامك بالنتيجة
            </p>
          </div>
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>التسجيل في البرنامج</DialogTitle>
        </DialogHeader>

        <div className="space-y-4 py-4">
          {/* Program Info */}
          <div className="rounded-lg bg-muted/50 p-4">
            <h4 className="font-semibold text-foreground">{program.title}</h4>
            <p className="mt-1 text-sm text-muted-foreground">
              {program.instructor} • {program.duration}
            </p>
          </div>

          {/* Batch Selection */}
          <div className="space-y-2">
            <Label htmlFor="batch">اختر الدفعة</Label>
            <Select
              value={batchId}
              onValueChange={setBatchId}
              defaultValue={selectedBatch?.id}
            >
              <SelectTrigger id="batch">
                <SelectValue placeholder="اختر الدفعة المناسبة" />
              </SelectTrigger>
              <SelectContent>
                {availableBatches.map((batch) => (
                  <SelectItem key={batch.id} value={batch.id}>
                    <div className="flex items-center gap-2">
                      <span>{batch.name}</span>
                      <span className="text-muted-foreground">
                        ({batch.startDate})
                      </span>
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Selected Batch Info */}
          {batchId && (
            <div className="space-y-2 rounded-lg border p-3">
              {(() => {
                const batch = program.batches.find((b) => b.id === batchId);
                if (!batch) return null;
                return (
                  <>
                    <div className="flex items-center gap-2 text-sm">
                      <Calendar className="h-4 w-4 text-primary" />
                      <span>
                        {batch.startDate} إلى {batch.endDate}
                      </span>
                    </div>
                    <div className="flex items-center gap-2 text-sm">
                      <Users className="h-4 w-4 text-primary" />
                      <span>
                        المقاعد المتاحة:{' '}
                        {batch.maxParticipants - batch.currentParticipants} من{' '}
                        {batch.maxParticipants}
                      </span>
                    </div>
                  </>
                );
              })()}
            </div>
          )}

          {/* Notes */}
          <div className="space-y-2">
            <Label htmlFor="notes">ملاحظات (اختياري)</Label>
            <Textarea
              id="notes"
              placeholder="أضف أي ملاحظات أو استفسارات..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              className="resize-none"
              rows={3}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose}>
            إلغاء
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={!batchId || isSubmitting}
          >
            {isSubmitting ? 'جاري التسجيل...' : 'تأكيد التسجيل'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
