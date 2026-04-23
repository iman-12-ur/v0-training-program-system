'use client';

import { Card, CardContent, CardFooter, CardHeader } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Calendar,
  Clock,
  MapPin,
  User,
  Users,
  ChevronLeft,
  MoreVertical,
  Edit,
  Trash2,
} from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { TrainingProgram } from '@/lib/types';

interface ProgramCardProps {
  program: TrainingProgram;
  onViewDetails: (program: TrainingProgram) => void;
  onRegister?: (program: TrainingProgram) => void;
  onEdit?: (program: TrainingProgram) => void;
  onDelete?: (program: TrainingProgram) => void;
  isAdmin?: boolean;
}

export function ProgramCard({ program, onViewDetails, onRegister, onEdit, onDelete, isAdmin }: ProgramCardProps) {
  const upcomingBatches = program.batches.filter((b) => b.status === 'upcoming');
  const hasAvailableSlots = upcomingBatches.some(
    (b) => b.currentParticipants < b.maxParticipants
  );

  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'القيادة والإدارة':
        return 'bg-blue-100 text-blue-700';
      case 'إدارة المشاريع':
        return 'bg-emerald-100 text-emerald-700';
      case 'المهارات الشخصية':
        return 'bg-violet-100 text-violet-700';
      case 'تقنية المعلومات':
        return 'bg-amber-100 text-amber-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  };

  return (
    <Card className="group flex flex-col overflow-hidden border-none shadow-sm transition-all hover:shadow-lg">
      <CardHeader className="space-y-3 pb-3">
        <div className="flex items-start justify-between gap-2">
          <div className="flex flex-wrap items-center gap-2">
            {program.categories?.map((cat) => (
              <Badge key={cat} variant="secondary" className={getCategoryColor(cat)}>
                {cat}
              </Badge>
            ))}
            {program.status === 'active' && hasAvailableSlots && (
              <Badge className="bg-emerald-500 hover:bg-emerald-600">متاح للتسجيل</Badge>
            )}
            {program.status === 'draft' && (
              <Badge variant="outline" className="border-amber-500 text-amber-600">
                مسودة
              </Badge>
            )}
            {program.status === 'inactive' && (
              <Badge variant="outline" className="border-red-500 text-red-600">
                غير نشط
              </Badge>
            )}
          </div>
          {isAdmin && (onEdit || onDelete) && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon" className="h-8 w-8">
                  <MoreVertical className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                {onEdit && (
                  <DropdownMenuItem onClick={() => onEdit(program)}>
                    <Edit className="ml-2 h-4 w-4" />
                    تعديل البرنامج
                  </DropdownMenuItem>
                )}
                {onDelete && (
                  <DropdownMenuItem
                    className="text-red-600"
                    onClick={() => onDelete(program)}
                  >
                    <Trash2 className="ml-2 h-4 w-4" />
                    حذف البرنامج
                  </DropdownMenuItem>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>
        <h3 className="text-xl font-bold text-foreground leading-tight">
          {program.title}
        </h3>
      </CardHeader>

      <CardContent className="flex-1 space-y-4 pb-4">
        <div className="flex items-start gap-3">
          {program.logo && (
            <img
              src={program.logo}
              alt="شعار البرنامج"
              className="h-12 w-12 rounded-lg object-contain border bg-white shrink-0"
            />
          )}
          <div className="flex-1">
            <p className="text-sm text-muted-foreground line-clamp-2">
              {program.description}
            </p>
            {program.targetAudience && (
              <p className="mt-1 text-xs text-primary font-medium">
                الفئة المستهدفة: {program.targetAudience}
              </p>
            )}
          </div>
        </div>

        <div className="grid gap-2 text-sm">
          <div className="flex items-center gap-2 text-muted-foreground">
            <Clock className="h-4 w-4" />
            <span>{program.duration}</span>
          </div>
          <div className="flex items-center gap-2 text-muted-foreground">
            <User className="h-4 w-4" />
            <span>{program.instructor}</span>
          </div>
          <div className="flex items-center gap-2 text-muted-foreground">
            <MapPin className="h-4 w-4" />
            <span>{program.location}</span>
          </div>
        </div>

        {upcomingBatches.length > 0 && (
          <div className="rounded-lg bg-muted/50 p-3">
            <p className="mb-2 text-xs font-medium text-muted-foreground">
              الدفعات القادمة ({upcomingBatches.length})
            </p>
            <div className="space-y-2">
              {upcomingBatches.slice(0, 2).map((batch) => (
                <div
                  key={batch.id}
                  className="flex items-center justify-between text-sm"
                >
                  <div className="flex items-center gap-2">
                    <Calendar className="h-3.5 w-3.5 text-primary" />
                    <span className="text-foreground">{batch.name}</span>
                  </div>
                  <div className="flex items-center gap-1 text-muted-foreground">
                    <Users className="h-3.5 w-3.5" />
                    <span>
                      {batch.currentParticipants}/{batch.maxParticipants}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </CardContent>

      <CardFooter className="gap-2 border-t bg-muted/30 pt-4">
        <Button
          variant="outline"
          className="flex-1"
          onClick={() => onViewDetails(program)}
        >
          التفاصيل
          <ChevronLeft className="mr-1 h-4 w-4" />
        </Button>
        {!isAdmin && onRegister && hasAvailableSlots && program.status === 'active' && (
          <Button className="flex-1" onClick={() => onRegister(program)}>
            طلب ترشيح
          </Button>
        )}
      </CardFooter>
    </Card>
  );
}
