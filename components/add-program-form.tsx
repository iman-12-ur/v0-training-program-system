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
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import { Plus, X, Image as ImageIcon, PencilLine, FileSpreadsheet, Upload, Download, CheckCircle2, AlertTriangle } from 'lucide-react';
import { categories, programTypes } from '@/lib/mock-data';
import * as XLSX from 'xlsx';

interface AddProgramFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (program: any) => void;
}

export function AddProgramForm({ isOpen, onClose, onSubmit }: AddProgramFormProps) {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    selectedCategories: [] as string[],
    programType: '',
    targetAudience: '',
    duration: '',
    instructor: '',
    location: '',
    logo: '',
    objectives: [''],
    topics: [''],
    prerequisites: [''],
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [inputMode, setInputMode] = useState<'manual' | 'excel'>('manual');
  const [excelMessage, setExcelMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  // تحميل قالب Excel فارغ بنفس توزيعة النظام
  const handleDownloadTemplate = () => {
    const headers = [
      'عنوان البرنامج',
      'وصف البرنامج',
      'التصنيفات',
      'نوع البرنامج',
      'الفئة المستهدفة',
      'المدة',
      'المدرب',
      'الموقع',
      'الأهداف',
      'المحاور',
      'المتطلبات المسبقة',
    ];
    const example = [
      'القيادة الفعّالة',
      'برنامج تدريبي لتطوير المهارات القيادية',
      'إدارية، قيادية',
      programTypes[0] || 'حضوري',
      'المشرفين والمدراء',
      '5 أيام',
      'أ. محمد العامري',
      'قاعة التدريب الرئيسية',
      'فهم أساسيات القيادة | تطوير مهارات التواصل',
      'أنماط القيادة | إدارة الفرق | حل المشكلات',
      'خبرة سنتين | موافقة المدير المباشر',
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, example]);
    ws['!cols'] = headers.map(() => ({ wch: 25 }));
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'البرامج');
    XLSX.writeFile(wb, 'قالب_البرامج_التدريبية.xlsx');
  };

  // قراءة ملف Excel وتعبئة الخانات تلقائياً
  const splitList = (value: string) =>
    String(value || '')
      .split(/[|\n,؛;]/)
      .map((s) => s.trim())
      .filter(Boolean);

  const handleExcelUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setExcelMessage(null);

    const reader = new FileReader();
    reader.onload = (evt) => {
      try {
        const data = new Uint8Array(evt.target?.result as ArrayBuffer);
        const wb = XLSX.read(data, { type: 'array' });
        const sheet = wb.Sheets[wb.SheetNames[0]];
        const rows = XLSX.utils.sheet_to_json<Record<string, any>>(sheet, { defval: '' });

        if (rows.length === 0) {
          setExcelMessage({ type: 'error', text: 'الملف فارغ. تأكد من تعبئة صف واحد على الأقل.' });
          return;
        }

        const row = rows[0];
        const get = (key: string) => String(row[key] ?? '').trim();

        const importedCategories = splitList(get('التصنيفات'));
        // إضافة أي تصنيف جديد غير موجود إلى القائمة المتاحة
        if (importedCategories.length > 0) {
          setAvailableCategories((prev) => {
            const merged = [...prev];
            importedCategories.forEach((c) => {
              if (!merged.includes(c)) merged.push(c);
            });
            return merged;
          });
        }

        setFormData((prev) => ({
          ...prev,
          title: get('عنوان البرنامج') || prev.title,
          description: get('وصف البرنامج') || prev.description,
          selectedCategories: importedCategories.length ? importedCategories : prev.selectedCategories,
          programType: get('نوع البرنامج') || prev.programType,
          targetAudience: get('الفئة المستهدفة') || prev.targetAudience,
          duration: get('المدة') || prev.duration,
          instructor: get('المدرب') || prev.instructor,
          location: get('الموقع') || prev.location,
          objectives: splitList(get('الأهداف')).length ? splitList(get('الأهداف')) : prev.objectives,
          topics: splitList(get('المحاور')).length ? splitList(get('المحاور')) : prev.topics,
          prerequisites: splitList(get('المتطلبات المسبقة')).length
            ? splitList(get('المتطلبات المسبقة'))
            : prev.prerequisites,
        }));

        setExcelMessage({
          type: 'success',
          text: `تم استيراد بيانات البرنامج بنجاح${rows.length > 1 ? ` (تم استخدام أول صف من أصل ${rows.length})` : ''}. راجع الخانات ثم اضغط حفظ.`,
        });
        setInputMode('manual');
      } catch (err) {
        setExcelMessage({ type: 'error', text: 'تعذّر قراءة الملف. تأكد أنه ملف Excel صالح بنفس القالب.' });
      }
    };
    reader.readAsArrayBuffer(file);
    e.target.value = '';
  };

  const handleCategoryToggle = (category: string) => {
    setFormData((prev) => ({
      ...prev,
      selectedCategories: prev.selectedCategories.includes(category)
        ? prev.selectedCategories.filter((c) => c !== category)
        : [...prev.selectedCategories, category],
    }));
  };

  const handleLogoUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setFormData((prev) => ({ ...prev, logo: reader.result as string }));
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    await new Promise((resolve) => setTimeout(resolve, 1000));
    onSubmit({
      ...formData,
      categories: formData.selectedCategories,
      id: `prog-${Date.now()}`,
      status: 'active',
      createdAt: new Date().toISOString().split('T')[0],
      batches: [],
      objectives: formData.objectives.filter((o) => o.trim()),
      topics: formData.topics.filter((t) => t.trim()),
      prerequisites: formData.prerequisites.filter((p) => p.trim()),
    });
    setIsSubmitting(false);
    setFormData({
      title: '',
      description: '',
      selectedCategories: [],
      programType: '',
      targetAudience: '',
      duration: '',
      instructor: '',
      location: '',
      logo: '',
      objectives: [''],
      topics: [''],
      prerequisites: [''],
    });
    setExcelMessage(null);
    setInputMode('manual');
  };

  

  const isValid =
    formData.title &&
    formData.description &&
    formData.selectedCategories.length > 0 &&
    formData.duration &&
    formData.instructor &&
    formData.location;

  const [availableCategories, setAvailableCategories] = useState(
    categories.filter((c) => c !== 'الكل')
  );
  const [newCategory, setNewCategory] = useState('');
  const [showNewCategoryInput, setShowNewCategoryInput] = useState(false);

  const handleAddNewCategory = () => {
    if (newCategory.trim() && !availableCategories.includes(newCategory.trim())) {
      const categoryToAdd = newCategory.trim();
      setAvailableCategories((prev) => [...prev, categoryToAdd]);
      setFormData((prev) => ({
        ...prev,
        selectedCategories: [...prev.selectedCategories, categoryToAdd],
      }));
      setNewCategory('');
      setShowNewCategoryInput(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] p-0">
        <DialogHeader className="border-b p-6 pb-4">
          <DialogTitle>إضافة برنامج تدريبي جديد</DialogTitle>
          {/* اختيار طريقة الإدخال */}
          <div className="mt-4 grid grid-cols-2 gap-2" style={{ direction: 'rtl' }}>
            <button
              type="button"
              onClick={() => setInputMode('manual')}
              className={`flex items-center justify-center gap-2 rounded-lg border p-3 text-sm font-medium transition-colors ${
                inputMode === 'manual'
                  ? 'border-primary bg-primary/10 text-primary'
                  : 'border-border text-muted-foreground hover:border-primary/50'
              }`}
            >
              <PencilLine className="h-4 w-4" />
              إدخال يدوي
            </button>
            <button
              type="button"
              onClick={() => setInputMode('excel')}
              className={`flex items-center justify-center gap-2 rounded-lg border p-3 text-sm font-medium transition-colors ${
                inputMode === 'excel'
                  ? 'border-primary bg-primary/10 text-primary'
                  : 'border-border text-muted-foreground hover:border-primary/50'
              }`}
            >
              <FileSpreadsheet className="h-4 w-4" />
              رفع ملف Excel
            </button>
          </div>
          {/* رسالة نتيجة الاستيراد */}
          {excelMessage && (
            <div
              className={`mt-3 flex items-start gap-2 rounded-lg border p-3 text-sm ${
                excelMessage.type === 'success'
                  ? 'border-emerald-200 bg-emerald-50 text-emerald-700'
                  : 'border-destructive/20 bg-destructive/10 text-destructive'
              }`}
              style={{ direction: 'rtl' }}
            >
              {excelMessage.type === 'success' ? (
                <CheckCircle2 className="h-4 w-4 shrink-0 mt-0.5" />
              ) : (
                <AlertTriangle className="h-4 w-4 shrink-0 mt-0.5" />
              )}
              <span>{excelMessage.text}</span>
            </div>
          )}
        </DialogHeader>

        {inputMode === 'excel' && (
          <div className="p-6" style={{ direction: 'rtl' }}>
            <div className="rounded-lg border-2 border-dashed border-muted-foreground/25 bg-muted/30 p-8 text-center">
              <FileSpreadsheet className="mx-auto h-12 w-12 text-primary" />
              <h4 className="mt-3 font-semibold text-foreground">رفع ملف Excel للبرنامج التدريبي</h4>
              <p className="mx-auto mt-1 max-w-md text-sm text-muted-foreground">
                حمّل القالب أولاً، عبّئ بيانات البرنامج بنفس توزيعة النظام، ثم ارفعه هنا وسيتم تعبئة الخانات تلقائياً لمراجعتها قبل الحفظ.
              </p>
              <div className="mt-5 flex flex-col items-center justify-center gap-3 sm:flex-row">
                <Button type="button" variant="outline" onClick={handleDownloadTemplate} className="gap-2">
                  <Download className="h-4 w-4" />
                  تحميل القالب
                </Button>
                <label className="inline-flex cursor-pointer items-center justify-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90">
                  <Upload className="h-4 w-4" />
                  اختيار ملف Excel
                  <input
                    type="file"
                    accept=".xlsx,.xls"
                    className="hidden"
                    onChange={handleExcelUpload}
                  />
                </label>
              </div>
            </div>
            <div className="mt-4 rounded-lg bg-muted/50 p-4 text-right text-xs text-muted-foreground">
              <p className="font-medium text-foreground mb-1">أعمدة القالب المطلوبة:</p>
              <p>عنوان البرنامج، وصف البرنامج، التصنيفات، نوع البرنامج، الفئة المستهدفة، المدة، المدرب، الموقع، الأهداف، المحاور، المتطلبات المسبقة.</p>
              <p className="mt-1">يمكن الفصل بين عناصر التصنيفات/الأهداف/المحاور/المتطلبات بعلامة | أو فاصلة.</p>
            </div>
          </div>
        )}

        <div className={inputMode === 'manual' ? '' : 'hidden'}>
        <ScrollArea className="max-h-[60vh]">
          <div className="space-y-6 p-6">
            {/* Basic Info */}
            <div className="grid gap-4 sm:grid-cols-2" style={{ direction: 'rtl' }}>
              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="title">عنوان البرنامج *</Label>
                <Input
                  id="title"
                  placeholder="مثال: القيادة الفعّالة"
                  value={formData.title}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, title: e.target.value }))
                  }
                />
              </div>

              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="description">وصف البرنامج *</Label>
                <Textarea
                  id="description"
                  placeholder="اكتب وصفاً مختصراً للبرنامج..."
                  value={formData.description}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      description: e.target.value,
                    }))
                  }
                  rows={3}
                />
              </div>

              {/* Categories Selection */}
              <div className="space-y-3 sm:col-span-2">
                <div className="flex items-center justify-between">
                  <Label>التصنيفات * (يمكن اختيار أكثر من تصنيف)</Label>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setShowNewCategoryInput(true)}
                    className="gap-1"
                  >
                    <Plus className="h-4 w-4" />
                    إضافة تصنيف جديد
                  </Button>
                </div>
                
                {showNewCategoryInput && (
                  <div className="flex items-center gap-2 p-3 border rounded-lg bg-muted/50">
                    <Input
                      placeholder="اسم التصنيف الجديد"
                      value={newCategory}
                      onChange={(e) => setNewCategory(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                          e.preventDefault();
                          handleAddNewCategory();
                        }
                      }}
                      className="flex-1"
                    />
                    <Button
                      type="button"
                      size="sm"
                      onClick={handleAddNewCategory}
                      disabled={!newCategory.trim()}
                    >
                      إضافة
                    </Button>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      onClick={() => {
                        setShowNewCategoryInput(false);
                        setNewCategory('');
                      }}
                    >
                      <X className="h-4 w-4" />
                    </Button>
                  </div>
                )}
                
                <div className="flex flex-wrap gap-2 flex-row-reverse justify-end">
                  {availableCategories.map((cat) => (
                    <div
                      key={cat}
                      className={`flex flex-row-reverse items-center gap-2 rounded-lg border px-3 py-2 cursor-pointer transition-colors ${
                        formData.selectedCategories.includes(cat)
                          ? 'border-primary bg-primary/10 text-primary'
                          : 'border-border hover:border-primary/50'
                      }`}
                      onClick={() => handleCategoryToggle(cat)}
                    >
                      <span className="text-sm">{cat}</span>
                      <Checkbox
                        checked={formData.selectedCategories.includes(cat)}
                        onCheckedChange={() => handleCategoryToggle(cat)}
                      />
                    </div>
                  ))}
                </div>
                {formData.selectedCategories.length > 0 && (
                  <div className="flex flex-wrap flex-row-reverse justify-end gap-1 mt-2">
                    {formData.selectedCategories.map((cat) => (
                      <Badge key={cat} variant="secondary" className="gap-1 flex-row-reverse pr-1">
                        <button
                          type="button"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleCategoryToggle(cat);
                          }}
                          className="rounded-full p-0.5 hover:bg-destructive hover:text-destructive-foreground transition-colors"
                        >
                          <X className="h-3 w-3" />
                        </button>
                        {cat}
                      </Badge>
                    ))}
                  </div>
                )}
              </div>

              {/* Target Audience */}
              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="targetAudience">الفئة المستهدفة</Label>
                <Textarea
                  id="targetAudience"
                  placeholder="مثال: المشرفين والمدراء، رؤساء الأقسام، الموظفين الجدد..."
                  value={formData.targetAudience}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, targetAudience: e.target.value }))
                  }
                  rows={2}
                />
              </div>

              {/* Row 1: Program Type and Instructor */}
              <div className="space-y-2">
                <Label htmlFor="instructor">المدرب *</Label>
                <Input
                  id="instructor"
                  placeholder="اسم المدرب"
                  value={formData.instructor}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      instructor: e.target.value,
                    }))
                  }
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="programType">نوع البرنامج</Label>
                <Select
                  value={formData.programType}
                  onValueChange={(value) =>
                    setFormData((prev) => ({ ...prev, programType: value }))
                  }
                >
                  <SelectTrigger id="programType">
                    <SelectValue placeholder="اختر نوع البرنامج" />
                  </SelectTrigger>
                  <SelectContent>
                    {programTypes.map((type) => (
                      <SelectItem key={type} value={type}>
                        {type}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Row 2: Duration and Location */}
              <div className="space-y-2">
                <Label htmlFor="duration">المدة *</Label>
                <Input
                  id="duration"
                  placeholder="مثال: 5 أيام"
                  value={formData.duration}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, duration: e.target.value }))
                  }
                />
              </div>

              {/* Location */}
              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="location">الموقع *</Label>
                <Input
                  id="location"
                  placeholder="مثال: قاعة التدريب الرئيسية أو رابط الاجتماع"
                  value={formData.location}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, location: e.target.value }))
                  }
                />
              </div>

              {/* Logo Upload */}
              <div className="space-y-2 sm:col-span-2">
                <Label>شعار البرنامج (اختياري)</Label>
                <p className="text-xs text-muted-foreground mb-2">
                  يمكنك إضافة شعار للبرنامج مثل شعارات الجودة أو ISO
                </p>
                <div className="flex items-center gap-4">
                  {formData.logo ? (
                    <div className="relative">
                      <img
                        src={formData.logo}
                        alt="شعار البرنامج"
                        className="h-20 w-20 rounded-lg object-contain border bg-white"
                      />
                      <Button
                        type="button"
                        variant="destructive"
                        size="icon"
                        className="absolute -top-2 -left-2 h-6 w-6"
                        onClick={() => setFormData((prev) => ({ ...prev, logo: '' }))}
                      >
                        <X className="h-3 w-3" />
                      </Button>
                    </div>
                  ) : (
                    <label className="flex h-20 w-20 cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed border-muted-foreground/25 bg-muted/50 transition-colors hover:border-primary hover:bg-muted">
                      <ImageIcon className="h-6 w-6 text-muted-foreground" />
                      <span className="mt-1 text-xs text-muted-foreground">رفع شعار</span>
                      <input
                        type="file"
                        accept="image/*"
                        className="hidden"
                        onChange={handleLogoUpload}
                      />
                    </label>
                  )}
                </div>
              </div>
            </div>

{/* Objectives */}
            <div className="space-y-3" style={{ direction: 'rtl' }}>
              <Label htmlFor="objectives">أهداف البرنامج</Label>
              <Textarea
                id="objectives"
                placeholder="اكتب أهداف البرنامج (كل هدف في سطر جديد)
مثال:
- فهم أساسيات القيادة الفعالة
- تطوير مهارات التواصل
- بناء فرق عمل متماسكة"
                value={formData.objectives.join('\n')}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    objectives: e.target.value.split('\n'),
                  }))
                }
                rows={4}
                className="text-right"
                style={{ direction: 'rtl' }}
              />
            </div>

            {/* Topics */}
            <div className="space-y-3" style={{ direction: 'rtl' }}>
              <Label htmlFor="topics">محاور البرنامج</Label>
              <Textarea
                id="topics"
                placeholder="اكتب محاور البرنامج (كل محور في سطر جديد)
مثال:
- مفهوم القيادة وأنماطها
- مهارات التأثير والإقناع
- إدارة فرق العمل
- حل المشكلات واتخاذ القرارات"
                value={formData.topics.join('\n')}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    topics: e.target.value.split('\n'),
                  }))
                }
                rows={4}
                className="text-right"
                style={{ direction: 'rtl' }}
              />
            </div>

            {/* Prerequisites */}
            <div className="space-y-3" style={{ direction: 'rtl' }}>
              <Label htmlFor="prerequisites">المتطلبات المسبقة</Label>
              <Textarea
                id="prerequisites"
                placeholder="اكتب المتطلبات المسبقة (كل متطلب في سطر جديد)
مثال:
- خبرة لا تقل عن سنتين
- موافقة المدير المباشر
- إتمام الدورة التأسيسية"
                value={formData.prerequisites.join('\n')}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    prerequisites: e.target.value.split('\n'),
                  }))
                }
                rows={4}
                className="text-right"
                style={{ direction: 'rtl' }}
              />
            </div>
          </div>
        </ScrollArea>
        </div>

        <DialogFooter className="border-t p-6 pt-4 flex-row-reverse gap-2">
          <Button onClick={handleSubmit} disabled={!isValid || isSubmitting}>
            {isSubmitting ? 'جاري الحفظ...' : 'حفظ البرنامج'}
          </Button>
          <Button variant="outline" onClick={onClose}>
            إلغاء
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
