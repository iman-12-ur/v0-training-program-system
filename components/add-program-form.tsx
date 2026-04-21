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
import { Plus, X } from 'lucide-react';
import { categories } from '@/lib/mock-data';

interface AddProgramFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (program: any) => void;
}

export function AddProgramForm({ isOpen, onClose, onSubmit }: AddProgramFormProps) {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    category: '',
    duration: '',
    instructor: '',
    location: '',
    objectives: [''],
    prerequisites: [''],
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async () => {
    setIsSubmitting(true);
    await new Promise((resolve) => setTimeout(resolve, 1000));
    onSubmit({
      ...formData,
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
      category: '',
      duration: '',
      instructor: '',
      location: '',
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
    formData.category &&
    formData.duration &&
    formData.instructor &&
    formData.location;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] p-0">
        <DialogHeader className="border-b p-6 pb-4">
          <DialogTitle>إضافة برنامج تدريبي جديد</DialogTitle>
        </DialogHeader>

        <ScrollArea className="max-h-[60vh]">
          <div className="space-y-6 p-6">
            {/* Basic Info */}
            <div className="grid gap-4 sm:grid-cols-2">
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

              <div className="space-y-2">
                <Label htmlFor="category">التصنيف *</Label>
                <Select
                  value={formData.category}
                  onValueChange={(value) =>
                    setFormData((prev) => ({ ...prev, category: value }))
                  }
                >
                  <SelectTrigger id="category">
                    <SelectValue placeholder="اختر التصنيف" />
                  </SelectTrigger>
                  <SelectContent>
                    {categories
                      .filter((c) => c !== 'الكل')
                      .map((cat) => (
                        <SelectItem key={cat} value={cat}>
                          {cat}
                        </SelectItem>
                      ))}
                  </SelectContent>
                </Select>
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
            </div>

            {/* Objectives */}
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <Label>أهداف البرنامج</Label>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addObjective}
                >
                  <Plus className="ml-1 h-4 w-4" />
                  إضافة هدف
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
              <div className="flex items-center justify-between">
                <Label>المتطلبات المسبقة</Label>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addPrerequisite}
                >
                  <Plus className="ml-1 h-4 w-4" />
                  إضافة متطلب
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

        <DialogFooter className="border-t p-6 pt-4">
          <Button variant="outline" onClick={onClose}>
            إلغاء
          </Button>
          <Button onClick={handleSubmit} disabled={!isValid || isSubmitting}>
            {isSubmitting ? 'جاري الحفظ...' : 'حفظ البرنامج'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
