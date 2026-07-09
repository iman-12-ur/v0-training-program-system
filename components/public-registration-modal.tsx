'use client';

import { useState, useEffect } from 'react';
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
import { Calendar, Users, CheckCircle2, User, Building2, Mail, Phone, BadgeCheck, AlertTriangle, Briefcase } from 'lucide-react';

import type { TrainingProgram, Batch, Registration } from '@/lib/types';

interface PublicRegistrationModalProps {
  program: TrainingProgram | null;
  selectedBatch: Batch | null;
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (registration: Omit<Registration, 'id' | 'status' | 'registeredAt' | 'approvedBy' | 'approvedAt'>) => void;
  existingRegistrations: Registration[];
}

export function PublicRegistrationModal({
  program,
  selectedBatch,
  isOpen,
  onClose,
  onConfirm,
  existingRegistrations,
}: PublicRegistrationModalProps) {
  const [batchId, setBatchId] = useState(selectedBatch?.id || '');
  const [formData, setFormData] = useState({
    visitorName: '',
    employeeId: '',
    jobTitle: '',
    court: '',
    department: '',
    email: '',
    phone: '',
    notes: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [duplicateError, setDuplicateError] = useState<string | null>(null);

  // Reset batch when selectedBatch changes
  useEffect(() => {
    if (selectedBatch) {
      setBatchId(selectedBatch.id);
    }
  }, [selectedBatch]);

  if (!program) return null;

  const availableBatches = program.batches.filter(
    (b) => b.status === 'upcoming' && b.currentParticipants < b.maxParticipants
  );

  // Check for duplicate registration
  const checkDuplicateRegistration = (employeeId: string): string | null => {
    if (!employeeId.trim()) return null;

    // Check if employee is already registered in any batch of this program
    const existingReg = existingRegistrations.find(
      (reg) =>
        reg.programId === program.id &&
        reg.employeeId.toLowerCase() === employeeId.toLowerCase() &&
        (reg.status === 'pending' || reg.status === 'approved')
    );

    if (existingReg) {
      return `أنت مسجل مسبقاً في هذا البرنامج (${existingReg.batchName}). لا يمكن التسجيل مرتين في نفس البرنامج.`;
    }

    return null;
  };

  // Convert Arabic numbers to English numbers
  const convertArabicToEnglish = (str: string): string => {
    const arabicNumerals = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
    let result = str;
    arabicNumerals.forEach((arabic, index) => {
      result = result.replace(new RegExp(arabic, 'g'), index.toString());
    });
    return result;
  };

  // Check if string contains only English numbers (and allowed characters)
  const hasOnlyEnglishNumbers = (str: string): boolean => {
    const arabicNumerals = /[٠-٩]/;
    return !arabicNumerals.test(str);
  };

  const validateForm = () => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.visitorName.trim()) {
      newErrors.visitorName = 'الاسم مطلوب';
    }
    if (!formData.employeeId.trim()) {
      newErrors.employeeId = 'الرقم الوظيفي مطلوب';
    } else if (!hasOnlyEnglishNumbers(formData.employeeId)) {
      newErrors.employeeId = 'الرقم الوظيفي يجب أن يحتوي على أرقام إنجليزية فقط';
    }
    if (!formData.jobTitle.trim()) {
      newErrors.jobTitle = 'المسمى الوظيفي مطلوب';
    }
    if (!formData.court.trim()) {
      newErrors.court = 'الدائرة/المحكمة مطلوبة';
    }
    if (!formData.department.trim()) {
      newErrors.department = 'القسم مطلوب';
    }
    if (!formData.email.trim()) {
      newErrors.email = 'البريد الإلكتروني مطلوب';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'البريد الإلكتروني غير صحيح';
    }
    if (!formData.phone.trim()) {
      newErrors.phone = 'رقم الهاتف مطلوب';
    } else if (!hasOnlyEnglishNumbers(formData.phone)) {
      newErrors.phone = 'رقم الهاتف يجب أن يحتوي على أرقام إنجليزية فقط';
    } else if (!/^(968)?[279]\d{7}$/.test(formData.phone)) {
      newErrors.phone = 'رقم هاتف عُماني غير صحيح (يبدأ بـ 2 أو 7 أو 9 و8 أرقام)';
    }
    if (!batchId) {
      newErrors.batchId = 'يرجى اختيار الدفعة';
    }

    // Check for duplicate registration
    const duplicate = checkDuplicateRegistration(formData.employeeId);
    if (duplicate) {
      setDuplicateError(duplicate);
      return false;
    }

    setErrors(newErrors);
    setDuplicateError(null);
    return Object.keys(newErrors).length === 0;
  };

  const handleEmployeeIdChange = (value: string) => {
    // Convert Arabic numbers to English
    const convertedValue = convertArabicToEnglish(value);
    setFormData({ ...formData, employeeId: convertedValue });
    // Check for duplicate on change
    const duplicate = checkDuplicateRegistration(convertedValue);
    setDuplicateError(duplicate);
  };

  const handlePhoneChange = (value: string) => {
    // Convert Arabic numbers to English
    const convertedValue = convertArabicToEnglish(value);
    setFormData({ ...formData, phone: convertedValue });
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
      jobTitle: formData.jobTitle,
      court: formData.court,
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
      jobTitle: '',
      court: '',
      department: '',
      email: '',
      phone: '',
      notes: '',
    });
    setErrors({});
    setDuplicateError(null);
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
            {program.targetAudience && (
              <p className="mt-1 text-xs text-primary">
                الفئة المستهدفة: {program.targetAudience}
              </p>
            )}
          </div>

          {/* Duplicate Error Alert */}
          {duplicateError && (
            <div className="flex items-start gap-3 rounded-lg bg-destructive/10 border border-destructive/20 p-4">
              <AlertTriangle className="h-5 w-5 text-destructive shrink-0 mt-0.5" />
              <p className="text-sm text-destructive">{duplicateError}</p>
            </div>
          )}

          {/* Batch Selection - Moved to top */}
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
                    onChange={(e) => handleEmployeeIdChange(e.target.value)}
                    className={`pr-9 ${errors.employeeId || duplicateError ? 'border-destructive' : ''}`}
                  />
                </div>
                {errors.employeeId && (
                  <p className="text-xs text-destructive">{errors.employeeId}</p>
                )}
              </div>
            </div>

            {/* Job Title Input */}
            <div className="space-y-2">
              <Label htmlFor="jobTitle">المسمى الوظيفي *</Label>
              <div className="relative">
                <Briefcase className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  id="jobTitle"
                  placeholder="مثال: أخصائي موارد بشرية"
                  value={formData.jobTitle}
                  onChange={(e) => setFormData({ ...formData, jobTitle: e.target.value })}
                  className={`pr-9 ${errors.jobTitle ? 'border-destructive' : ''}`}
                />
              </div>
              {errors.jobTitle && (
                <p className="text-xs text-destructive">{errors.jobTitle}</p>
              )}
            </div>

            {/* Court Input */}
            <div className="space-y-2">
              <Label htmlFor="court">الدائرة / المحكمة *</Label>
              <div className="relative">
                <Building2 className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  id="court"
                  placeholder="أدخل اسم الدائرة أو المحكمة"
                  value={formData.court}
                  onChange={(e) => setFormData({ ...formData, court: e.target.value })}
                  className={`pr-9 ${errors.court ? 'border-destructive' : ''}`}
                />
              </div>
              {errors.court && (
                <p className="text-xs text-destructive">{errors.court}</p>
              )}
            </div>

            {/* Department Input */}
            <div className="space-y-2">
              <Label htmlFor="department">القسم *</Label>
              <Input
                id="department"
                placeholder="أدخل اسم القسم"
                value={formData.department}
                onChange={(e) => setFormData({ ...formData, department: e.target.value })}
                className={errors.department ? 'border-destructive' : ''}
              />
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
                <Label htmlFor="phone">رقم الهاتف *</Label>
                <div className="relative">
                  <Phone className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    id="phone"
                    type="tel"
                    inputMode="numeric"
                    maxLength={8}
                    placeholder="9xxxxxxx"
                    value={formData.phone}
                    onChange={(e) => handlePhoneChange(e.target.value)}
                    className={`pr-9 ${errors.phone ? 'border-destructive' : ''}`}
                  />
                </div>
                {errors.phone ? (
                  <p className="text-xs text-destructive">{errors.phone}</p>
                ) : (
                  <p className="text-xs text-muted-foreground">رقم عُماني يبدأ بـ 7 أو 9 (8 أرقام)</p>
                )}
              </div>
            </div>
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
            disabled={isSubmitting || availableBatches.length === 0 || !!duplicateError}
          >
            {isSubmitting ? 'جاري تقديم الطلب...' : 'تقديم طلب الترشيح'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
