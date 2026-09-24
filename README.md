# نظام إدارة البرامج التدريبية | Training Program System

مستودع يضم مكوّنين مستقلّين. **البرنامج الفعلي هو تطبيق .NET MVC** الموجود في مجلد `TrainingSystemNet/`.

---

## أين أفتح لتشغيل / رفع البرنامج؟

البرنامج مكتوب بلغة **.NET MVC** (وليس Next.js). لتشغيله أو رفعه:

| الأداة | الملف الذي تفتحه |
|---|---|
| **Visual Studio** | `TrainingSystemNet/TrainingSystem.sln` |
| **الطرفية (Terminal)** | `cd TrainingSystemNet/TrainingSystem` ثم `dotnet run` |
| **النشر / الرفع** | `cd TrainingSystemNet/TrainingSystem` ثم `dotnet publish -c Release` — ثم ارفع محتوى مجلد `bin/Release/net*/publish/` |

> بعد التشغيل يفتح المتصفح على العنوان الذي يعرضه `dotnet run` (عادةً `https://localhost:5xxx`).

---

## بنية المستودع

```
v0-training-program-system/
│
├── TrainingSystemNet/          ★ البرنامج الفعلي (.NET MVC) — افتح هذا
│   └── TrainingSystem/
│       ├── TrainingSystem.sln   ← ملف الحل (Visual Studio)
│       ├── Controllers/         منطق التطبيق (14 وحدة تحكم)
│       ├── Models/              الكيانات (13 كياناً)
│       ├── Views/               واجهات Razor (عربي RTL / Bootstrap 5.3.2)
│       ├── Data/                ApplicationDbContext
│       ├── Services/            الإشعارات + مؤشرات الأداء
│       └── wwwroot/             CSS / JS / الأصول
│
├── docs/                       الوثائق (هيكل المشروع + خريطة ERD بصيغتَي PDF و Word)
│
├── app/ · components/ · lib/ · hooks/ · styles/ · public/
│                               تطبيق معاينة Next.js (خاص بـ v0 فقط — ليس البرنامج)
│
└── _archive/                   أرشيف النسخ القديمة وملفات ZIP السابقة (غير مستخدَم)
```

---

## الوثائق

- `docs/هيكلة-المشروع.md` — وثيقة الهيكلة الكاملة (نصّية، سهلة التحديث).
- `docs/نظام-التدريب-الهيكل-وERD.pdf` — الهيكل الكامل وخريطة ERD (PDF).
- `docs/نظام-التدريب-الهيكل-وERD.docx` — نفس المحتوى بصيغة Word قابلة للتحرير.

---

## ملاحظات

- مجلد `_archive/` يحوي أرشيفات ونسخاً **قديمة** متجاوَزة، محفوظة للرجوع فقط. لا تستخدمها للتشغيل.
- تطبيق Next.js في الجذر هو سطح معاينة v0 ولا علاقة له بمنطق نظام التدريب.
