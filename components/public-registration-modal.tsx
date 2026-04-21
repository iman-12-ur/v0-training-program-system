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
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { Calendar, Users, CheckCircle2, User, Building2, Mail, Phone, BadgeCheck } from 'lucide-react';
import { departments } from '@/lib/mock-data';
import type { TrainingProgram, Batch, Registration } from '@/lib/types';

interface PublicRegistrationModalProps {
  program: TrainingProgram | null;
  selectedBatch: Batch | null;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (registration: Omit<Registration, 'id' | 'status' | 'registeredAt' | 'approvedBy' | 'approvedAt'>) => void;
}

export function PublicRegistrationModal({
  program,
  selectedBatch,
  isOpen,
  onClose,
  onConfirm,
}: PublicRegistrationModalProps) {
  const [batchId, setBatchId] = useState(selectedBatch?.id || '');
  const [formData, setFormData] = useState({
    visitorName: '',
    employeeId: '',
    department: '',
    email: '',
    phone: '',
    notes: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  if (!program) return null;

  const availableBatches = program.batches.filter(
    (b) => b.status === 'upcoming' && b.currentParticipants < b.maxParticipants
  );

  const validateForm = () => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.visitorName.trim()) {
      newErrors.visitorName = 'الاسم مطلوب';
    }
    if (!formData.employeeId.trim()) {
      newErrors.employeeId = 'الرقم الوظيفي مطلوب';
    }
    if (!formData.department) {
      newErrors.department = 'القسم مطلوب';
    }
    if (!formData.email.trim()) {
      newErrors.email = 'البريد الإلكتروني مطلوب';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'البريد الإلكتروني غير صحيح';
    }
    if (!batchId) {
      newErrors.batchId = 'يرجى اختيار الدفعة';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validateForm()) return;

    setIsSubmitting(true);
    await new Promise((resolve) => setTimeout(resolve, 1000));
    
    const batch = program.batches.find((b) => b.id === batchId);
    
    onConfirm({
      programId: program.id,
      programTitle: program.title,
      batchId,
      batchName: batch?.name || '',
      visitorName: formData.visitorName,
      employeeId: formData.employeeId,
      department: formData.department,
      email: formData.email,
      phone: formData.phone,
      notes: formData.notes,
    });

    setIsSubmitting(false);
    setIsSuccess(true);
  };

  const handleClose = () => {
    setBatchId(selectedBatch?.id || '');
    setFormData({
      visitorName: '',
      employeeId: '',
      department: '',
      email: '',
      phone: '',
      notes: '',
    });
    setErrors({});
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
              تم تقديم طلب الترشيح بنجاح
            </h3>
            <p className="text-muted-foreground">
              سيتم مراجعة طلبك من قبل إدارة التدريب وإبلاغك عبر البريد الإلكتروني بنتيجة الترشيح
            </p>
            <div className="mt-4 rounded-lg bg-muted/50 p-4 text-right w-full">
              <p className="text-sm text-muted-foreground">رقم الطلب</p>
              <p className="font-mono font-semibold text-foreground">REG-{Date.now().toString().slice(-6)}</p>
            </div>
          </div>
          <DialogFooter>
            <Button onClick={handleClose} className="w-full">
              إغلاق
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>طلب ترشيح للبرنامج التدريبي</DialogTitle>
        </DialogHeader>

        <div className="space-y-5 py-4">
          {/* Program Info */}
          <div className="rounded-lg bg-primary/5 border border-primary/20 p-4">
            <h4 className="font-semibold text-foreground">{program.title}</h4>
            <p className="mt-1 text-sm text-muted-foreground">
              {program.instructor} - {program.duration}
            </p>
          </div>

          {/* Employee Information */}
          <div className="space-y-4">
            <h5 className="font-medium text-foreground flex items-center gap-2">
              <User className="h-4 w-4 text-primary" />
              بيانات الموظف
            </h5>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="visitorName">الاسم الكامل *</Label>
                <Input
                  id="visitorName"
                  placeholder="أدخل اسمك الكامل"
                  value={formData.visitorName}
                  onChange={(e) => setFormData({ ...formData, visitorName: e.target.value })}
                  className={errors.visitorName ? 'border-destructive' : ''}
                />
                {errors.visitorName && (
                  <p className="text-xs text-destructive">{errors.visitorName}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="employeeId">الرقم الوظيفي *</Label>
                <div className="relative">
                  <BadgeCheck className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="employeeId"
                    placeholder="مثال: EMP-001"
                    value={formData.employeeId}
                    onChange={(e) => setFormData({ ...formData, employeeId: e.target.value })}
                    className={`pr-9 ${errors.employeeId ? 'border-destructive' : ''}`}
                  />
                </div>
                {errors.employeeId && (
                  <p className="text-xs text-destructive">{errors.employeeId}</p>
                )}
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="department">القسم / الإدارة *</Label>
              <Select
                value={formData.department}
                onValueChange={(value) => setFormData({ ...formData, department: value })}
              >
                <SelectTrigger id="department" className={errors.department ? 'border-destructive' : ''}>
                  <Building2 className="ml-2 h-4 w-4 text-muted-foreground" />
                  <SelectValue placeholder="اختر القسم" />
                </SelectTrigger>
                <SelectContent>
                  {departments.map((dept) => (
                    <SelectItem key={dept} value={dept}>
                      {dept}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.department && (
                <p className="text-xs text-destructive">{errors.department}</p>
              )}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="email">البريد الإلكتروني *</Label>
                <div className="relative">
                  <Mail className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="email"
                    type="email"
                    placeholder="example@company.com"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    className={`pr-9 ${errors.email ? 'border-destructive' : ''}`}
                  />
                </div>
                {errors.email && (
                  <p className="text-xs text-destructive">{errors.email}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="phone">رقم الجوال (اختياري)</Label>
                <div className="relative">
                  <Phone className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="phone"
                    type="tel"
                    placeholder="05xxxxxxxx"
                    value={formData.phone}
                    onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                    className="pr-9"
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Batch Selection */}
          <div className="space-y-4">
            <h5 className="font-medium text-foreground flex items-center gap-2">
              <Calendar className="h-4 w-4 text-primary" />
              اختيار الدفعة
            </h5>

            <div className="space-y-2">
              <Label htmlFor="batch">الدفعة المناسبة *</Label>
              <Select
                value={batchId}
                onValueChange={setBatchId}
                defaultValue={selectedBatch?.id}
              >
                <SelectTrigger id="batch" className={errors.batchId ? 'border-destructive' : ''}>
                  <SelectValue placeholder="اختر الدفعة المناسبة" />
                </SelectTrigger>
                <SelectContent>
                  {availableBatches.length === 0 ? (
                    <SelectItem value="none" disabled>
                      لا توجد دفعات متاحة حالياً
                    </SelectItem>
                  ) : (
                    availableBatches.map((batch) => (
                      <SelectItem key={batch.id} value={batch.id}>
                        <div className="flex items-center gap-2">
                          <span>{batch.name}</span>
                          <span className="text-muted-foreground">
                            ({batch.startDate})
                          </span>
                        </div>
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
              {errors.batchId && (
                <p className="text-xs text-destructive">{errors.batchId}</p>
              )}
            </div>

            {/* Selected Batch Info */}
            {batchId && (
              <div className="space-y-2 rounded-lg border bg-muted/30 p-3">
                {(() => {
                  const batch = program.batches.find((b) => b.id === batchId);
                  if (!batch) return null;
                  return (
                    <>
                      <div className="flex items-center gap-2 text-sm">
                        <Calendar className="h-4 w-4 text-primary" />
                        <span>
                          من {batch.startDate} إلى {batch.endDate}
                        </span>
                      </div>
                      <div className="flex items-center gap-2 text-sm">
                        <Users className="h-4 w-4 text-primary" />
                        <span>
                          المقاعد المتاحة:{' '}
                          <strong className="text-primary">
                            {batch.maxParticipants - batch.currentParticipants}
                          </strong>{' '}
                          من {batch.maxParticipants}
                        </span>
                      </div>
                    </>
                  );
                })()}
              </div>
            )}
          </div>

          {/* Notes */}
          <div className="space-y-2">
            <Label htmlFor="notes">ملاحظات إضافية (اختياري)</Label>
            <Textarea
              id="notes"
              placeholder="أي ملاحظات أو معلومات إضافية تود إضافتها..."
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              className="resize-none"
              rows={3}
            />
          </div>
        </div>

        <DialogFooter className="gap-2">
          <Button variant="outline" onClick={handleClose}>
            إلغاء
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={isSubmitting || availableBatches.length === 0}
          >
            {isSubmitting ? 'جاري تقديم الطلب...' : 'تقديم طلب الترشيح'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
