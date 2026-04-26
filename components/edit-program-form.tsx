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
import { X, Image as ImageIcon, Plus } from 'lucide-react';
import { categories, programTypes } from '@/lib/mock-data';
import type { TrainingProgram } from '@/lib/types';

interface EditProgramFormProps {
  program: TrainingProgram | null;
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (program: TrainingProgram) => void;
}

export function EditProgramForm({
  program,
  isOpen,
  onClose,
  onSubmit,
}: EditProgramFormProps) {
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
    objectives: [''] as string[],
    topics: [''] as string[],
    prerequisites: [''] as string[],
    status: 'active' as TrainingProgram['status'],
  });

  const [availableCategories, setAvailableCategories] = useState(
    categories.filter((c) => c !== 'الكل')
  );
  const [newCategory, setNewCategory] = useState('');
  const [showNewCategoryInput, setShowNewCategoryInput] = useState(false);

  useEffect(() => {
    if (program) {
      setFormData({
        title: program.title,
        description: program.description,
        selectedCategories: program.categories || [],
        programType: program.programType || '',
        targetAudience: program.targetAudience || '',
        duration: program.duration,
        instructor: program.instructor,
        location: program.location,
        logo: program.logo || '',
        objectives: program.objectives?.length ? program.objectives : [''],
        topics: program.topics?.length ? program.topics : [''],
        prerequisites: program.prerequisites?.length ? program.prerequisites : [''],
        status: program.status,
      });
    }
  }, [program]);

  const handleCategoryToggle = (category: string) => {
    setFormData((prev) => ({
      ...prev,
      selectedCategories: prev.selectedCategories.includes(category)
        ? prev.selectedCategories.filter((c) => c !== category)
        : [...prev.selectedCategories, category],
    }));
  };

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

  const handleSubmit = () => {
    if (!program) return;
    
    const updatedProgram: TrainingProgram = {
      ...program,
      title: formData.title,
      description: formData.description,
      categories: formData.selectedCategories,
      programType: formData.programType,
      targetAudience: formData.targetAudience,
      duration: formData.duration,
      instructor: formData.instructor,
      location: formData.location,
      logo: formData.logo,
      objectives: formData.objectives.filter((o) => o.trim()),
      topics: formData.topics.filter((t) => t.trim()),
      prerequisites: formData.prerequisites.filter((p) => p.trim()),
      status: formData.status,
    };

    onSubmit(updatedProgram);
    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] p-0">
        <DialogHeader className="border-b p-6 pb-4">
          <DialogTitle>تعديل البرنامج التدريبي</DialogTitle>
        </DialogHeader>

        <ScrollArea className="max-h-[60vh]">
          <div className="space-y-6 p-6">
            {/* Basic Info */}
            <div className="grid gap-4 sm:grid-cols-2" style={{ direction: 'rtl' }}>
              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="edit-title">عنوان البرنامج *</Label>
                <Input
                  id="edit-title"
                  placeholder="مثال: القيادة الفعّالة"
                  value={formData.title}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, title: e.target.value }))
                  }
                />
              </div>

              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="edit-description">وصف البرنامج *</Label>
                <Textarea
                  id="edit-description"
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
                <Label htmlFor="edit-targetAudience">الفئة المستهدفة</Label>
                <Textarea
                  id="edit-targetAudience"
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
                <Label htmlFor="edit-instructor">المدرب *</Label>
                <Input
                  id="edit-instructor"
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
                <Label htmlFor="edit-programType">نوع البرنامج</Label>
                <Select
                  value={formData.programType}
                  onValueChange={(value) =>
                    setFormData((prev) => ({ ...prev, programType: value }))
                  }
                >
                  <SelectTrigger id="edit-programType">
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

              {/* Row 2: Duration and Status */}
              <div className="space-y-2">
                <Label htmlFor="edit-status">حالة البرنامج *</Label>
                <Select
                  value={formData.status}
                  onValueChange={(value: TrainingProgram['status']) =>
                    setFormData((prev) => ({ ...prev, status: value }))
                  }
                >
                  <SelectTrigger id="edit-status">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="active">نشط - متاح للتسجيل</SelectItem>
                    <SelectItem value="inactive">غير نشط - غير متاح</SelectItem>
                    <SelectItem value="draft">مسودة</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="edit-duration">المدة *</Label>
                <Input
                  id="edit-duration"
                  placeholder="مثال: 5 أيام"
                  value={formData.duration}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, duration: e.target.value }))
                  }
                />
              </div>

              {/* Location */}
              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="edit-location">الموقع *</Label>
                <Input
                  id="edit-location"
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
              <Label htmlFor="edit-objectives">أهداف البرنامج</Label>
              <Textarea
                id="edit-objectives"
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
              <Label htmlFor="edit-topics">المحاور</Label>
              <Textarea
                id="edit-topics"
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
              <Label htmlFor="edit-prerequisites">المتطلبات المسبقة</Label>
              <Textarea
                id="edit-prerequisites"
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

        <DialogFooter className="border-t p-6 pt-4 flex-row-reverse gap-2">
          <Button
            onClick={handleSubmit}
            disabled={!formData.title || formData.selectedCategories.length === 0}
          >
            حفظ التغييرات
          </Button>
          <Button variant="outline" onClick={onClose}>
            إلغاء
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
