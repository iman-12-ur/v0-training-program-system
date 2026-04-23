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
import { Plus, X, Upload, Image as ImageIcon } from 'lucide-react';
import { categories, programTypes } from '@/lib/mock-data';

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
    prerequisites: [''],
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

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
      status: 'draft',
      createdAt: new Date().toISOString().split('T')[0],
      batches: [],
      objectives: formData.objectives.filter((o) => o.trim()),
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
      prerequisites: [''],
    });
  };

  const addObjective = () => {
    setFormData((prev) => ({
      ...prev,
      objectives: [...prev.objectives, ''],
    }));
  };

  const removeObjective = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      objectives: prev.objectives.filter((_, i) => i !== index),
    }));
  };

  const updateObjective = (index: number, value: string) => {
    setFormData((prev) => ({
      ...prev,
      objectives: prev.objectives.map((o, i) => (i === index ? value : o)),
    }));
  };

  const addPrerequisite = () => {
    setFormData((prev) => ({
      ...prev,
      prerequisites: [...prev.prerequisites, ''],
    }));
  };

  const removePrerequisite = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      prerequisites: prev.prerequisites.filter((_, i) => i !== index),
    }));
  };

  const updatePrerequisite = (index: number, value: string) => {
    setFormData((prev) => ({
      ...prev,
      prerequisites: prev.prerequisites.map((p, i) => (i === index ? value : p)),
    }));
  };

  const isValid =
    formData.title &&
    formData.description &&
    formData.selectedCategories.length > 0 &&
    formData.duration &&
    formData.instructor &&
    formData.location;

  const availableCategories = categories.filter((c) => c !== 'الكل');

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] p-0">
        <DialogHeader className="border-b p-6 pb-4">
          <DialogTitle>إضافة برنامج تدريبي جديد</DialogTitle>
        </DialogHeader>

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
                <Label>التصنيفات * (يمكن اختيار أكثر من تصنيف)</Label>
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
                      <Badge key={cat} variant="secondary" className="gap-1 flex-row-reverse">
                        <X
                          className="h-3 w-3 cursor-pointer"
                          onClick={() => handleCategoryToggle(cat)}
                        />
                        {cat}
                      </Badge>
                    ))}
                  </div>
                )}
              </div>

              {/* Target Audience */}
              <div className="space-y-2">
                <Label htmlFor="targetAudience">الفئة المستهدفة</Label>
                <Input
                  id="targetAudience"
                  placeholder="مثال: المشرفين والمدراء"
                  value={formData.targetAudience}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, targetAudience: e.target.value }))
                  }
                />
              </div>

              {/* Program Type */}
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

              <div className="space-y-2 sm:col-span-2">
                <Label htmlFor="location">الموقع *</Label>
                <Input
                  id="location"
                  placeholder="مثال: قاعة التدريب الرئيسية"
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
            <div className="space-y-3">
              <div className="flex flex-row-reverse items-center justify-between">
                <Label>أهداف البرنامج</Label>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addObjective}
                >
                  إضافة هدف
                  <Plus className="mr-1 h-4 w-4" />
                </Button>
              </div>
              <div className="space-y-2">
                {formData.objectives.map((obj, index) => (
                  <div key={index} className="flex items-center gap-2">
                    <Input
                      placeholder={`الهدف ${index + 1}`}
                      value={obj}
                      onChange={(e) => updateObjective(index, e.target.value)}
                    />
                    {formData.objectives.length > 1 && (
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        onClick={() => removeObjective(index)}
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Prerequisites */}
            <div className="space-y-3">
              <div className="flex flex-row-reverse items-center justify-between">
                <Label>المتطلبات المسبقة</Label>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addPrerequisite}
                >
                  إضافة متطلب
                  <Plus className="mr-1 h-4 w-4" />
                </Button>
              </div>
              <div className="space-y-2">
                {formData.prerequisites.map((prereq, index) => (
                  <div key={index} className="flex items-center gap-2">
                    <Input
                      placeholder={`المتطلب ${index + 1}`}
                      value={prereq}
                      onChange={(e) => updatePrerequisite(index, e.target.value)}
                    />
                    {formData.prerequisites.length > 1 && (
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        onClick={() => removePrerequisite(index)}
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </div>
        </ScrollArea>

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
