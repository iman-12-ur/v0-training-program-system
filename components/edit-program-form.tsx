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
import { categories } from '@/lib/mock-data';
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
    category: '',
    duration: '',
    instructor: '',
    location: '',
    status: 'active' as TrainingProgram['status'],
  });

  useEffect(() => {
    if (program) {
      setFormData({
        title: program.title,
        description: program.description,
        category: program.category,
        duration: program.duration,
        instructor: program.instructor,
        location: program.location,
        status: program.status,
      });
    }
  }, [program]);

  const handleSubmit = () => {
    if (!program) return;
    
    const updatedProgram: TrainingProgram = {
      ...program,
      title: formData.title,
      description: formData.description,
      category: formData.category,
      duration: formData.duration,
      instructor: formData.instructor,
      location: formData.location,
      status: formData.status,
    };

    onSubmit(updatedProgram);
    onClose();
  };

  const programCategories = categories.filter((c) => c !== 'الكل');

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>تعديل البرنامج التدريبي</DialogTitle>
        </DialogHeader>

        <div className="space-y-6 py-4">
          <div className="space-y-2">
            <Label htmlFor="edit-title">اسم البرنامج</Label>
            <Input
              id="edit-title"
              value={formData.title}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, title: e.target.value }))
              }
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="edit-description">وصف البرنامج</Label>
            <Textarea
              id="edit-description"
              value={formData.description}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, description: e.target.value }))
              }
              rows={3}
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label>التصنيف</Label>
              <Select
                value={formData.category}
                onValueChange={(value) =>
                  setFormData((prev) => ({ ...prev, category: value }))
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="اختر التصنيف" />
                </SelectTrigger>
                <SelectContent>
                  {programCategories.map((cat) => (
                    <SelectItem key={cat} value={cat}>
                      {cat}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>حالة البرنامج</Label>
              <Select
                value={formData.status}
                onValueChange={(value: TrainingProgram['status']) =>
                  setFormData((prev) => ({ ...prev, status: value }))
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="active">نشط</SelectItem>
                  <SelectItem value="inactive">غير نشط</SelectItem>
                  <SelectItem value="draft">مسودة</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="edit-duration">المدة</Label>
              <Input
                id="edit-duration"
                value={formData.duration}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, duration: e.target.value }))
                }
                placeholder="مثال: 16 ساعة"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="edit-instructor">المدرب</Label>
              <Input
                id="edit-instructor"
                value={formData.instructor}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, instructor: e.target.value }))
                }
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="edit-location">الموقع</Label>
            <Input
              id="edit-location"
              value={formData.location}
              onChange={(e) =>
                setFormData((prev) => ({ ...prev, location: e.target.value }))
              }
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            إلغاء
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={!formData.title || !formData.category}
          >
            حفظ التغييرات
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
